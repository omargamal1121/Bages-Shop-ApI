using Bags_Shop_API.Services.DiscountServices;
using Bags_Shop_API.Services.ImageServices;
using Bags_Shop_API.Services.ProductServices;
using Bags_Shop_API.Services.ProductServices.ProductsDtos;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Specification;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;
using LinqKit;

namespace Bags_Shop_API.Services.CollectionServices.Queries
{
    public class GetCollectionByIdQueryHandler : IRequestHandler<GetCollectionByIdQuery, Result<CollectionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICollectionMapper _collectionMapper;

        public GetCollectionByIdQueryHandler(IUnitOfWork unitOfWork, ICollectionMapper collectionMapper)
        {
            _unitOfWork = unitOfWork;
            _collectionMapper = collectionMapper;
        }

        public async Task<Result<CollectionDto>> Handle(GetCollectionByIdQuery request, CancellationToken cancellationToken)
        {
            var spec = new BaseSpecificationWithProjection<Collection, CollectionDto>(c => new CollectionDto
            {
                 ArDescription= c.ArDescription,
                 EnDescription= c.EnDescription,
                 ArName= c.ArName,
                 EnName= c.EnName,
                 Id= c.Id,
                 IsActive= c.IsActive,
                Images=c.Images.Select(i => new ImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    CloudinaryPublicId = i.CloudinaryPublicId
                }).ToList(),
                products =c.Products.Where(p => request.IsAdminRequest || p.IsActive).Select(p=> new ProductDto
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
                     }).ToList(),
                     

                 }
                 
                 ).ToList(),
            });
            if(request.IsActive is null)
            spec.Criteria = c => c.Id == request.Id;
            else spec.Criteria = c => c.Id == request.Id&&c.IsActive==request.IsActive  ;

            var collection =( await _unitOfWork.Collections.GetAllAsync<CollectionDto>(spec)).FirstOrDefault();
          
            if (collection == null)
                return Result<CollectionDto>.Fail($"Collection with ID {request.Id} not found", 404);

            return Result<CollectionDto>.Ok(collection);
        }
    }
}
