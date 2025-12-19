using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.DiscountServices.Commands
{
    public class AddProductsToDiscountCommandHandler : IRequestHandler<AddProductsToDiscountCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddProductsToDiscountCommandHandler> _logger;

        public AddProductsToDiscountCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<AddProductsToDiscountCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            AddProductsToDiscountCommand request,
            CancellationToken cancellationToken)
        {
            var discount = await _unitOfWork.Discounts.GetByIdAsync(request.DiscountId);
            if (discount == null)
            {
                _logger.LogWarning(
                    "Discount not found with ID: {DiscountId}",
                    request.DiscountId);
                return Result<bool>.Fail($"No Discount With Id {request.DiscountId}", 404);
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

                product.DiscountId = request.DiscountId;
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Products {ProductIds} added to Discount {DiscountId}",
                string.Join(",", request.ProductIds),
                request.DiscountId);

            return Result<bool>.Ok(true, "Products added to discount successfully");
        }
    }
}
