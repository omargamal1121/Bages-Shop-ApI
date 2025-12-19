namespace Bags_Shop_API.Services.DiscountServices.Jobs
{
    public interface IDiscountSchedulerService
    {
        void ScheduleActivation(int discountId, DateTime startDate);
        void ScheduleExpiration(int discountId, DateTime endDate);
        void CancelScheduledJobs(int discountId);
    }
}
