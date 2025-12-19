using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.CollectionServices.Commands
{
    public class DeleteCollectionCommand : IRequest<Result<string>>, IInvalidateCache
    {
        public bool InvalidateAll => true;
        public int Id { get; set; }

        public DeleteCollectionCommand(int id)
        {
            Id = id;
        }
    }
}
