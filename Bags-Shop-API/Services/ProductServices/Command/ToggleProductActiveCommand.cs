using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.ProductServices.ProductsDtos;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.ProductServices.Command
{
    public class ToggleProductActiveCommand : IRequest<Result<bool>>, IInvalidateCache
    {
        public bool InvalidateAll => true;
        public int Id { get; set; }
        public bool IsActive { get; set; }

        public ToggleProductActiveCommand(int id, bool isActive)
        {
            Id = id;
            IsActive = isActive;
        }
    }
}
