using Bags_Shop_API.Services.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bags_Shop_API.Services.DiscountServices.Queries
{
    public class GetDiscountByIdQuery : IRequest<Result<DiscountDto>>, ICacheableQuery
    {
        public int Id { get; set; }

        [BindNever]
        public bool IsAdminRequest { get; set; } = false;

        public GetDiscountByIdQuery(int id, bool isAdminRequest = false)
        {
            Id = id;
            IsAdminRequest = isAdminRequest;
        }

        [BindNever]
        public string CacheKey => $"GetDiscountByIdQuery-{Id}-{IsAdminRequest}";
        
        [BindNever]
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
    }
}
