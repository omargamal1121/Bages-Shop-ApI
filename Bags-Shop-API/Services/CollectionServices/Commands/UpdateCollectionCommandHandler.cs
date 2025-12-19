using Bags_Shop_API.Services.ImageServices.Commands;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.CollectionServices.Commands
{
    public class UpdateCollectionCommandHandler : IRequestHandler<UpdateCollectionCommand, Result<CollectionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICollectionMapper _collectionMapper;
        private readonly IMediator _mediator;

        public UpdateCollectionCommandHandler(IUnitOfWork unitOfWork, ICollectionMapper collectionMapper, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _collectionMapper = collectionMapper;
            _mediator = mediator;
        }

        public async Task<Result<CollectionDto>> Handle(UpdateCollectionCommand request, CancellationToken cancellationToken)
        {
            var collection = await _unitOfWork.Collections.GetByIdAsync(request.Id);

            if (collection == null)
                return Result<CollectionDto>.Fail($"No Collection With Id {request.Id}", 404);

            bool isUpdated = false;

            if (!string.IsNullOrEmpty(request.EnName))
            {
                if (request.EnName.Length < 3 || request.EnName.Length > 100)
                    return Result<CollectionDto>.Fail("English name must be between 3 and 100 characters");

                collection.EnName = request.EnName;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(request.ArName))
            {
                if (request.ArName.Length < 3 || request.ArName.Length > 100)
                    return Result<CollectionDto>.Fail("Arabic name must be between 3 and 100 characters");

                collection.ArName = request.ArName;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(request.ArDescription))
            {
                if (request.ArDescription.Length < 10 || request.ArDescription.Length > 500)
                    return Result<CollectionDto>.Fail("Arabic description must be between 10 and 500 characters");

                collection.ArDescription = request.ArDescription;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(request.EnDescription))
            {
                if (request.EnDescription.Length < 10 || request.EnDescription.Length > 500)
                    return Result<CollectionDto>.Fail("English description must be between 10 and 500 characters");

                collection.EnDescription = request.EnDescription;
                isUpdated = true;
            }

            // Handle image uploads
            if (request.Images != null && request.Images.Any())
            {
                var saveImagesCommand = new SaveImagesCommand(request.Images, request.Id, isProduct: false);
                var imageResult = await _mediator.Send(saveImagesCommand, cancellationToken);

                if (!imageResult.Success)
                {
                    return Result<CollectionDto>.Fail($"Failed to save images: {imageResult.Message}", imageResult.StatusCode);
                }

                isUpdated = true;
            }

            if (!isUpdated)
                return Result<CollectionDto>.Fail("No valid fields to update");

            await _unitOfWork.SaveChangesAsync();

            return Result<CollectionDto>.Ok(_collectionMapper.ToDto(collection));
        }
    }
}
