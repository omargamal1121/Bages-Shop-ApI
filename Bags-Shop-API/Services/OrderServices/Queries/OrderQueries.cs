using Bags_Shop_API.Models;
using Bags_Shop_API.Services.OrderServices.Dtos;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.OrderServices.Queries
{
    public class GetOrderByIdQuery : IRequest<Result<OrderDto>>, ICacheableQuery
    {
        public int Id { get; }
        public GetOrderByIdQuery(int id) => Id = id;

        public string CacheKey => $"OrderById_{Id}";
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
    }

    public class GetAllOrdersQuery : IRequest<Result<List<OrderDto>>>, ICacheableQuery
    {
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public OrderStatus? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string CacheKey => $"AllOrders_{CreatedFrom}_{CreatedTo}_{Status}_{Page}_{PageSize}";
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
    }

    public class GetOrdersByUserKeyQuery : IRequest<Result<List<OrderDto>>>, ICacheableQuery
    {
        public string UserKey { get; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public OrderStatus? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public GetOrdersByUserKeyQuery(string userKey)
        {
            UserKey = userKey;
        }

        public string CacheKey => $"OrdersByUser_{UserKey}_{CreatedFrom}_{CreatedTo}_{Status}_{Page}_{PageSize}";
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
    }

    public class GetOrderByIdAndUserKeyQuery : IRequest<Result<OrderDto>>, ICacheableQuery
    {
        public int Id { get; }
        public string UserKey { get; }

        public GetOrderByIdAndUserKeyQuery(int id, string userKey)
        {
            Id = id;
            UserKey = userKey;
        }

        public string CacheKey => $"OrderByIdAndUser_{Id}_{UserKey}";
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
    }
}
