using Bags_Shop_API.Services.ProductServices.ProductsDtos;

namespace Bags_Shop_API.Services.ProductServices
{
	public class ProductMapper: IProductMapper
    {
        public ProductMapper() { }
        public CreateProductResponseDto createProductResponseDto(Product product) 
        {
            return new CreateProductResponseDto
            {
                ArDescription = product.ArDescription,
                EnName = product.EnName,
                ArName = product.ArName,
                EnDescription = product.EnDescription,
                Id = product.Id,
            }
            ;
        }
        public ProductDto UpdateProductDto(Product product)
        {
            return new ProductDto
            {
                ArDescription = product.ArDescription,
                EnName = product.EnName,
                EnDescription = product.EnDescription,
            
                Id = product.Id,
                ArName = product.ArName,
                
            };
        }

    }

}
