using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.DiscountServices.Commands
{
    public class ToggleDiscountActiveCommand : IRequest<Result<DiscountDto>>, IInvalidateCache
    {
        public bool InvalidateAll => true;
        public int Id { get; set; }
        public bool IsActive { get; set; }

        public ToggleDiscountActiveCommand(int id, bool isActive)
        {
            Id = id;
            IsActive = isActive;
        }
    }
}
