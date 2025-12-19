using Bags_Shop_API.Models;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using Hangfire;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Bags_Shop_API.Services.PaymentServices
{
    public interface IPayMobServices
    {
        Task<Result<PaymobPaymentStatusDto>> GetPaymentStatusAsync(long orderId);
        Task<Result<PaymentLinkResult>> GetPaymentLinkAsync(CreatePaymentDto dto, int expires);
    }

    public class PayMobServices : IPaymentProcessor, IPayMobServices
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<PayMobServices> _logger;
        private readonly IErrorNotificationService _errorNotificationService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private static readonly object _tokenLock = new object();
        private static string _token = string.Empty;
        private static DateTime _tokenGeneratedAt = DateTime.MinValue;

        public PayMobServices(
            IConfiguration configuration,
            ILogger<PayMobServices> logger,
            IErrorNotificationService errorNotificationService,
            IBackgroundJobClient backgroundJobClient,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _errorNotificationService = errorNotificationService;
            _backgroundJobClient = backgroundJobClient;
            _httpClient = httpClient;
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
                var result = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result == null || string.IsNullOrWhiteSpace(result.token))
                {
                    _logger.LogError("Invalid token response from PayMob");
                    return false;
                }

                lock (_tokenLock)
                {
                    _token = result.token;
                    _tokenGeneratedAt = DateTime.UtcNow;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while retrieving PayMob token");
                return false;
            }
        }

        public async Task<Result<PaymobPaymentStatusDto>> GetPaymentStatusAsync(long orderId)
        {
            var tokenResult = await GetTokenAsync();
            if (!tokenResult) return Result<PaymobPaymentStatusDto>.Fail("Failed to authenticate with Paymob");

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var response = await _httpClient.GetAsync($"https://accept.paymob.com/api/ecommerce/orders/{orderId}");

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

                var currency = doc.RootElement.TryGetProperty("currency", out var currencyEl) ? currencyEl.GetString() : "EGP";

                _logger.LogInformation("Successfully retrieved Paymob status for Order {OrderId}", orderId);

                return Result<PaymobPaymentStatusDto>.Ok(new PaymobPaymentStatusDto
                {
                    Status = (paidAmount >= totalAmount && totalAmount > 0) ? "Paid" : "Unpaid",
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting payment status for order {OrderId}", orderId);
                return Result<PaymobPaymentStatusDto>.Fail("Failed to retrieve payment status");
            }
        }

        public async Task<Result<PaymentLinkResult>> GetPaymentLinkAsync(CreatePaymentDto dto, int expires)
        {
            if (dto == null) return Result<PaymentLinkResult>.Fail("Invalid payment request", 400);

            if (dto.WalletPhoneNumber == null && dto.PaymentMethod == PaymentMethod.Wallet)
            {
                return Result<PaymentLinkResult>.Fail("Wallet Phone Number Needed", 400);
            }

            try
            {
                var tokenResult = await GetTokenAsync();
                if (!tokenResult) return Result<PaymentLinkResult>.Fail("Authentication failed", 401);

                var paymobOrderRequest = new CreateOrderRequest
                {
                    auth_token = _token,
                    amount_cents = (int)(dto.Amount * 100),
                    currency = "EGP",
                    delivery_needed = true,
                    merchant_order_id = dto.Ordernumber
                };

                var paymobOrderId = await CreateOrderInPaymobAsync(paymobOrderRequest);
                if (paymobOrderId == 0) return Result<PaymentLinkResult>.Fail("Failed to create payment order", 500);

                string integrationId = dto.PaymentMethod switch
                {
                    PaymentMethod.CardPayment => _configuration.GetValue<string>("Security:Paymob:IntegrationIds:CardPayment") ?? "",
                    PaymentMethod.Wallet => _configuration.GetValue<string>("Security:Paymob:IntegrationIds:Wallte") ?? "",
                    _ => ""
                };

                if (string.IsNullOrEmpty(integrationId))
                {
                    _logger.LogError("Integration ID not found for payment method: {PaymentMethod}", dto.PaymentMethod);
                    return Result<PaymentLinkResult>.Fail("Payment method not configured", 400);
                }

                string redirectionUrl = _configuration["Security:Paymob:redirection_url"] ?? "";

                var paymentKeyRequest = new PaymentKeyContent
                {
                    amount_cents = (int)(dto.Amount * 100),
                    auth_token = _token,
                    expiration = expires,
                    order_id = paymobOrderId,
                    integration_id = integrationId,
                    redirection_url = redirectionUrl,
                    billing_data = new billing_data()
                };

                var paymentKey = await GeneratePaymentKeyAsync(paymentKeyRequest);
                if (string.IsNullOrEmpty(paymentKey)) return Result<PaymentLinkResult>.Fail("Failed to generate payment key", 500);

                string paymentUrl = dto.PaymentMethod == PaymentMethod.Wallet
                    ? await WalletUrl(paymentKey, dto.WalletPhoneNumber!)
                    : await OnlineCardUrl(paymentKey);

                return Result<PaymentLinkResult>.Ok(new PaymentLinkResult { PaymentUrl = paymentUrl, PaymobOrderId = paymobOrderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while generating payment link");
                return Result<PaymentLinkResult>.Fail("Failed to initiate payment", 500);
            }
        }

        private async Task<int> CreateOrderInPaymobAsync(CreateOrderRequest order)
        {
            try
            {
                var json = JsonSerializer.Serialize(order);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://accept.paymob.com/api/ecommerce/orders", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Paymob Order creation failed. Status: {StatusCode}", response.StatusCode);
                    return 0;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseContent = JsonSerializer.Deserialize<CreateOrderResponse>(responseJson);
                
                if (responseContent?.id != null)
                {
                    _logger.LogInformation("Successfully created Paymob Order with ID {PaymobOrderId}", responseContent.id);
                }

                return responseContent?.id ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Paymob Order creation");
                return 0; 
            }
        }

        private async Task<string?> GeneratePaymentKeyAsync(PaymentKeyContent content)
        {
            try
            {
                var json = JsonSerializer.Serialize(content);
                var requestBody = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://accept.paymob.com/api/acceptance/payment_keys", requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Paymob Payment Key generation failed. Status: {StatusCode}", response.StatusCode);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (result != null && !string.IsNullOrEmpty(result.token))
                {
                    _logger.LogInformation("Successfully generated Paymob Payment Key");
                }

                return result?.token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Paymob Payment Key generation");
                return null; 
            }
        }

        private async Task<string> WalletUrl(string paymentKey, string phoneNumber)
        {
            var walletRequest = new { source = new { identifier = phoneNumber, subtype = "WALLET" }, payment_token = paymentKey };
            var response = await _httpClient.PostAsync("https://accept.paymob.com/api/acceptance/payments/pay", 
                new StringContent(JsonSerializer.Serialize(walletRequest), Encoding.UTF8, "application/json"));
            
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Paymob Wallet redirection failed. Status: {StatusCode}, Response: {Response}", response.StatusCode, content);
                throw new Exception($"Paymob Wallet redirection failed with status {response.StatusCode}");
            }

            var payResult = JsonSerializer.Deserialize<PaymobWalletResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (string.IsNullOrEmpty(payResult?.redirect_url))
            {
                _logger.LogError("Wallet redirect_url not found in response: {Response}", content);
                throw new Exception("Wallet redirect_url not found");
            }

            _logger.LogInformation("Successfully obtained Paymob Wallet redirect URL");
            return payResult.redirect_url;
        }

        private async Task<string> OnlineCardUrl(string paymentKey)
        {
            var iframeId = _configuration.GetValue<string>("Security:Paymob:IframeId") ?? "0";
            return $"https://accept.paymob.com/api/acceptance/iframes/{iframeId}?payment_token={paymentKey}";
        }

        private class TokenResponse { public string token { get; set; } = string.Empty; }
        private class CreateOrderRequest { public bool delivery_needed { get; set; } public decimal amount_cents { get; set; } public string currency { get; set; } = "EGP"; public string auth_token { get; set; } = string.Empty; public int? merchant_order_id { get; set; } }
        private class CreateOrderResponse { public int id { get; set; } }
        private class PaymentKeyContent { public string currency { get; set; } = "EGP"; public string auth_token { get; set; } = string.Empty; public decimal amount_cents { get; set; } public int expiration { get; set; } = 1000; public int order_id { get; set; } public string integration_id { get; set; } = string.Empty; public string redirection_url { get; set; } = string.Empty; public billing_data billing_data { get; set; } = new billing_data(); }
        private class billing_data { public string first_name { get; set; } = "NA"; public string last_name { get; set; } = "NA"; public string email { get; set; } = "NA"; public string phone_number { get; set; } = "NA"; public string apartment { get; set; } = "NA"; public string floor { get; set; } = "NA"; public string street { get; set; } = "NA"; public string building { get; set; } = "NA"; public string shipping_method { get; set; } = "NA"; public string postal_code { get; set; } = "NA"; public string city { get; set; } = "NA"; public string country { get; set; } = "EG"; public string state { get; set; } = "NA"; }
        private class PaymobWalletResponse { public string? redirect_url { get; set; } }
    }
}
