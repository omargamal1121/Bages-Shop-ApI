using Bags_Shop_API.Services.ProductServices.ProductsDtos;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Specification;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bags_Shop_API.Services.ProductServices.Queries
{

    public class GetAllProductsQuery : IRequest<Result<List<ProductDto>>>, ICacheableQuery
    {
        public string? SearchName { get; set; }
        public bool? IsActive { get; set; }
        public int? CollectionId { get; set; }
        public int? DiscountId { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        
        [BindNever]
        public bool IsAdminRequest { get; set; } = false;

        [BindNever]
        public string CacheKey => $"GetAllProductsQuery-{SearchName}-{IsActive}-{CollectionId}-{DiscountId}-{PageNumber}-{PageSize}-{IsAdminRequest}";
        
        [BindNever]
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
    }
}
