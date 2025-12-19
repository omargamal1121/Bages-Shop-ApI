using Bags_Shop_API.Models;
using Bags_Shop_API.Services.OrderServices;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using Bags_Shop_API.Services.PaymentServices.Dtos;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Bags_Shop_API.Services.PaymentServices
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

            // Idempotency Check
            var spec = new WebhookByTransactionSpec(dto.Obj.Id);
            var isAlreadyProcessed = await _unitOfWork.Webhook.AnyAsync(spec);
            if (isAlreadyProcessed)
            {
                _logger.LogInformation("Paymob webhook with TxnId {TxnId} already processed. Skipping.", dto.Obj.Id);
                return true;
            }

            var obj = JObject.FromObject(dto.Obj);
            string? secretKey = _configuration["Security:Paymob:HMAC"];

            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogError("Paymob HMAC secret key not configured at Security:Paymob:HMAC");
                return false;
            }

            if (!VerifyPaymobHmac(obj, receivedHmac, secretKey))
            {
                _logger.LogError("HMAC validation failed for Paymob webhook TxnId={TxnId}. Content={Content}", dto.Obj.Id, obj.ToString());
                return false;
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var webhookResult = await ProcessWebhookData(dto);
                if (!webhookResult.Success)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Successfully processed Paymob webhook for Order {OrderId}, TxnId {TxnId}", webhookResult.OrderId, dto.Obj.Id);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error while handling Paymob webhook for TxnId {TxnId}", dto.Obj.Id);
                await _errorNotificationService.SendErrorNotificationAsync("Webhook processing error", ex.Message);
                return false;
            }
        }

        private async Task<(bool Success, int OrderId)> ProcessWebhookData(PaymobWebhookDto dto)
        {
            var transaction = dto.Obj!;
            var webhook = new PaymentWebhook
            {
                TransactionId = transaction.Id,
                PaymobOrderId = transaction.Order?.Id ?? 0,
                PaymentMethod = transaction.SourceData?.Type ?? "Unknown",
                Success = transaction.Success,
                Status = transaction.Success ? "Approved" : "Declined",
                AmountCents = (decimal)transaction.AmountCents,
                Currency = transaction.Currency ?? "EGP",
                SourceSubType = transaction.SourceData?.SubType,
                SourceIssuer = dto.IssuerBank,
                CardLast4 = transaction.SourceData?.PanLast4,
                PaymentProvider = "PayMob",
                ProviderOrderId = transaction.Order?.Id.ToString(),
                RawData = Newtonsoft.Json.JsonConvert.SerializeObject(dto),
                HmacVerified = true,
                ProcessedAt = DateTime.UtcNow,
                WebhookUniqueKey = $"{transaction.Id}_{transaction.Order?.Id}_{transaction.AmountCents}"
            };

            int localOrderId = await ExtractAndValidateOrderId(transaction);
            if (localOrderId > 0)
            {
                webhook.OrderId = localOrderId;
            }
            else
            {
                _logger.LogWarning("Webhook could not be linked to a local order. TxnId: {TxnId}", transaction.Id);
                await _unitOfWork.Webhook.AddAsync(webhook);
                return (true, 0);
            }

            var updateResult = await UpdatePaymentAndOrderStatus(transaction, localOrderId);
            webhook.PaymentId = updateResult.PaymentId;

            await _unitOfWork.Webhook.AddAsync(webhook);
            return (true, localOrderId);
        }

        private async Task<int> ExtractAndValidateOrderId(PaymobTransactionObj transaction)
        {
            var merchantOrderNumber = transaction.Order?.MerchantOrderId;
            if (string.IsNullOrWhiteSpace(merchantOrderNumber)) return 0;

            if (int.TryParse(merchantOrderNumber, out int orderId))
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                return order?.Id ?? 0;
            }
            return 0;
        }

        private bool VerifyPaymobHmac(JObject obj, string receivedHmac, string secretKey)
        {
            try
            {
                var sb = new StringBuilder();
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
                    if (current != null && current.Type != JTokenType.Null)
                    {
                        fieldValue = current.Type == JTokenType.Boolean ? (current.Value<bool>() ? "true" : "false") : current.ToString();
                    }
                    sb.Append(fieldValue);
                }

                using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                var calculatedHmac = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                return string.Equals(calculatedHmac, receivedHmac, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        private async Task<(bool Success, int? PaymentId)> UpdatePaymentAndOrderStatus(PaymobTransactionObj transaction, int localOrderId)
        {
            PaymentStatus status = transaction.Success ? PaymentStatus.Completed : (transaction.Pending ? PaymentStatus.Pending : PaymentStatus.Failed);
            OrderStatus orderStatus = transaction.Success ? OrderStatus.Paid : OrderStatus.Pending;

            var paymentResult = await _paymentServices.UpdatePaymentAfterPaid(localOrderId, transaction.Id.ToString(), transaction.Order?.Id ?? 0, status);
            if (!paymentResult.Success) return (false, null);

            var orderUpdateResult = await _orderServices.UpdateOrderAfterPaid(localOrderId, orderStatus);
            return (true, paymentResult.Data);
        }
    }
}
