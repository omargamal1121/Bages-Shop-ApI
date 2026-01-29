using Bags_Shop_API.Models;
using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.UnitOfWorkService;
using Microsoft.Extensions.Caching.Memory;

namespace Bags_Shop_API.Services.OrderServices.Commands
{
	public class OrderBackgroundJobs : IOrderBackgroundJobs
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheTokenProvider _tokenProvider;

        public OrderBackgroundJobs(IUnitOfWork unitOfWork,ICacheTokenProvider  cacheTokenProvider)
        {
            _tokenProvider = cacheTokenProvider; 
            _unitOfWork = unitOfWork;
        }

        public async Task CheckOnOrderAsync(int orderId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null) return;

            if (order.Status == OrderStatus.Pending &&
                order.ExpiresAt < DateTime.UtcNow)
            {
                order.Status = OrderStatus.Expired;
                _tokenProvider.Reset();
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }



}
