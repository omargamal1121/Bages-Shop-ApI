using Bags_Shop_API.Models;
using Bags_Shop_API.Services.ProductServices.ProductsDtos;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Specification;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.DiscountServices.Queries
{
    public class GetDiscountByIdQueryHandler : IRequestHandler<GetDiscountByIdQuery, Result<DiscountDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDiscountByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DiscountDto>> Handle(GetDiscountByIdQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            var spec = new BaseSpecificationWithProjection<Discount, DiscountDto>(d => new DiscountDto
            {
                Id = d.Id,
                DiscountPercentage = d.DiscountPercentage,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
             
                IsActive = d.IsActive && d.StartDate <= now && d.EndDate >= now,
                Products = d.Products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    ArName = p.ArName,
                    EnName = p.EnName,
                    ArDescription = p.ArDescription,
                    EnDescription = p.EnDescription,
                    Price = p.Price,
                    IsActive = p.IsActive
                }).ToList()
            });

            spec.Criteria = d => d.Id == request.Id;

            var discountDtos = await _unitOfWork.Discounts.GetAllAsync(spec);
            var discountDto = discountDtos.FirstOrDefault();

            if (discountDto == null)
                return Result<DiscountDto>.Fail($"Discount with ID {request.Id} not found", 404);

            if (!request.IsAdminRequest && !discountDto.IsActive)
                 return Result<DiscountDto>.Fail($"Discount with ID {request.Id} not found or not available", 404);

            return Result<DiscountDto>.Ok(discountDto);
        }
    }
}
