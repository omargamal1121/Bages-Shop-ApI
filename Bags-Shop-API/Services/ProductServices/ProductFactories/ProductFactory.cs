using Bags_Shop_API.Services.Shared;

namespace Bags_Shop_API.Services.ProductServices.ProductFactories
{
	public class ProductFactory : IProductFactory 
    {
        public Result<Product> CreateProduct(string EnName, string ArName,string EnDescription, string ArDescription,decimal Price)
        {
            if (string.IsNullOrEmpty(EnName) || EnName.Count() < 3)
                return Result<Product>.Fail("Invalid English Name");
            if (string.IsNullOrEmpty(ArName) || ArName.Count() < 3)
                return Result<Product>.Fail("Invalid Arabic Name");
            if (string.IsNullOrEmpty(EnDescription) || EnDescription.Count() < 3)
                return Result<Product>.Fail("Invalid English Description");
            if (string.IsNullOrEmpty(ArDescription) || ArDescription.Count() < 3)
                return Result<Product>.Fail("Invalid Arabic Description");
            
            return Result<Product>.Ok(new Product
            {

                ArDescription = ArDescription,
                EnName = EnName,
                EnDescription = EnDescription,
                Price= Price,
           
                ArName = ArName,

            });


        }

        

    }

}
