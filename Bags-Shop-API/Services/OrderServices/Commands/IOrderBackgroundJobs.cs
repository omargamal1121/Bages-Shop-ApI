namespace Bags_Shop_API.Services.OrderServices.Commands
{
	public interface IOrderBackgroundJobs
    {
        Task CheckOnOrderAsync(int orderId);
    }




}
