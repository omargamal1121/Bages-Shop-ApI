using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.CollectionServices.Commands
{
    public class AddProductToCollectionCommandHandler : IRequestHandler<AddProductToCollectionCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddProductToCollectionCommandHandler> _logger;

        public AddProductToCollectionCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<AddProductToCollectionCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            AddProductToCollectionCommand request,
            CancellationToken cancellationToken)
        {
            var collection = await _unitOfWork.Collections.GetByIdAsync(request.CollectionId);
            if (collection == null)
            {
                _logger.LogWarning(
                    "Collection not found with ID: {CollectionId}",
                    request.CollectionId);
                return Result<bool>.Fail($"No Collection With Id {request.CollectionId}", 404);
            }

            if (request.ProductIds == null || !request.ProductIds.Any())
            {
                return Result<bool>.Fail("No product IDs provided");
            }

            foreach (var productId in request.ProductIds.Distinct())
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning("Product not found with ID: {ProductId}", productId);
                    return Result<bool>.Fail($"No Product With Id {productId}", 404);
                }

                product.CollectionId = request.CollectionId;
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Products {ProductIds} added to Collection {CollectionId}",
                string.Join(",", request.ProductIds),
                request.CollectionId);

            return Result<bool>.Ok(true, "Products added to collection successfully");
        }
    }
}


