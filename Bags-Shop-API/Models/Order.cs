using System.ComponentModel.DataAnnotations;

namespace Bags_Shop_API.Models
{
	public class Order
	{
		[Key]
		public int Id { get; set; }
        [Required(ErrorMessage = "Address Is Required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Address Must Be Between 3 and 100 Characters")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Phone Is Required")]
        [RegularExpression(@"^(010|011|012|015)\d{8}$",
        ErrorMessage = "Phone Must Be Valid Egyptian Phone Number")]

        public string Phone { get; set; }
		public OrderStatus Status { get; set; }
		public ICollection<OrderItem> OrderItems { get; set; }
		public ICollection<Payment>   Payments { get; set; }
		[Required]
		
		public decimal FinalPrice { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(2);

		public string? Name { get; set; }
		public string? Userkey { get; set; }


	}
}
