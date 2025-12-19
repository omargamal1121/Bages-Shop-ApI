using Bags_Shop_API.Models;

namespace Bags_Shop_API.Services.DiscountServices
{
    public interface IDiscountMapper
    {
        DiscountDto ToDto(Discount discount);
    }

    public class DiscountMapper : IDiscountMapper
    {
        public DiscountDto ToDto(Discount discount)
        {
            return new DiscountDto
            {
                Id = discount.Id,
                DiscountPercentage = discount.DiscountPercentage,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                IsActive = discount.IsActive
            };
        }
    }
}
