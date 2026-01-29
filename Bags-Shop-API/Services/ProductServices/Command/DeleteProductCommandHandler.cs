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


            var productInfo = await _unitOfWork.Products.GetByIdAsync(request.Id);
       

            if (productInfo == null||productInfo.Delete_AT is not null)
            {
                _logger.LogWarning("Product not found with ID: {ProductId}", request.Id);
                return Result<bool>.Fail($"No Product With Id {request.Id}", 404);
            }
            productInfo.Delete_AT = DateTime.UtcNow;
            _unitOfWork.Products.Update(productInfo);
            await _unitOfWork.SaveChangesAsync();




            _logger.LogInformation("Product with ID: {ProductId} deleted successfully from database", request.Id);

            return Result<bool>.Ok(true, "Product deleted successfully");
        }
    }
}


