using Bags_Shop_API.Services.DiscountServices.Jobs;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.DiscountServices.Commands
{
    public class UpdateDiscountCommandHandler : IRequestHandler<UpdateDiscountCommand, Result<DiscountDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDiscountMapper _discountMapper;
        private readonly IDiscountSchedulerService _schedulerService;

        public UpdateDiscountCommandHandler(
            IUnitOfWork unitOfWork, 
            IDiscountMapper discountMapper,
            IDiscountSchedulerService schedulerService)
        {
            _unitOfWork = unitOfWork;
            _discountMapper = discountMapper;
            _schedulerService = schedulerService;
        }

        public async Task<Result<DiscountDto>> Handle(UpdateDiscountCommand request, CancellationToken cancellationToken)
        {
            var discount = await _unitOfWork.Discounts.GetByIdAsync(request.Id);

            if (discount == null)
                return Result<DiscountDto>.Fail($"No Discount With Id {request.Id}", 404);

            bool isUpdated = false;

            if (request.DiscountPercentage.HasValue)
            {
                if (request.DiscountPercentage <= 0 || request.DiscountPercentage >= 90)
                    return Result<DiscountDto>.Fail("Discount percentage must be between 1 and 90");

                discount.DiscountPercentage = request.DiscountPercentage.Value;
                isUpdated = true;
            }

            if (request.StartDate.HasValue)
            {
                if (request.EndDate.HasValue && request.StartDate >= request.EndDate)
                    return Result<DiscountDto>.Fail("Start date must be before end date");

                if (!request.EndDate.HasValue && request.StartDate >= discount.EndDate)
                    return Result<DiscountDto>.Fail("Start date must be before end date");

                discount.StartDate = request.StartDate.Value;
                isUpdated = true;
            }

            if (request.EndDate.HasValue)
            {
                if (request.EndDate <= discount.StartDate)
                    return Result<DiscountDto>.Fail("End date must be after start date");

                discount.EndDate = request.EndDate.Value;
                isUpdated = true;
            }

            if (!isUpdated)
                return Result<DiscountDto>.Fail("No valid fields to update");

            await _unitOfWork.SaveChangesAsync();

            // Reschedule jobs if dates were updated
            if (request.StartDate.HasValue || request.EndDate.HasValue)
            {
                _schedulerService.CancelScheduledJobs(discount.Id);
                _schedulerService.ScheduleActivation(discount.Id, discount.StartDate);
                _schedulerService.ScheduleExpiration(discount.Id, discount.EndDate);
            }

            return Result<DiscountDto>.Ok(_discountMapper.ToDto(discount));
        }
    }
}
