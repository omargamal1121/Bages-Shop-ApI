using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.DiscountServices.Commands
{
    public class RemoveProductsFromDiscountCommand : IRequest<Result<bool>>, IInvalidateCache
    {
        public bool InvalidateAll => true;

        public List<int> ProductIds { get; set; } = new();

        public RemoveProductsFromDiscountCommand(List<int> productIds)
        {
            ProductIds = productIds;
        }
    }
}
