using Bags_Shop_API.UnitOfWorkService;

namespace Bags_Shop_API.Services.DiscountServices.Jobs
{
    public class DiscountJobService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DiscountJobService> _logger;
        

        public DiscountJobService(IUnitOfWork unitOfWork, ILogger<DiscountJobService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ActivateDiscountAsync(int discountId)
        {
            try
            {
                var discount = await _unitOfWork.Discounts.GetByIdAsync(discountId);
                
                if (discount == null)
                {
                    _logger.LogWarning("Discount with ID {DiscountId} not found for activation", discountId);
                    return;
                }

                if (discount.IsActive)
                {
                    _logger.LogInformation("Discount with ID {DiscountId} is already active", discountId);
                    return;
                }
                if(discount.EndDate < DateTime.Now)
                {
                    _logger.LogInformation("Discount with ID {DiscountId} has already expired and cannot be activated", discountId);
                    return;
                }
                if(discount.StartDate > DateTime.Now)
                {
                    _logger.LogInformation("Discount with ID {DiscountId} start date is in the future and cannot be activated yet", discountId);
                    return;
                }

                discount.IsActive = true;
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Successfully activated discount with ID {DiscountId}", discountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating discount with ID {DiscountId}", discountId);
                throw;
            }
        }

        public async Task ExpireDiscountAsync(int discountId)
        {
            try
            {
                var discount = await _unitOfWork.Discounts.GetByIdAsync(discountId);
                
                if (discount == null)
                {
                    _logger.LogWarning("Discount with ID {DiscountId} not found for expiration", discountId);
                    return;
                }

                if (!discount.IsActive)
                {
                    _logger.LogInformation("Discount with ID {DiscountId} is already inactive", discountId);
                    return;
                }

                discount.IsActive = false;
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Successfully expired discount with ID {DiscountId}", discountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error expiring discount with ID {DiscountId}", discountId);
                throw;
            }
        }
    }
}
