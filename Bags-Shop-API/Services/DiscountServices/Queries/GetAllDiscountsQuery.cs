using Bags_Shop_API.Services.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bags_Shop_API.Services.DiscountServices.Queries
{
    public class GetAllDiscountsQuery : IRequest<Result<List<DiscountDto>>>, ICacheableQuery
    {
        public bool? IsActive { get; set; }
        public bool? OnlyValid { get; set; } // Only return discounts that are currently valid (within date range)
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }

        [BindNever]
        public bool IsAdminRequest { get; set; } = false;

        [BindNever]
        public string CacheKey => $"GetAllDiscountsQuery-{IsActive}-{OnlyValid}-{PageNumber}-{PageSize}-{IsAdminRequest}";
        
        [BindNever]
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
    }
}
