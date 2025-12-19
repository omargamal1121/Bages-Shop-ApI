using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.CollectionServices.Commands
{
    public class RemoveProductFromCollectionCommandHandler : IRequestHandler<RemoveProductFromCollectionCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RemoveProductFromCollectionCommandHandler> _logger;

        public RemoveProductFromCollectionCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<RemoveProductFromCollectionCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            RemoveProductFromCollectionCommand request,
            CancellationToken cancellationToken)
        {
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

                product.CollectionId = null;
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Products {ProductIds} removed from their Collection",
                string.Join(",", request.ProductIds));

            return Result<bool>.Ok(true, "Products removed from collection successfully");
        }
    }
}


