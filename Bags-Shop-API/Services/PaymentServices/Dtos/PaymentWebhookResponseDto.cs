namespace Bags_Shop_API.Services.PaymentServices.Dtos
{
    public class PaymentWebhookResponseDto
    {
        public int Id { get; set; }
        public long TransactionId { get; set; }
        public long PaymobOrderId { get; set; }
        public int OrderId { get; set; }
        public int? PaymentId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal AmountCents { get; set; }
        public string Currency { get; set; } = "EGP";
        public string? SourceSubType { get; set; }
        public string? SourceIssuer { get; set; }
        public string? CardLast4 { get; set; }
        public string? PaymentProvider { get; set; }
        public string? RawData { get; set; }
        public bool HmacVerified { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
