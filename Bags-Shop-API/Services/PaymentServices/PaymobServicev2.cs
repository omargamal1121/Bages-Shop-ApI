using Bags_Shop_API.Models;
using Bags_Shop_API.Services.PaymentServices.Dtos.PaymobIntegration.Models;
using Bags_Shop_API.Services.Shared;
using Hangfire;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

using Bags_Shop_API.UnitOfWorkService;

namespace Bags_Shop_API.Services.PaymentServices
{
    public class PaymobServicev2 : IPaymentProcessor, IPayMobServices
	{
        private readonly ILogger<PaymobServicev2> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IErrorNotificationService _errorNotificationService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IUnitOfWork _unitOfWork;
        private static readonly object _tokenLock = new object();
        private static string _token = string.Empty;
        private static DateTime _tokenGeneratedAt = DateTime.MinValue;

        public PaymobServicev2(
            IConfiguration configuration,
            ILogger<PaymobServicev2> logger,
            IErrorNotificationService errorNotificationService,
            IBackgroundJobClient backgroundJobClient,
            IUnitOfWork unitOfWork,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _errorNotificationService = errorNotificationService;
            _backgroundJobClient = backgroundJobClient;
            _unitOfWork = unitOfWork;
            _httpClient = httpClient;
        }

       
        public async Task<Result<PaymentLinkResult>> GetPaymentLinkAsync(CreatePaymentDto createPayment, int expiresArg, long? orderproviderId = null, int? paymentid = null)
        {
            int expires = 86400; // Force 24 hours

            try
            {
                var secretKey = _configuration.GetValue<string>("Security:Paymob:Api:secretKey") 
                                ?? _configuration.GetValue<string>("Security:Paymob:SecretKey");
                var publicKeyConfig = _configuration.GetValue<string>("Security:Paymob:PublicKey");

                if (string.IsNullOrEmpty(secretKey))
                {
                    _logger.LogError("Paymob Secret Key is missing in configuration.");
                    return Result<PaymentLinkResult>.Fail("Payment configuration error (Secret Key missing).", 500);
                }

                if (string.IsNullOrEmpty(publicKeyConfig))
                {
                    _logger.LogWarning("Paymob Public Key is missing in configuration. Unified Checkout URL will be incomplete.");
                    return Result<PaymentLinkResult>.Fail("Payment configuration error (public Key missing).", 500);
                }

                var paymentMethods = new List<int>();

                var card = _configuration.GetValue<int?>("Security:Paymob:IntegrationIds:CardPayment");
                if (card.HasValue) paymentMethods.Add(card.Value);

                var wallet = _configuration.GetValue<int?>("Security:Paymob:IntegrationIds:Wallet");
                if (wallet.HasValue) paymentMethods.Add(wallet.Value);

                if (paymentMethods.Count == 0)
                {
                    _logger.LogError("Payment method is empty");
                    return Result<PaymentLinkResult>.Fail("Sorry no payment method now.", 400);
                }

                var notificationUrl = _configuration.GetValue<string>("Security:Paymob:NotificationUrl");
                var redirectionUrl = _configuration.GetValue<string>("Security:Paymob:redirection_url");
                var specialReference = createPayment.Ordernumber.ToString();

                var request = new UnifiedIntentionRequest
                {
                    Amount = (int)(createPayment.Amount * 100),
                    Currency = createPayment.Currency ?? "EGP",
                    PaymentMethods = paymentMethods,
                    Items = new List<Item>(),
                    BillingData = new billing_data()
                    {
                        phone_number = createPayment.BillingPhone ?? "NA",
                        street = createPayment.BillingAddress ?? "NA"
                    },
                    SpecialReference = specialReference,
                    NotificationUrl = notificationUrl,
                    RedirectionUrl = redirectionUrl,
                    Expiration = expires,
                    Extras = new Dictionary<string, object>
                    {
                        {"special_reference", specialReference },
                        {"order_provider_id", orderproviderId ?? 0   },
                        {"PaymentId", paymentid ?? 0}
                    }
                };

                var jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                var jsonBody = JsonSerializer.Serialize(request, jsonOptions);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://accept.paymob.com/v1/intention");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Token", secretKey);
                requestMessage.Content = content;

                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Paymob Intention API Failed. Status: {Status}, Response: {Response}", response.StatusCode, responseContent);
                    return Result<PaymentLinkResult>.Fail($"Payment Provider Error: {response.StatusCode}");
                }

                var intentionResult = JsonSerializer.Deserialize<IntentionApiResponse>(responseContent);

                if (intentionResult == null || string.IsNullOrEmpty(intentionResult.ClientSecret))
                {
                    _logger.LogError("Failed to deserialize Paymob Intention response or ClientSecret missing. Response: {Response}", responseContent);
                    return Result<PaymentLinkResult>.Fail("Invalid response from Payment Provider.");
                }

                var unifiedUrl = $"https://accept.paymob.com/unifiedcheckout/?publicKey={publicKeyConfig}&clientSecret={intentionResult.ClientSecret}";

                var resultNew = new PaymentLinkResult
                {
                    ClientSecret = intentionResult.ClientSecret,
                    PublicKey = publicKeyConfig ?? "",
                    UnifiedCheckoutUrl = unifiedUrl,
                    IntentionId = intentionResult.Id ?? string.Empty,
                    PaymobOrderId = intentionResult.IntentionOrderId,
                    PaymentUrl = unifiedUrl
                };

                _logger.LogInformation("Successfully created Paymob Intention. ID: {Id}, OrderID: {OrderId}", intentionResult.Id, intentionResult.IntentionOrderId);

                return Result<PaymentLinkResult>.Ok(resultNew);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in CreatePaymentAsync");
                return Result<PaymentLinkResult>.Fail("An internal error occurred during payment initiation.", 500);
            }
        }

        public async Task<Result<PaymobPaymentStatusDto>> GetPaymentStatusAsync(long orderId)
        {
            var tokenResult = await GetTokenAsync();
            if (!tokenResult) return Result<PaymobPaymentStatusDto>.Fail("Failed to authenticate with Paymob");

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://accept.paymob.com/api/ecommerce/orders/{orderId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Paymob API call failed for order {OrderId}", orderId);
                    return Result<PaymobPaymentStatusDto>.Fail("Failed to retrieve payment status");
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                long paidAmount = 0;
                if (doc.RootElement.TryGetProperty("paid_amount_cents", out var paidEl))
                {
                    paidAmount = paidEl.GetInt64();
                }

                long totalAmount = 0;
                if (doc.RootElement.TryGetProperty("amount_cents", out var totalEl))
                {
                    totalAmount = totalEl.GetInt64();
                }

                var status = (paidAmount >= totalAmount && totalAmount > 0) ? "Paid" : "Unpaid";

                _logger.LogInformation("Successfully retrieved Paymob status for Order {OrderId}. Status: {Status}", orderId, status);

                return Result<PaymobPaymentStatusDto>.Ok(new PaymobPaymentStatusDto
                {
                    Status = status,
                    PaidAmountCents = (int)paidAmount,
                    Currency = "EGP" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting payment status for order {OrderId}", orderId);
                return Result<PaymobPaymentStatusDto>.Fail("Failed to retrieve payment status");
            }
        }

        private async Task<bool> GetTokenAsync()
        {
            lock (_tokenLock)
            {
                if (!string.IsNullOrEmpty(_token) && _tokenGeneratedAt.AddMinutes(55) > DateTime.UtcNow)
                {
                    return true;
                }
            }

            try
            {
                var apiKey = _configuration.GetValue<string>("Security:Paymob:ApiKey");

                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("PayMob API key not found in configuration at Security:Paymob:ApiKey");
                    return false;
                }

                var body = new { api_key = apiKey };
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://accept.paymob.com/api/auth/tokens", content);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("PayMob - Failed to retrieve auth token. Status: {StatusCode}, Response: {Error}", response.StatusCode, error);
                    return false;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                
  
                using var doc = JsonDocument.Parse(responseContent);
                if (doc.RootElement.TryGetProperty("token", out var tokenEl))
                {
                    lock (_tokenLock)
                    {
                        var newToken = tokenEl.GetString();
                        if (!string.IsNullOrEmpty(newToken))
                        {
                            _token = newToken;
                            _tokenGeneratedAt = DateTime.UtcNow;
                            return true;
                        }
                    }
                }
                
                _logger.LogError("Invalid token response from PayMob: {Response}", responseContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while retrieving PayMob token");
                return false;
            }
        }
    }
   
}
