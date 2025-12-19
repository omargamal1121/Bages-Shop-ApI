using Bags_Shop_API.Services.DiscountServices;
using Bags_Shop_API.Services.ImageServices;

namespace Bags_Shop_API.Services.ProductServices.ProductsDtos
{
    public class ProductDto 
    {
        public int Id { get; set; }
        public string ArName { get; set; }
        public string EnName { get; set; }
        public string ArDescription { get; set; }
        public string EnDescription { get; set; }
        public int Quantity { get; set; }
		public decimal Price { get; set; }
		public decimal FinalPrice { get; set; }
		public bool IsActive { get; set; }
        
        // Nested DTOs
        public DiscountDto? Discount { get; set; }
        public List<ImageDto> Images { get; set; } = new List<ImageDto>();
    }
}
