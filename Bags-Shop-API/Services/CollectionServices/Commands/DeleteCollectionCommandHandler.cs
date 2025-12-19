using Bags_Shop_API.Services.ImageServices;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Specification;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.CollectionServices.Commands
{
    public class DeleteCollectionCommandHandler : IRequestHandler<DeleteCollectionCommand, Result<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteCollectionCommandHandler> _logger;
        private readonly ICloudinaryImageService _cloudinaryImageService;

        public DeleteCollectionCommandHandler(
            IUnitOfWork unitOfWork, 
            ILogger<DeleteCollectionCommandHandler> logger, 
            ICloudinaryImageService cloudinaryImageService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cloudinaryImageService = cloudinaryImageService;
        }

        private record CollectionDeleteInfo(bool HasOrderedProducts, List<string> CloudinaryPublicIds);

        public async Task<Result<string>> Handle(DeleteCollectionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing delete request for collection ID: {CollectionId}", request.Id);

            var spec = new BaseSpecificationWithProjection<Collection, CollectionDeleteInfo>(c => new CollectionDeleteInfo(
                c.Products.Any(p => p.orderItems.Any()),
                c.Images.Select(i => i.CloudinaryPublicId).ToList()
            ));

            spec.Criteria = c => c.Id == request.Id;

            var collectionInfos = await _unitOfWork.Collections.GetAllAsync(spec);
            var collectionInfo = collectionInfos.FirstOrDefault();

            if (collectionInfo == null)
            {
                _logger.LogWarning("Collection not found with ID: {CollectionId}", request.Id);
                return Result<string>.Fail($"Collection not found with ID: {request.Id}", 404);
            }

            if (collectionInfo.HasOrderedProducts)
            {
                _logger.LogWarning("Cannot delete collection {CollectionId} as it contains products used in order items", request.Id);
                return Result<string>.Fail("can't delete this but u can deactive it", 409);
            }

            _logger.LogInformation("Deleting collection {CollectionId} and its {ImageCount} images", request.Id, collectionInfo.CloudinaryPublicIds.Count);

            // Enqueue Cloudinary delections in background via Hangfire
            foreach (var publicId in collectionInfo.CloudinaryPublicIds)
            {
                _cloudinaryImageService.EnqueueDeletion(publicId);
            }

            // Wrap deletions in a transaction for atomicity
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Bulk delete images and collection from DB
                await _unitOfWork.Images.ExecuteDeleteAsync(i => i.CollectionId == request.Id);
                await _unitOfWork.Collections.ExecuteDeleteAsync(c => c.Id == request.Id);

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error deleting collection {CollectionId}", request.Id);
                return Result<string>.Fail("An error occurred during deletion", 500);
            }

            _logger.LogInformation("Collection with ID: {CollectionId} deleted successfully from database", request.Id);

            return Result<string>.Ok("Collection deleted successfully");
        }
    }
}
