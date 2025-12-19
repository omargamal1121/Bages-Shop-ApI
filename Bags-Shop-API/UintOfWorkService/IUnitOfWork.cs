using Bags_Shop_API.Models;
using Bags_Shop_API.Repo;

namespace Bags_Shop_API.UnitOfWorkService
{
	public interface IUnitOfWork : IDisposable
    {
        // Repositories
        public IMainRepository<Product> Products { get; }
        public IMainRepository<Image> Images { get; }
        public IMainRepository<Order> Orders { get; }
        public IMainRepository<OrderItem> OrderItems { get; }
        public IMainRepository<Collection> Collections { get; }
        public IMainRepository<Discount> Discounts { get; }
        public IMainRepository<Payment> Payments { get; set; }
        public IMainRepository<PaymentWebhook>  Webhook { get; set; }
        Task<int> SaveChangesAsync();
        int SaveChanges();

        // Transactions
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}