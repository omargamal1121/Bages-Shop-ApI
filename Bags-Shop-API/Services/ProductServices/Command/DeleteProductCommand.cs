using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.ProductServices.Command
{
    public class DeleteProductCommand : IRequest<Result<bool>>, IInvalidateCache
    {
        public bool InvalidateAll => true;

        public int Id { get; set; }

        public DeleteProductCommand(int id)
        {
            Id = id;
        }
    }
}


