using Bags_Shop_API.Services.Shared;

namespace Bags_Shop_API.Services.CollectionServices
{
    public interface ICollectionFactory
    {
        Result<Collection> CreateCollection(string arName, string enName, string arDescription, string enDescription);
    }

    public class CollectionFactory : ICollectionFactory
    {
        public Result<Collection> CreateCollection(string arName, string enName, string arDescription, string enDescription)
        {
            if (string.IsNullOrWhiteSpace(arName) || arName.Length < 3 || arName.Length > 100)
                return Result<Collection>.Fail("Arabic name must be between 3 and 100 characters");

            if (string.IsNullOrWhiteSpace(enName) || enName.Length < 3 || enName.Length > 100)
                return Result<Collection>.Fail("English name must be between 3 and 100 characters");

            if (string.IsNullOrWhiteSpace(arDescription) || arDescription.Length < 10 || arDescription.Length > 500)
                return Result<Collection>.Fail("Arabic description must be between 10 and 500 characters");

            if (string.IsNullOrWhiteSpace(enDescription) || enDescription.Length < 10 || enDescription.Length > 500)
                return Result<Collection>.Fail("English description must be between 10 and 500 characters");

            var collection = new Collection
            {
                ArName = arName,
                EnName = enName,
                ArDescription = arDescription,
                EnDescription = enDescription,
                IsActive = false
            };

            return Result<Collection>.Ok(collection);
        }
    }
}
