using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.DiscountServices.Commands
{
    public class ToggleDiscountActiveCommandHandler : IRequestHandler<ToggleDiscountActiveCommand, Result<DiscountDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDiscountMapper _discountMapper;

        public ToggleDiscountActiveCommandHandler(IUnitOfWork unitOfWork, IDiscountMapper discountMapper)
        {
            _unitOfWork = unitOfWork;
            _discountMapper = discountMapper;
        }

        public async Task<Result<DiscountDto>> Handle(ToggleDiscountActiveCommand request, CancellationToken cancellationToken)
        {
            var discount = await _unitOfWork.Discounts.GetByIdAsync(request.Id);

            if (discount == null)
                return Result<DiscountDto>.Fail($"No Discount With Id {request.Id}", 404);

            discount.IsActive = request.IsActive;
            await _unitOfWork.SaveChangesAsync();

            return Result<DiscountDto>.Ok(_discountMapper.ToDto(discount));
        }
    }
}
