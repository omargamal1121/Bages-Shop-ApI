namespace Bags_Shop_API.Services.PaymentServices
{

		public class PaymentKeyContent { public string currency { get; set; } = "EGP"; public string auth_token { get; set; } = string.Empty; public decimal amount_cents { get; set; } public int expiration { get; set; } = 1000; public int order_id { get; set; } public string integration_id { get; set; } = string.Empty; public string redirection_url { get; set; } = string.Empty; public billing_data billing_data { get; set; } = new billing_data(); }
 
}
