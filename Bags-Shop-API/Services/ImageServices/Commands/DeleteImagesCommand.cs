using Bags_Shop_API;
using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.ImageServices.Commands
{
    public class DeleteImagesCommand : IRequest<Result<List<string>>>, IInvalidateCache
    {
        public bool InvalidateAll => true;
        public List<Image> Images { get; set; }

        public DeleteImagesCommand(List<Image> images)
        {
            Images = images;
        }
    }
}
