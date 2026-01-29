using Bags_Shop_API.Models;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;

namespace Bags_Shop_API.Services.OrderServices
{
    public interface IOrderServices
    {
        Task<Result<bool>> UpdateOrderAfterPaid(int orderId, OrderStatus status);
        Task ConfirmOrderAsync(int orderId, string userId, bool isPaid, bool isCash, string? transactionId); 
    }

    public class OrderServices : IOrderServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderServices> _logger;

        public OrderServices(IUnitOfWork unitOfWork, ILogger<OrderServices> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<bool>> UpdateOrderAfterPaid(int orderId, OrderStatus status)
        {
             try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                if (order == null)
                    return Result<bool>.Fail("Order not found");

                order.Status = status;
          
                
                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status to {Status}", orderId, status);
                return Result<bool>.Fail(ex.Message);
            }
        }

        public async Task ConfirmOrderAsync(int orderId, string userId, bool isPaid, bool isCash, string? transactionId)
        {
            _logger.LogInformation("Starting ConfirmOrderAsync for Order {OrderId}. isPaid: {isPaid}, isCash: {isCash}", orderId, isPaid, isCash);

            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found for confirmation", orderId);
                    return;
                }

                if (order.Status != OrderStatus.Pending)
                {
                    _logger.LogInformation("Order {OrderId} is already confirmed or processed (Status: {Status})", orderId, order.Status);
                    return;
                }

                order.Status = isCash ? OrderStatus.Processing : (isPaid ? OrderStatus.Paid : OrderStatus.Pending);
                
    
                if (order.Status != OrderStatus.Pending)
                {
                    _logger.LogInformation("Order {OrderId} processed. Status updated to {Status}", orderId, order.Status);
                }

                await _unitOfWork.SaveChangesAsync();

              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order {OrderId}", orderId);
                throw;
            }
        }
    }
}
