using Bags_Shop_API.Models;
using Bags_Shop_API.Services.DiscountServices;
using Bags_Shop_API.Services.ImageServices;
using Bags_Shop_API.Services.ProductServices.ProductsDtos;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Specification;
using Bags_Shop_API.UnitOfWorkService;
using LinqKit;
using MediatR;

namespace Bags_Shop_API.Services.ProductServices.Queries
{
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<List<ProductDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAllProductsQueryHandler> _logger;
        public GetAllProductsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllProductsQueryHandler> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
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

                FinalPrice = p.Price,

                Images = p.Images.Select(i => new ImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    CloudinaryPublicId = i.CloudinaryPublicId
                }).ToList()
            });


            var criteria = PredicateBuilder.New<Product>(true);

    
            if (!string.IsNullOrWhiteSpace(request.SearchName))
            {
                var searchTerm = request.SearchName.ToLower();
                criteria = criteria.And(p => p.ArName.ToLower().Contains(searchTerm) ||
                                              p.EnName.ToLower().Contains(searchTerm));
            }

            if (request.IsActive.HasValue)
            {
                var isActive = request.IsActive.Value; 
                criteria = criteria.And(p => p.IsActive == isActive);
            }

    
            if (request.CollectionId.HasValue)
            {
                var collectionId = request.CollectionId.Value;
                criteria = criteria.And(p => p.CollectionId == collectionId);
            }

      
            if (request.DiscountId.HasValue)
            {
                var discountId = request.DiscountId.Value;
                criteria = criteria.And(p => p.DiscountId == discountId);
            }

   
            spec.Criteria = criteria;

        
            spec.ApplyOrderBy(p => p.Id); 

       
            if (request.PageNumber.HasValue && request.PageSize.HasValue)
            {
                spec.ApplyPaging(request.PageNumber.Value, request.PageSize.Value);
            }

 
            var productDtos = await _unitOfWork.Products.GetAllAsync(spec);

            foreach (var product in productDtos)
            {
                if (!request.IsAdminRequest && product.Discount?.IsActive == false)
                {
                    product.Discount = null;
                }


                product.FinalPrice = product.Discount != null && product.Discount.IsActive
                    ? product.Price - (product.Price * product.Discount.DiscountPercentage / 100)
                    : product.Price;
            }

            if (productDtos == null || !productDtos.Any())
                return Result<List<ProductDto>>.Fail("No products found", 404);

            return Result<List<ProductDto>>.Ok(productDtos);
        }
    }
    }
