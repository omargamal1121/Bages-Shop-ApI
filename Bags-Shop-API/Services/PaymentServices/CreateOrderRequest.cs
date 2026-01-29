namespace Bags_Shop_API.Services.PaymentServices
{

    public partial class PayMobServices
	{
		public class CreateOrderRequest { public bool delivery_needed { get; set; } public decimal amount_cents { get; set; } public string currency { get; set; } = "EGP"; public string auth_token { get; set; } = string.Empty; public int? merchant_order_id { get; set; } }
    }
}
