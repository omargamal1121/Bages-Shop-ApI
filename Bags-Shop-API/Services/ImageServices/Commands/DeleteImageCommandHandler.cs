using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.ImageServices.Commands
{
    public class DeleteImageCommandHandler : IRequestHandler<DeleteImageCommand, Result<string>>
    {
        private readonly ILogger<DeleteImageCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryImageService _cloudinaryService;

        public DeleteImageCommandHandler(
            ILogger<DeleteImageCommandHandler> logger,
            IUnitOfWork unitOfWork,
            ICloudinaryImageService cloudinaryService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<Result<string>> Handle(DeleteImageCommand request, CancellationToken cancellationToken)
        {
            var image = await _unitOfWork.Images.GetByIdAsync(request.ImageId);
            if (image == null)
            {
                _logger.LogWarning("Image not found with ID: {ImageId}", request.ImageId);
                return Result<string>.Fail($"Image not found with ID: {request.ImageId}", 404);
            }

            _logger.LogInformation("Deleting image ID: {ImageId}", image.Id);

            if (string.IsNullOrEmpty(image.CloudinaryPublicId))
            {
                _logger.LogWarning("Image {ImageId} has no CloudinaryPublicId, skipping Cloudinary deletion", image.Id);
            }

            try
            {
                _unitOfWork.Images.Remove(image);
                await _unitOfWork.SaveChangesAsync();

                // Enqueue deletion from Cloudinary
                _cloudinaryService.EnqueueDeletion(image.CloudinaryPublicId);

                // If this image belongs to a product, check if the product has any remaining images.
                if (image.ProductId.HasValue)
                {
                    var productId = image.ProductId.Value;

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

                _logger.LogInformation("Image {ImageId} deleted from database", image.Id);
                return Result<string>.Ok("Image deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image {ImageId}", image.Id);
                return Result<string>.Fail("An error occurred while deleting the image", 500);
            }
        }
    }
}
