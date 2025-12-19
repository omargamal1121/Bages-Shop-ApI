using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.ImageServices.Commands
{
    public class SaveImagesCommand : IRequest<Result<List<Image>>>, IInvalidateCache
    {
        public bool InvalidateAll => true;
        public List<IFormFile> Images { get; set; }
        public int EntityId { get; set; }
        public bool IsProduct { get; set; }

        public SaveImagesCommand(List<IFormFile> images, int entityId, bool isProduct)
        {
            Images = images;
            EntityId = entityId;
            IsProduct = isProduct;
        }
    }
}
