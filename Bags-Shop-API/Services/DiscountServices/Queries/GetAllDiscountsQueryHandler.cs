using Bags_Shop_API.Models;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Specification;
using Bags_Shop_API.UnitOfWorkService;
using LinqKit;
using MediatR;

namespace Bags_Shop_API.Services.DiscountServices.Queries
{
    public class GetAllDiscountsQueryHandler : IRequestHandler<GetAllDiscountsQuery, Result<List<DiscountDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDiscountMapper _discountMapper;

        public GetAllDiscountsQueryHandler(IUnitOfWork unitOfWork, IDiscountMapper discountMapper)
        {
            _unitOfWork = unitOfWork;
            _discountMapper = discountMapper;
        }

        public async Task<Result<List<DiscountDto>>> Handle(GetAllDiscountsQuery request, CancellationToken cancellationToken)
        {
            var spec = new BaseSpecification<Discount>();
            var criteria = PredicateBuilder.New<Discount>(true);

            if (request.IsActive.HasValue)
            {
               criteria = criteria.And(d => d.IsActive == request.IsActive.Value);
            }

            if (request.OnlyValid.HasValue && request.OnlyValid.Value)
            {
                var now = DateTime.Now;
                criteria = criteria.And(d => d.StartDate <= now && d.EndDate >= now);
            }

            spec.Criteria = criteria;


            if (request.PageNumber.HasValue && request.PageSize.HasValue)
            {
                spec.ApplyPaging(request.PageNumber.Value, request.PageSize.Value);
            }

            var discounts = await _unitOfWork.Discounts.GetAllAsync(spec);

            if (discounts == null || !discounts.Any())
                return Result<List<DiscountDto>>.Fail("No discounts found", 404);

            var discountDtos = discounts.Select(d => _discountMapper.ToDto(d)).ToList();

            return Result<List<DiscountDto>>.Ok(discountDtos);
        }
    }
}
