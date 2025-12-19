using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.DiscountServices.Commands
{
    public class DeleteDiscountCommand : IRequest<Result<string>>, IInvalidateCache
    {
        public bool InvalidateAll => true;
        public int Id { get; set; }

        public DeleteDiscountCommand(int id)
        {
            Id = id;
        }
    }
}
