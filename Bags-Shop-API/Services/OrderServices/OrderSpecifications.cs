using Bags_Shop_API.Models;
using Bags_Shop_API.Specification;

namespace Bags_Shop_API.Services.OrderServices
{
    public class OrderWithItemsSpec : BaseSpecification<Order>
    {
        public OrderWithItemsSpec() : base()
        {
            AddInclude(o => o.OrderItems);
            AddInclude("OrderItems.Product");
            AddInclude(o => o.Payments);
        }

        public OrderWithItemsSpec(int id) : base(o => o.Id == id)
        {
            AddInclude(o => o.OrderItems);
            AddInclude("OrderItems.Product");
            AddInclude(o => o.Payments);
        }
    }
}
