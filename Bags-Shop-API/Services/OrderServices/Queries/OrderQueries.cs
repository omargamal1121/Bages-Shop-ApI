using Bags_Shop_API.Services.OrderServices.Dtos;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.OrderServices.Queries
{
    public class GetOrderByIdQuery : IRequest<Result<OrderDto>>
    {
        public int Id { get; }
        public GetOrderByIdQuery(int id) => Id = id;
    }

    public class GetAllOrdersQuery : IRequest<Result<List<OrderDto>>>
    {
        // Add pagination or filters if needed
    }
}
