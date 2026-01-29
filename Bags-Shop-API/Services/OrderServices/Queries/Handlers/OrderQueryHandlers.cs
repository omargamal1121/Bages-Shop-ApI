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
            var spec = new OrderWithItemsProjectionSpec(request.Id);
            var order = await _unitOfWork.Orders.GetByIdAsync(spec);

            if (order == null)
                return Result<OrderDto>.Fail("Order not found.", 404);

            return Result<OrderDto>.Ok(order);
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
            var spec = new OrderWithFiltersProjectionSpec(
                request.CreatedFrom, 
                request.CreatedTo, 
                request.Status,
                request.Page,
                request.PageSize);
            
            var orders = await _unitOfWork.Orders.GetAllAsync(spec);

            return Result<List<OrderDto>>.Ok(orders);
        }
    }

    public class GetOrdersByUserKeyQueryHandler : IRequestHandler<GetOrdersByUserKeyQuery, Result<List<OrderDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetOrdersByUserKeyQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<OrderDto>>> Handle(GetOrdersByUserKeyQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserKey))
                return Result<List<OrderDto>>.Fail("User key is required.", 400);

            var spec = new OrdersByUserKeyProjectionSpec(
                request.UserKey, 
                request.CreatedFrom, 
                request.CreatedTo, 
                request.Status, 
                request.Page, 
                request.PageSize);
            
            var orders = await _unitOfWork.Orders.GetAllAsync(spec);

            return Result<List<OrderDto>>.Ok(orders);
        }
    }

    public class GetOrderByIdAndUserKeyQueryHandler : IRequestHandler<GetOrderByIdAndUserKeyQuery, Result<OrderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetOrderByIdAndUserKeyQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<OrderDto>> Handle(GetOrderByIdAndUserKeyQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserKey))
                return Result<OrderDto>.Fail("User key is required.", 400);

            var spec = new OrderByIdAndUserKeyProjectionSpec(request.Id, request.UserKey);
            var order = await _unitOfWork.Orders.GetByIdAsync(spec);

            if (order == null)
                return Result<OrderDto>.Fail("Order not found.", 404);

            return Result<OrderDto>.Ok(order);
        }
    }
}
