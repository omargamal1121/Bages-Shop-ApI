using System.Text.Json.Serialization;

namespace Bags_Shop_API.Services.PaymentServices
{
	public class IntentionApiResponse
    {
        [JsonPropertyName("client_secret")]
        public string? ClientSecret { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; } // "pi_..."

        [JsonPropertyName("intention_order_id")]
        public long IntentionOrderId { get; set; }
    }
}
