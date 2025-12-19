using Bags_Shop_API.Services.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bags_Shop_API.Services.CollectionServices.Queries
{
    public class GetAllCollectionsQuery : IRequest<Result<List<CollectionDto>>>, ICacheableQuery
    {
        public bool? IsActive { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public string? SearchName { get; set; }

        [BindNever]
        public bool IsAdminRequest { get; set; } = false;

        [BindNever]
        public string CacheKey => $"GetAllCollectionsQuery-{IsActive}-{PageNumber}-{PageSize}-{IsAdminRequest}-{SearchName}";
        
        [BindNever]
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
    }
}
