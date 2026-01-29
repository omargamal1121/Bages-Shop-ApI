using Bags_Shop_API.Models;

namespace Bags_Shop_API.Services.PaymentServices.Dtos
{
    public class PaymentDetailsDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public PaymentStatus Status { get; set; }
        public PaymentMethod Method { get; set; }
        public string? TransactionId { get; set; }
        public long ProviderOrderId { get; set; }
        public string? PaymentLink { get; set; }
        public string? PaymentIntentionId { get; set; }
        public DateTime? PaymentLinkExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
