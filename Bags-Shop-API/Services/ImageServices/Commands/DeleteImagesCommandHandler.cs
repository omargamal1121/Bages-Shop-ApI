using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.ImageServices.Commands
{
    public class DeleteImagesCommandHandler : IRequestHandler<DeleteImagesCommand, Result<List<string>>>
    {
        private readonly ILogger<DeleteImagesCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryImageService _cloudinaryService;

        public DeleteImagesCommandHandler(
            ILogger<DeleteImagesCommandHandler> logger,
            IUnitOfWork unitOfWork,
            ICloudinaryImageService cloudinaryService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<Result<List<string>>> Handle(DeleteImagesCommand request, CancellationToken cancellationToken)
        {
            if (request.Images == null || !request.Images.Any())
                return Result<List<string>>.Fail("No images provided for deletion");

            _logger.LogInformation("Deleting {Count} images", request.Images.Count);

            var deletedIds = new List<string>();

            try
            {
                // Capture distinct product IDs before removal
                var affectedProductIds = request.Images
                    .Where(i => i.ProductId.HasValue)
                    .Select(i => i.ProductId!.Value)
                    .Distinct()
                    .ToList();

                _unitOfWork.Images.RemoveRange(request.Images);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("All {Count} images removed from database", request.Images.Count);
                foreach (var image in request.Images)
                {
                    _cloudinaryService.EnqueueDeletion(image.CloudinaryPublicId);
                    deletedIds.Add(image.Id.ToString());
                }

                // For each affected product, deactivate if it has no remaining images
                foreach (var productId in affectedProductIds)
                {
                    var remainingImagesSpec = new Bags_Shop_API.Specification.BaseSpecification<Bags_Shop_API.Image>(i => i.ProductId == productId);
                    var hasAnyImages = await _unitOfWork.Images.AnyAsync(remainingImagesSpec);

                    if (!hasAnyImages)
                    {
                        var product = await _unitOfWork.Products.GetByIdAsync(productId);
                        if (product != null && product.IsActive)
                        {
                            _logger.LogInformation("Deactivating product {ProductId} because it no longer has images", productId);
                            product.IsActive = false;
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }

                return Result<List<string>>.Ok(deletedIds, "Images deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting images");
                return Result<List<string>>.Fail("Error occurred while deleting images", 500);
            }
        }
    }
}
