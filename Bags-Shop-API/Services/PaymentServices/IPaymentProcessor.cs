using Bags_Shop_API.Models;
using Bags_Shop_API.Services.Shared;

namespace Bags_Shop_API.Services.PaymentServices
{
    public interface IPaymentProcessor
    {
        Task<Result<PaymentLinkResult>> GetPaymentLinkAsync(CreatePaymentDto dto, int expires);
        Task<Result<PaymobPaymentStatusDto>> GetPaymentStatusAsync(long orderId);
    }

    public class CreatePaymentDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public string? Notes { get; set; }
        public int Ordernumber { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? WalletPhoneNumber { get; set; }
        public string? BillingAddress { get; set; }
        public string? BillingPhone { get; set; }
    }

    public class PaymentLinkResult
    {
        public string PaymentUrl { get; set; } = string.Empty;
        public long PaymobOrderId { get; set; }
    }

    public class PaymobPaymentStatusDto
    {
        public string Status { get; set; } = "Unpaid"; // Paid, Pending, Unpaid
        public int PaidAmountCents { get; set; }
        public string Currency { get; set; } = "EGP";
    }
}
