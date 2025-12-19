using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Specification;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.CollectionServices.Commands
{
    public class ToggleCollectionActiveCommandHandler : IRequestHandler<ToggleCollectionActiveCommand, Result<CollectionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICollectionMapper _collectionMapper;

        public ToggleCollectionActiveCommandHandler(IUnitOfWork unitOfWork, ICollectionMapper collectionMapper)
        {
            _unitOfWork = unitOfWork;
            _collectionMapper = collectionMapper;
        }

        public async Task<Result<CollectionDto>> Handle(ToggleCollectionActiveCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive)
            {
                var spec = new BaseSpecificationWithProjection<Collection, CollectionProjection>(
                    c => new CollectionProjection(c.Products.Any(p => p.IsActive), c.Images.Any()));
                spec.Criteria = c => c.Id == request.Id;
                
                var projection = await _unitOfWork.Collections.GetByIdAsync(spec);

                if (projection == null)
                    return Result<CollectionDto>.Fail($"No Collection With Id {request.Id}", 404);

                if (!projection.HasProducts)
                    return Result<CollectionDto>.Fail("Collection must have at least one active product to be active.", 400);

                if (!projection.HasImages)
                    return Result<CollectionDto>.Fail("Collection must have at least one image to be active.", 400);
            }

            var collection = await _unitOfWork.Collections.GetByIdAsync(request.Id);

            if (collection == null)
                return Result<CollectionDto>.Fail($"No Collection With Id {request.Id}", 404);

            collection.IsActive = request.IsActive;

            await _unitOfWork.SaveChangesAsync();

            return Result<CollectionDto>.Ok(_collectionMapper.ToDto(collection));
        }

        private record CollectionProjection(bool HasProducts, bool HasImages);
    }
}
