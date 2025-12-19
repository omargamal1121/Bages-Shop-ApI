using System.ComponentModel.DataAnnotations;

namespace Bags_Shop_API.Models
{
	public class Order
	{
		[Key]
		public int Id { get; set; }
		[Required(ErrorMessage = "Address Is Required")]
		[Range(10,100, ErrorMessage = "Address Must Be Between 3 and 100 Characters")]
		public string Address { get; set; }
		[Required(ErrorMessage = "Phone Is Required")]
		[RegularExpression(@"^(012|011|015|010)[0-9][8]")]
		[Range(10, 11, ErrorMessage = "Phone Must Be Valid Egyptian Phone Number")]

		public string Phone { get; set; }
		public OrderStatus Status { get; set; }
		public ICollection<OrderItem> OrderItems { get; set; }
		public ICollection<Payment>   Payments { get; set; }
		[Required]
		
		public decimal FinalPrice { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(2);


    }
}
