using Bags_Shop_API.Services.DiscountServices;
using Bags_Shop_API.Services.ImageServices;
using Bags_Shop_API.Services.ProductServices;
using Bags_Shop_API.Services.ProductServices.ProductsDtos;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Specification;
using Bags_Shop_API.UnitOfWorkService;
using LinqKit;
using MediatR;
using System.Linq.Expressions;

namespace Bags_Shop_API.Services.CollectionServices.Queries
{
    public class GetAllCollectionsQueryHandler : IRequestHandler<GetAllCollectionsQuery, Result<List<CollectionDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllCollectionsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<CollectionDto>>> Handle(GetAllCollectionsQuery request, CancellationToken cancellationToken)
        {
            var spec = new BaseSpecificationWithProjection<Collection, CollectionDto>(c => new CollectionDto
            {
                ArDescription = c.ArDescription,
                EnDescription = c.EnDescription,
                ArName = c.ArName,
                EnName = c.EnName,
                Id = c.Id,
                IsActive = c.IsActive,
                products = c.Products.Where(p => request.IsAdminRequest || p.IsActive).Select(p => new ProductDto
                {
                    ArDescription = p.ArDescription,
                    ArName = p.ArName,
                    EnDescription = p.EnDescription,
                    EnName = p.EnName,
                    Price = p.Price,
                    Id = p.Id,
                   
                    IsActive = p.IsActive,
                    Discount = (p.Discount != null && (request.IsAdminRequest || p.Discount.IsActive)) ? new DiscountDto
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
                }).ToList(),
                Images = c.Images.Select(i => new ImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    CloudinaryPublicId = i.CloudinaryPublicId
                }).ToList()
            });
            Expression<Func<Collection, bool>>? criteria = PredicateBuilder.New<Collection>(true); 
            
            if (request.IsActive.HasValue)
            {
              criteria =  criteria.And ( c => c.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchName))
            {
                var searchTerm = request.SearchName.ToLower();
                criteria = criteria.And(c => c.ArName.ToLower().Contains(searchTerm) || c.EnName.ToLower().Contains(searchTerm));
            }

            if (request.PageNumber.HasValue && request.PageSize.HasValue)
            {
                spec.ApplyPaging(request.PageNumber.Value, request.PageSize.Value);
            }

            var collectionDtos = await _unitOfWork.Collections.GetAllAsync(spec);

            if (collectionDtos == null || !collectionDtos.Any())
                return Result<List<CollectionDto>>.Fail("No collections found", 404);

            return Result<List<CollectionDto>>.Ok(collectionDtos);
        }
    }
}
