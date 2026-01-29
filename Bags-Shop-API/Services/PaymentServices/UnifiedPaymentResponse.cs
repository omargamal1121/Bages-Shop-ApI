namespace Bags_Shop_API.Services.PaymentServices
{
	// Response Models
	public class UnifiedPaymentResponse
    {
        public string ClientSecret { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string UnifiedCheckoutUrl { get; set; } = string.Empty;
        public string IntentionId { get; set; } = string.Empty;
        public long IntentionOrderId { get; set; }
    }
}
