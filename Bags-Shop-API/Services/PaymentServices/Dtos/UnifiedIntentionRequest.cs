namespace Bags_Shop_API.Services.PaymentServices.Dtos.PaymobIntegration.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class UnifiedIntentionRequest
	{
		[JsonPropertyName("amount")]
		public int Amount { get; set; } // Amount in cents

		[JsonPropertyName("currency")]
		public string Currency { get; set; } = "EGP";

		[JsonPropertyName("payment_methods")]
		public List<int> PaymentMethods { get; set; } = new List<int>();

		[JsonPropertyName("items")]
		public List<Item>? Items { get; set; }

		[JsonPropertyName("billing_data")]
		public billing_data BillingData { get; set; } = new billing_data();

		[JsonPropertyName("extras")]
		public Dictionary<string, object>? Extras { get; set; }

		[JsonPropertyName("special_reference")]
		public string? SpecialReference { get; set; }

		[JsonPropertyName("expiration")]
		public int Expiration { get; set; } = 3600;

		[JsonPropertyName("notification_url")]
		public string? NotificationUrl { get; set; }

		[JsonPropertyName("redirection_url")]
		public string? RedirectionUrl { get; set; }
	}

	public class Item
	{
		[JsonPropertyName("name")]
		public string? Name { get; set; }

		[JsonPropertyName("amount")]
		public int Amount { get; set; } // Amount in cents

		[JsonPropertyName("description")]
		public string? Description { get; set; }

		[JsonPropertyName("quantity")]
		public int Quantity { get; set; } = 1;
	}
}
