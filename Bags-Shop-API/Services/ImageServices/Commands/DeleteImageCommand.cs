using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.ImageServices.Commands
{
    public class DeleteImageCommand : IRequest<Result<string>>, IInvalidateCache
    {
        public bool InvalidateAll => true;
        public int ImageId { get; set; }

        public DeleteImageCommand(int imageId)
        {
            ImageId = imageId;
        }
    }
}
