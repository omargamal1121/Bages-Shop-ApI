using Bags_Shop_API.Models;
using Bags_Shop_API.Services.Shared;

namespace Bags_Shop_API.Services.DiscountServices
{
    public interface IDiscountFactory
    {
        Result<Discount> CreateDiscount(decimal discountPercentage, DateTime startDate, DateTime endDate);
    }

    public class DiscountFactory : IDiscountFactory
    {
        public Result<Discount> CreateDiscount(decimal discountPercentage, DateTime startDate, DateTime endDate)
        {
            if (discountPercentage <= 0 || discountPercentage >= 90)
                return Result<Discount>.Fail("Discount percentage must be between 1 and 90");

            if (startDate >= endDate)
                return Result<Discount>.Fail("Start date must be before end date");

            if (endDate <= DateTime.Now)
                return Result<Discount>.Fail("End date must be in the future");

            var discount = new Discount
            {
                DiscountPercentage = discountPercentage,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = false
            };

            return Result<Discount>.Ok(discount);
        }
    }
}
