using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.ImageServices.Commands
{
    public class SaveImageCommand : IRequest<Result<Image>>, IInvalidateCache
    {
        public bool InvalidateAll => true;
        public IFormFile Image { get; set; }
        public int EntityId { get; set; }
        public bool IsProduct { get; set; }

        public SaveImageCommand(IFormFile image, int entityId, bool isProduct)
        {
            Image = image;
            EntityId = entityId;
            IsProduct = isProduct;
        }
    }
}
