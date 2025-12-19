using Bags_Shop_API.Models;
using Bags_Shop_API.Services.OrderServices.Dtos;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.OrderServices.Queries.Handlers
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var spec = new OrderWithItemsSpec(request.Id);
            var order = await _unitOfWork.Orders.GetByIdAsync(spec);

            if (order == null)
                return Result<OrderDto>.Fail("Order not found.", 404);

            var dto = MapToDto(order);
            return Result<OrderDto>.Ok(dto);
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                Address = order.Address,
                Phone = order.Phone,
                Status = order.Status.ToString(),
                FinalPrice = order.FinalPrice,
                CreatedAt = order.CreatedAt,
                ExpiresAt = order.ExpiresAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemSummaryDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.EnName ?? "Unknown Product", // Assuming EnName exists
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList(),
                Payments = order.Payments?.Select(p => new PaymentSummaryDto
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Method = p.Method.ToString(),
                    Status = p.Status.ToString(),
                    TransactionId = p.TransactionId,
                    CreatedAt = p.CreatedAt
                }).ToList() ?? new()
            };
        }
    }

    public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<List<OrderDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllOrdersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<OrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            var spec = new OrderWithItemsSpec();
            var orders = await _unitOfWork.Orders.GetAllAsync(spec);

            var dtos = orders.Select(MapToDto).ToList();
            return Result<List<OrderDto>>.Ok(dtos);
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                Address = order.Address,
                Phone = order.Phone,
                Status = order.Status.ToString(),
                FinalPrice = order.FinalPrice,
                CreatedAt = order.CreatedAt,
                ExpiresAt = order.ExpiresAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemSummaryDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.EnName ?? "Unknown Product",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            };
        }
    }
}
