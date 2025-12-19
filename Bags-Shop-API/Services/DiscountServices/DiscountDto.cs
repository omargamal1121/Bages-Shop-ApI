using Bags_Shop_API.Services.ProductServices.ProductsDtos;

namespace Bags_Shop_API.Services.DiscountServices
{
    public class DiscountDto
    {
        public int Id { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public List<ProductDto> Products { get; set; } = new();
    }
}
