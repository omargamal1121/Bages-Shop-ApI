using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bags_Shop_API.Models
{
	public class OrderItem
	{
		[ForeignKey("Product")]
		public int ProductId { get; set; }
		public Product Product { get; set; }
		[Range(1, 20, ErrorMessage = "Quantity Must Be Between 1 and 100")]
		public int Quantity { get; set; }
		[ForeignKey("Order")]
        public int OrderId { get; set; }
		
		public Order Order { get; set; }

		public decimal UnitPrice
        { get; set; }
		public decimal TotalPrice { get; set; }
	}
}
