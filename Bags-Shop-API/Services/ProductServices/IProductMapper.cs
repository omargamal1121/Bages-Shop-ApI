using Bags_Shop_API.Services.ProductServices.ProductsDtos;

namespace Bags_Shop_API.Services.ProductServices
{
	public interface IProductMapper
    {
        public CreateProductResponseDto createProductResponseDto(Product product);
        public ProductDto UpdateProductDto(Product product);
    }

}
