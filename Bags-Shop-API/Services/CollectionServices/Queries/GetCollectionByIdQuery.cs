using Bags_Shop_API.Services.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bags_Shop_API.Services.CollectionServices.Queries
{
    public class GetCollectionByIdQuery : IRequest<Result<CollectionDto>>, ICacheableQuery
    {
        public int Id { get; set; }
		public bool? IsActive { get; set; }
        
        [BindNever]
        public bool IsAdminRequest { get; set; } = false;

		public GetCollectionByIdQuery(int id, bool? isactive=null, bool isAdminRequest = false)
        {
            IsActive = isactive;
            Id = id;
            IsAdminRequest = isAdminRequest;
        }

        [BindNever]
        public string CacheKey => $"GetCollectionByIdQuery-{Id}-{IsActive}-{IsAdminRequest}";
        
        [BindNever]
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
    }
}
