using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.DiscountServices.Commands
{
    public class CreateDiscountCommand : IRequest<Result<int>>, IInvalidateCache
    {
        public bool InvalidateAll => true;
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
