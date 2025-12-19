using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.CollectionServices.Commands
{
    public class CreateCollectionCommandHandler : IRequestHandler<CreateCollectionCommand, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICollectionFactory _collectionFactory;

        public CreateCollectionCommandHandler(IUnitOfWork unitOfWork, ICollectionFactory collectionFactory)
        {
            _unitOfWork = unitOfWork;
            _collectionFactory = collectionFactory;
        }

        public async Task<Result<int>> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
                return Result<int>.Fail("Invalid Request");

            var collection = _collectionFactory.CreateCollection(
                request.ArName,
                request.EnName,
                request.ArDescription,
                request.EnDescription);

            if (!collection.Success || collection.Data is null)
                return Result<int>.Fail(collection.Message);

            await _unitOfWork.Collections.AddAsync(collection.Data);
            await _unitOfWork.SaveChangesAsync();

            return Result<int>.Ok(collection.Data.Id);
        }
    }
}
