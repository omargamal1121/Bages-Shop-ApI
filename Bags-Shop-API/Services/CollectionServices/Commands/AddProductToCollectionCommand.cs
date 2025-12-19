using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.CollectionServices.Commands
{
    public class AddProductToCollectionCommand : IRequest<Result<bool>>, IInvalidateCache
    {
        public bool InvalidateAll => true;

        public int CollectionId { get; set; }
        public List<int> ProductIds { get; set; } = new();

        public AddProductToCollectionCommand(int collectionId, List<int> productIds)
        {
            CollectionId = collectionId;
            ProductIds = productIds;
        }
    }
}


