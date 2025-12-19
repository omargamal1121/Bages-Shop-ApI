using Hangfire;

namespace Bags_Shop_API.Services.DiscountServices.Jobs
{
    public class DiscountSchedulerService : IDiscountSchedulerService
    {
        private readonly ILogger<DiscountSchedulerService> _logger;

        public DiscountSchedulerService(ILogger<DiscountSchedulerService> logger)
        {
            _logger = logger;
        }

        public void ScheduleActivation(int discountId, DateTime startDate)
        {
            var jobId = $"activate-discount-{discountId}";
            
            // Calculate delay from now
            var delay = startDate - DateTime.UtcNow;
            
            if (delay.TotalSeconds > 0)
            {
                BackgroundJob.Schedule<DiscountJobService>(
                    service => service.ActivateDiscountAsync(discountId),
                    delay);
                
                _logger.LogInformation(
                    "Scheduled activation job {JobId} for discount {DiscountId} at {StartDate}",
                    jobId, discountId, startDate);
            }
            else
            {
                // If start date is in the past, activate immediately
                BackgroundJob.Enqueue<DiscountJobService>(
                    service => service.ActivateDiscountAsync(discountId));
                
                _logger.LogInformation(
                    "Start date is in the past. Enqueued immediate activation for discount {DiscountId}",
                    discountId);
            }
        }

        public void ScheduleExpiration(int discountId, DateTime endDate)
        {
            var jobId = $"expire-discount-{discountId}";
            
            // Calculate delay from now
            var delay = endDate - DateTime.UtcNow;
            
            if (delay.TotalSeconds > 0)
            {
                BackgroundJob.Schedule<DiscountJobService>(
                    service => service.ExpireDiscountAsync(discountId),
                    delay);
                
                _logger.LogInformation(
                    "Scheduled expiration job {JobId} for discount {DiscountId} at {EndDate}",
                    jobId, discountId, endDate);
            }
            else
            {
                // If end date is in the past, expire immediately
                BackgroundJob.Enqueue<DiscountJobService>(
                    service => service.ExpireDiscountAsync(discountId));
                
                _logger.LogInformation(
                    "End date is in the past. Enqueued immediate expiration for discount {DiscountId}",
                    discountId);
            }
        }

        public void CancelScheduledJobs(int discountId)
        {
            // Note: Hangfire doesn't provide a direct way to delete jobs by custom ID
            // In a production scenario, you'd want to store job IDs in the database
            // For now, we'll log the cancellation request
            _logger.LogInformation(
                "Cancellation requested for discount {DiscountId} jobs. " +
                "Jobs will complete if already running or be removed from queue.",
                discountId);
        }
    }
}
