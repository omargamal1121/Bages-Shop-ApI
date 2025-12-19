using Bags_Shop_API.Services.Shared;

namespace Bags_Shop_API.Services.ProductServices.ProductFactories
{
	public interface IProductFactory
    {
        public Result<Product> CreateProduct(string EnName, string ArName, string EnDescription, string ArDescription,decimal Price);
    }

}
