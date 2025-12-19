using Bags_Shop_API.Services.ProductServices.ProductsDtos;
using Bags_Shop_API.Services.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bags_Shop_API.Services.ProductServices.Queries
{
    public class GetProductByIdQuery : IRequest<Result<ProductDto>>, ICacheableQuery
    {
        public int Id { get; set; }

		public bool IsAdmin { get; set; }

		public GetProductByIdQuery(int id, bool isadmin = false)
        {
            IsAdmin= isadmin;

            Id = id;
        }

        [BindNever]
        public string CacheKey => $"GetProductByIdQuery-{Id}-{IsAdmin}";
        
        [BindNever]
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
    }
}
