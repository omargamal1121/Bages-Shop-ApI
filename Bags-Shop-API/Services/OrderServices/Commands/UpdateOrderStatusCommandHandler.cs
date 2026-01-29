using Bags_Shop_API.ContextFile;
using Bags_Shop_API.Models;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bags_Shop_API.Services.OrderServices.Commands
{
    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<bool>>
    {
        
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderStatusCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
   
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);


            if (order == null)
            {
                return Result<bool>.Fail("Order not found", 404);
            }
          
          
            if (!IsValidStatusTransition(order.Status, request.Status))
            {
                return Result<bool>.Fail(
                    $"Invalid status transition from {order.Status} to {request.Status}", 
                    400);
            }

 
            order.Status = request.Status;

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Ok(true, "Order status updated successfully");
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Failed to update order status: {ex.Message}", 500);
            }
        }

        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            if (currentStatus == OrderStatus.Cancelled)
            {
                return false;
            }

         
            if (newStatus == OrderStatus.Pending && currentStatus != OrderStatus.Pending)
            {
                return false;
            }

           
            if (currentStatus == OrderStatus.Expired && newStatus != OrderStatus.Cancelled)
            {
                return false;
            }

            return true;
        }
    }
}
