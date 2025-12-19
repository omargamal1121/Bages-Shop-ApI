using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.DiscountServices.Commands
{
    public class AddProductsToDiscountCommand : IRequest<Result<bool>>, IInvalidateCache
    {
        public bool InvalidateAll => true;

        public int DiscountId { get; set; }
        public List<int> ProductIds { get; set; } = new();

        public AddProductsToDiscountCommand(int discountId, List<int> productIds)
        {
            DiscountId = discountId;
            ProductIds = productIds;
        }
    }
}
