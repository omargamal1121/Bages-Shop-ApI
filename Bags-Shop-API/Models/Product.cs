using Bags_Shop_API.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bags_Shop_API
{
	public class Product
	{
		[Key]
		
		public int Id { get; set; }
		[Required(ErrorMessage ="Need Arabic Name For Product")]
		[Range(3,100, ErrorMessage ="Product Name Must Be Between 3 and 100 Characters")]

		public string ArName { get; set; }
		[Required(ErrorMessage = "Need English Name For Product")]
		[Range(3, 100, ErrorMessage = "Product Name Must Be Between 3 and 100 Characters")]
        public string EnName { get; set; }
		[Required(ErrorMessage = "Need Arabic Description For Product")]
		[Range(10, 500, ErrorMessage = "Product Description Must Be Between 10 and 500 Characters")]
        public string ArDescription { get; set; }
		[Required(ErrorMessage = "Need English Description For Product")]
		[Range(10, 500, ErrorMessage = "Product Description Must Be Between 10 and 500 Characters")]
        public string EnDescription { get; set; }
		public  decimal Price { get; set; }
		public ICollection<Image>  Images { get; set; }
		public int? CollectionId { get; set; }
		public Collection  Collection { get; set; }
		public List<OrderItem>  orderItems { get; set; }
		public int? DiscountId { get; set; }
		public Discount? Discount { get; set; }
        public bool IsActive { get; set; } = false;

		public  DateTime? Delete_AT { get; set; }


	}
}
