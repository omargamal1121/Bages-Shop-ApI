using System.ComponentModel.DataAnnotations;

namespace Bags_Shop_API.Models
{
	public class Discount 
	{
		public int Id { get; set; }
		public ICollection<Product>  Products { get; set; }
		[Required(ErrorMessage = "Discount Percentage Required ")]
		[Range(1,90,ErrorMessage ="Must be greater than 0 and less than 90")]
		public decimal DiscountPercentage { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = false;


    }
}
