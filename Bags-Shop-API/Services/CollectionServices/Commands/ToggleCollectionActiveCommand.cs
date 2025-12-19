using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.CollectionServices.Commands
{
    public class ToggleCollectionActiveCommand : IRequest<Result<CollectionDto>>, IInvalidateCache
    {
        public bool InvalidateAll => true;
        public int Id { get; set; }
        public bool IsActive { get; set; }

        public ToggleCollectionActiveCommand(int id, bool isActive)
        {
            Id = id;
            IsActive = isActive;
        }
    }
}
