using Bags_Shop_API.Services.ImageServices;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Specification;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.ProductServices.Command
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteProductCommandHandler> _logger;
        private readonly ICloudinaryImageService _cloudinaryImageService;

        public DeleteProductCommandHandler(
            IUnitOfWork unitOfWork, 
            ILogger<DeleteProductCommandHandler> logger,
            ICloudinaryImageService cloudinaryImageService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cloudinaryImageService = cloudinaryImageService;
        }

        private record ProductDeleteInfo(bool HasOrderItems, List<string> CloudinaryPublicIds);

        public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing delete request for product ID: {ProductId}", request.Id);

            var spec = new BaseSpecificationWithProjection<Product, ProductDeleteInfo>(p => new ProductDeleteInfo(
                p.orderItems.Any(),
                p.Images.Select(i => i.CloudinaryPublicId).ToList()
            ));

            spec.Criteria = p => p.Id == request.Id;

            var productInfos = await _unitOfWork.Products.GetAllAsync(spec);
            var productInfo = productInfos.FirstOrDefault();

            if (productInfo == null)
            {
                _logger.LogWarning("Product not found with ID: {ProductId}", request.Id);
                return Result<bool>.Fail($"No Product With Id {request.Id}", 404);
            }

            if (productInfo.HasOrderItems)
            {
                _logger.LogWarning("Cannot delete product {ProductId} as it is used in order items", request.Id);
                return Result<bool>.Fail("can't delete this but u can deactive it", 409);
            }

            _logger.LogInformation("Deleting product {ProductId} and its {ImageCount} images", request.Id, productInfo.CloudinaryPublicIds.Count);

            // Enqueue Cloudinary delections in background via Hangfire
            foreach (var publicId in productInfo.CloudinaryPublicIds)
            {
                _cloudinaryImageService.EnqueueDeletion(publicId);
            }

            // Wrap deletions in a transaction for atomicity
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Bulk delete images and product from DB
                await _unitOfWork.Images.ExecuteDeleteAsync(i => i.ProductId == request.Id);
                await _unitOfWork.Products.ExecuteDeleteAsync(p => p.Id == request.Id);

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error deleting product {ProductId}", request.Id);
                return Result<bool>.Fail("An error occurred during deletion", 500);
            }

            _logger.LogInformation("Product with ID: {ProductId} deleted successfully from database", request.Id);

            return Result<bool>.Ok(true, "Product deleted successfully");
        }
    }
}


