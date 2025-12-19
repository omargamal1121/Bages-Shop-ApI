using Bags_Shop_API.Services.DiscountServices.Jobs;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.DiscountServices.Commands
{
    public class CreateDiscountCommandHandler : IRequestHandler<CreateDiscountCommand, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDiscountFactory _discountFactory;
        private readonly IDiscountSchedulerService _schedulerService;

        public CreateDiscountCommandHandler(
            IUnitOfWork unitOfWork, 
            IDiscountFactory discountFactory,
            IDiscountSchedulerService schedulerService)
        {
            _unitOfWork = unitOfWork;
            _discountFactory = discountFactory;
            _schedulerService = schedulerService;
        }

        public async Task<Result<int>> Handle(CreateDiscountCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
                return Result<int>.Fail("Invalid Request");

            var discount = _discountFactory.CreateDiscount(
                request.DiscountPercentage,
                request.StartDate,
                request.EndDate);

            if (!discount.Success || discount.Data is null)
                return Result<int>.Fail(discount.Message);

            await _unitOfWork.Discounts.AddAsync(discount.Data);
            await _unitOfWork.SaveChangesAsync();


            _schedulerService.ScheduleActivation(discount.Data.Id, request.StartDate);
            _schedulerService.ScheduleExpiration(discount.Data.Id, request.EndDate);

            return Result<int>.Ok(discount.Data.Id);
        }
    }
}
