namespace Bags_Shop_API.Services.CollectionServices
{
    public interface ICollectionMapper
    {
        CollectionDto ToDto(Collection collection);
    }

    public class CollectionMapper : ICollectionMapper
    {
        public CollectionDto ToDto(Collection collection)
        {
            return new CollectionDto
            {
                Id = collection.Id,
                ArName = collection.ArName,
                EnName = collection.EnName,
                ArDescription = collection.ArDescription,
                EnDescription = collection.EnDescription,
                IsActive = collection.IsActive
            };
        }
    }
}
