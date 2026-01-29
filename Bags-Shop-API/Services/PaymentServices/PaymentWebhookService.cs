using Bags_Shop_API.Models;
using Bags_Shop_API.Services.OrderServices;
using Bags_Shop_API.Services.PaymentServices;
using Bags_Shop_API.Services.PaymentServices.Dtos;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ApplicationLayer.Services.PaymentWebhookService
{
    public interface IPaymentWebhookService
    {
        Task<bool> HandlePaymobAsync(PaymobWebhookDto dto, string receivedHmac);
    }

    public class PaymentWebhookService : IPaymentWebhookService
    {
        private readonly IErrorNotificationService _errorNotificationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentWebhookService> _logger;
        private readonly IPaymentServices _paymentServices;
        private readonly IOrderServices _orderServices;
        private readonly IConfiguration _configuration;

        private static readonly string[] HmacFieldsOrder = new[]
        {
            "amount_cents", "created_at", "currency", "error_occured", "has_parent_transaction",
            "id", "integration_id", "is_3d_secure", "is_auth", "is_capture", "is_refunded",
            "is_standalone_payment", "is_voided", "order.id", "owner", "pending",
            "source_data.pan", "source_data.sub_type", "source_data.type", "success"
        };

        public PaymentWebhookService(
            IErrorNotificationService errorNotificationService,
            IOrderServices orderServices,
            IPaymentServices paymentServices,
            IUnitOfWork unitOfWork,
            ILogger<PaymentWebhookService> logger,
            IConfiguration configuration)
        {
            _errorNotificationService = errorNotificationService;
            _orderServices = orderServices;
            _paymentServices = paymentServices;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> HandlePaymobAsync(PaymobWebhookDto dto, string receivedHmac)
        {
            _logger.LogInformation("Received Paymob webhook: {@WebhookDto}", dto);

            if (dto?.Obj == null)
            {
                _logger.LogWarning("Paymob webhook has null Obj payload. Ignored.");
                return false;
            }
            var obj = JObject.FromObject(dto.Obj);

       
            string? secretKey = _configuration["Security:Paymob:HMAC"];
            _logger.LogInformation(secretKey);
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogError("Paymob HMAC secret key not configured");
                return false;
            }

            bool isValid = VerifyPaymobHmac(obj, receivedHmac, secretKey);
            if (!isValid)
            {
                _logger.LogError("HMAC validation failed for Paymob webhook TxnId={TxnId}", dto.Obj.Id);
                return false;
            }


            try
            {
                var webhookResult = await ProcessWebhookData(dto);
                if (!webhookResult.Success)
                {

                    return false;
                }
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully processed Paymob webhook for Order {OrderId}", webhookResult.OrderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while handling Paymob webhook");
                _ = _errorNotificationService.SendErrorNotificationAsync(ex.Message);
                return false;
            }
        }

        private async Task<(bool Success, int OrderId)> ProcessWebhookData(PaymobWebhookDto dto)
        {
            var transaction = dto.Obj;

            var webhook = new PaymentWebhook
            {
                TransactionId = transaction.Id,
                OrderId = 0,
                PaymentMethod = GetPaymentMethod(transaction),
                Success = transaction.Success,
                Status = transaction.Success ? "Approved" : "Declined",
                AmountCents = transaction.AmountCents,
                Currency = transaction.Currency ?? "EGP",
                SourceSubType = transaction.SourceData?.SubType,
                SourceIssuer = dto.IssuerBank,
                CardLast4 = transaction.SourceData?.PanLast4,
                PaymentProvider = "PayMob",
                ProviderOrderId = transaction.Order?.Id.ToString(),
                HmacVerified = true,
                AuthorizationCode = ExtractAuthorizationCode(transaction),
                ReceiptNumber = ExtractReceiptNumber(transaction),
                Is3DSecure = string.Equals(transaction.SourceData?.Type, "card", StringComparison.OrdinalIgnoreCase),
                IsCapture = false,
                IsVoided = false,
                IsRefunded = false,
                IntegrationId = transaction.PaymentKeyClaims?.IntegrationId.ToString(),
                ProfileId = transaction.PaymentKeyClaims?.UserId.ToString(),
                ProcessedAt = DateTime.UtcNow,
                RetryCount = 0,
                WebhookUniqueKey = $"{transaction.Id}_{transaction.Order?.Id}_{transaction.AmountCents}"
            };

            if (transaction.Order != null)
            {
                _logger.LogInformation("Paymob order extra: PaidAmountCents={PaidAmountCents}, PaymentStatus={PaymentStatus}",
                    transaction.Order.PaidAmountCents, transaction.Order.PaymentStatus);
                webhook.PaymobOrderId = transaction.Order.Id;
            }

            int localOrderId = ExtractAndValidateOrderId(transaction);
            if (localOrderId > 0)
            {
                webhook.OrderId = localOrderId;
            }

            //bool isExist = await _unitOfWork.Repository<PaymentWebhook>()
            //    .GetAll()
            //    .AnyAsync(w => w.WebhookUniqueKey == webhook.WebhookUniqueKey);

            //if (isExist)
            //{
            //    _logger.LogWarning("Duplicate webhook detected with key: {WebhookUniqueKey}", webhook.WebhookUniqueKey);
            //    return (true, localOrderId);
            //}

            await _unitOfWork.Webhook.AddAsync(webhook);

            if (localOrderId <= 0)
            {
                _logger.LogWarning("Webhook could not be linked to a local order. TxnId: {TxnId}", transaction.Id);
                return (true, 0);
            }


            var updateResult = await UpdatePaymentAndOrderStatus(transaction, localOrderId);
            if (!updateResult.Success)
            {
                return (false, localOrderId);
            }
            await _unitOfWork.SaveChangesAsync();
            webhook.PaymentId = updateResult.PaymentId;
            return (true, localOrderId);
        }

        private int ExtractAndValidateOrderId(PaymobTransactionObj transaction)
        {
            var specialReference = transaction.PaymentKeyClaims?.Extra?.SpecialReference;

            if (string.IsNullOrWhiteSpace(specialReference))
                return 0;

            if (int.TryParse(specialReference, out int orderId))
                return orderId;

            return 0;
        }


        private bool VerifyPaymobHmac(JObject obj, string receivedHmac, string secretKey)
        {
            try
            {
                var sb = new StringBuilder();
                _logger.LogInformation("=== HMAC Field Extraction ===");

                foreach (var field in HmacFieldsOrder)
                {
                    string[] parts = field.Split('.');
                    JToken? current = obj;

                    foreach (var part in parts)
                    {
                        if (current == null || current.Type == JTokenType.Null)
                        {
                            current = null;
                            break;
                        }
                        current = current[part];
                    }

                    string fieldValue = "";
                    if (current == null || current.Type == JTokenType.Null)
                    {
                        fieldValue = "";
                    }
                    else if (current.Type == JTokenType.Boolean)
                    {
                        fieldValue = current.Value<bool>() ? "true" : "false";
                    }
                    else
                    {
                        fieldValue = current.ToString();
                    }

                    sb.Append(fieldValue);
                    _logger.LogDebug("HMAC Field '{Field}': '{Value}' (Type: {Type})",
                        field,
                        fieldValue,
                        current?.Type.ToString() ?? "null");
                }

                var dataToHash = sb.ToString();
                _logger.LogInformation("Final HMAC string: '{DataToHash}'", dataToHash);
                _logger.LogInformation("HMAC string length: {Length}", dataToHash.Length);

                using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
                var calculatedHmac = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                _logger.LogInformation("Calculated HMAC: {CalculatedHmac}", calculatedHmac);
                _logger.LogInformation("Received HMAC: {ReceivedHmac}", receivedHmac);

                bool isValid = string.Equals(calculatedHmac, receivedHmac, StringComparison.OrdinalIgnoreCase);
                _logger.LogInformation("HMAC validation result: {IsValid}", isValid);

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating HMAC for Paymob webhook");
                return false;
            }
        }

        private async Task<(bool Success, int? PaymentId)> UpdatePaymentAndOrderStatus(PaymobTransactionObj transaction, int localOrderId)
        {
            PaymentStatus status = PaymentStatus.Failed;
            OrderStatus orderStatus = OrderStatus.Pending;

            if (transaction.Pending)
            {
                status = PaymentStatus.Pending;
            }
            else if (transaction.Success)
            {
                status = PaymentStatus.Completed;
                orderStatus = OrderStatus.Paid;
            }

            var paymentResult = await _paymentServices.UpdatePaymentAfterPaid(
                localOrderId,
                transaction.Id.ToString(),
                transaction.PaymentKeyClaims?.OrderId ?? 0,
                status,
                transaction.SourceData?.Type);

            if (!paymentResult.Success)
            {
                _logger.LogError("Failed to update payment for order {OrderId}", localOrderId);
                return (false, null);
            }

            var orderUpdateResult = await _orderServices.UpdateOrderAfterPaid(localOrderId, orderStatus);
            if (!orderUpdateResult.Success)
            {
                _logger.LogError("Failed to update order status for order {OrderId}", localOrderId);
                return (false, paymentResult.Data);
            }

            return (true, paymentResult.Data);
        }



        private string? ExtractAuthorizationCode(PaymobTransactionObj obj) =>
            obj.SourceData?.SubType == "card" ? $"AUTH_{obj.Id}" : null;

        private string? ExtractReceiptNumber(PaymobTransactionObj obj) =>
            obj.SourceData?.SubType == "card" ? $"RECEIPT_{obj.Id}" : null;

        private string GetPaymentMethod(PaymobTransactionObj transaction)
        {
            var type = transaction.SourceData?.Type;
            return !string.IsNullOrWhiteSpace(type) ? type : "Unknown";
        }
    }
}


