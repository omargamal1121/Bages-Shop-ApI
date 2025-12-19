using Bags_Shop_API.Services.DiscountServices.Jobs;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.DiscountServices.Commands
{
    public class DeleteDiscountCommandHandler : IRequestHandler<DeleteDiscountCommand, Result<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteDiscountCommandHandler> _logger;
        private readonly IDiscountSchedulerService _schedulerService;

        public DeleteDiscountCommandHandler(
            IUnitOfWork unitOfWork, 
            ILogger<DeleteDiscountCommandHandler> logger,
            IDiscountSchedulerService schedulerService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _schedulerService = schedulerService;
        }

        public async Task<Result<string>> Handle(DeleteDiscountCommand request, CancellationToken cancellationToken)
        {
            var discount = await _unitOfWork.Discounts.GetByIdAsync(request.Id);

            if (discount == null)
            {
                _logger.LogWarning("Discount not found with ID: {DiscountId}", request.Id);
                return Result<string>.Fail($"Discount not found with ID: {request.Id}", 404);
            }

            try
            {
                // Cancel scheduled jobs before deleting
                _schedulerService.CancelScheduledJobs(discount.Id);
                
                _unitOfWork.Discounts.Remove(discount);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Discount {DiscountId} deleted successfully", request.Id);
                return Result<string>.Ok("Discount deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting discount {DiscountId}", request.Id);
                return Result<string>.Fail("An error occurred while deleting the discount", 500);
            }
        }
    }
}
