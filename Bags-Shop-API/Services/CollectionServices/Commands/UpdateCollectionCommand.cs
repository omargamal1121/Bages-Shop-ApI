using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.CollectionServices.Commands
{
    public class UpdateCollectionCommand : IRequest<Result<CollectionDto>>, IInvalidateCache
    {
        public bool InvalidateAll => true;
        public int Id { get; set; }
        public string? ArName { get; set; }
        public string? EnName { get; set; }
        public string? ArDescription { get; set; }
        public string? EnDescription { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}
