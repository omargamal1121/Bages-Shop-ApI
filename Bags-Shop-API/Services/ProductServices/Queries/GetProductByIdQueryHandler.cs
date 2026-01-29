using Bags_Shop_API.Models;
using Bags_Shop_API.Services.DiscountServices;
using Bags_Shop_API.Services.ImageServices;
using Bags_Shop_API.Services.ProductServices.ProductsDtos;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Specification;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.ProductServices.Queries
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProductByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var spec = new BaseSpecificationWithProjection<Product, ProductDto>(p => new ProductDto
            {
                ArDescription = p.ArDescription,
                ArName = p.ArName,
                EnDescription = p.EnDescription,
                EnName = p.EnName,
                Price = p.Price,
                Id = p.Id,
                IsActive = p.IsActive,

                Discount = p.Discount != null ? new DiscountDto
                {
                    Id = p.Discount.Id,
                    DiscountPercentage = p.Discount.DiscountPercentage,
                    StartDate = p.Discount.StartDate,
                    EndDate = p.Discount.EndDate,
                    IsActive = p.Discount.IsActive
                } : null,

                Images = p.Images.Select(i => new ImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    CloudinaryPublicId = i.CloudinaryPublicId
                }).ToList()
            });

            spec.Criteria = p => p.Id == request.Id&&p.Delete_AT==null;

            var products = await _unitOfWork.Products.GetAllAsync(spec);
            var productDto = products.FirstOrDefault();

            if (productDto == null)
                return Result<ProductDto>.Fail($"Product with ID {request.Id} not found", 404);


            if (!request.IsAdmin)
            {
                if (!productDto.IsActive)
                    return Result<ProductDto>.Fail($"Product with ID {request.Id} not found", 404);

             
                if (productDto.Discount != null && !productDto.Discount.IsActive)
                {
                    productDto.Discount = null;
                }
}

     
            productDto.FinalPrice = productDto.Discount != null && productDto.Discount.IsActive
                ? productDto.Price - (productDto.Price * productDto.Discount.DiscountPercentage / 100)
                : productDto.Price;

            return Result<ProductDto>.Ok(productDto);
        }
    }
}