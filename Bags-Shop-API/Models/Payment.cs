using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bags_Shop_API.Models
{
	public class Payment
	{
		[Key]
		public int Id { get; set; }
		[ForeignKey("Order")]
		public int OrderId { get; set; }
		public Order  Order { get; set; }
		public PaymentMethod Method { get; set; }
		public PaymentStatus Status { get; set; }
        
        // Added for Paymob Integration
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public long ProviderOrderId { get; set; } // Paymob Order ID
        public string? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }

        public ICollection<PaymentWebhook>  PaymentWebhooks { get; set; }

	}
}
