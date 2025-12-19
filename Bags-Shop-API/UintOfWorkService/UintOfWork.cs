using Bags_Shop_API.ContextFile;
using Bags_Shop_API.Models;
using Bags_Shop_API.Repo;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Bags_Shop_API.UnitOfWorkService
{

    public class UnitOfWork : IUnitOfWork
    {
        private readonly Context _context;
        private IDbContextTransaction? _transaction;
        private int _transactionCounter = 0;
        private readonly ILogger<UnitOfWork>? _logger;


        public IMainRepository<Product> Products { get;  }
        public IMainRepository<Image> Images { get; }
        public IMainRepository<Order> Orders { get; }
        public IMainRepository<OrderItem> OrderItems { get; }
        public IMainRepository<Collection> Collections { get; }
        public IMainRepository<Discount> Discounts { get; }
		public IMainRepository<Payment>  Payments  { get; set; }
        public IMainRepository<PaymentWebhook> Webhook { get; set; }


        public UnitOfWork(
            IMainRepository<Discount> _discount,
            IMainRepository<Payment> _Payments,
            IMainRepository<Collection> _collections,
            IMainRepository<OrderItem> _orderItems,
            IMainRepository<Order> _orders, 
            IMainRepository<Image> _images,
            IMainRepository<Product> _products,
            IMainRepository<PaymentWebhook> _webhook,
            Context context, 
            ILogger<UnitOfWork>? logger = null)
        {
            Payments=_Payments;
            Products =_products;
            Images=_images;
            OrderItems=_orderItems;
            Orders=_orders;
            Collections=_collections;
            _logger=logger;
            Discounts=_discount;
            Webhook=_webhook;

            _context = context;
    
        }


      

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger?.LogError(ex, "Concurrency conflict occurred while saving changes");

             
                foreach (var entry in ex.Entries)
                {
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    if (databaseValues == null)
                    {
                        throw new InvalidOperationException(
                            "Unable to save changes. The record was deleted by another user.", ex);
                    }

                    throw new InvalidOperationException(
                        "Unable to save changes. The record was modified by another user. " +
                        "Please refresh and try again.", ex);
                }

                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger?.LogError(ex, "Database update error occurred");

                var innerMessage = ex.InnerException?.Message?.ToLower() ?? string.Empty;

   
                if (innerMessage.Contains("foreign key") || innerMessage.Contains("reference"))
                {
                    throw new InvalidOperationException(
                        "Cannot save changes. The referenced record does not exist.", ex);
                }

            
                if (innerMessage.Contains("unique") || innerMessage.Contains("duplicate"))
                {
                    throw new InvalidOperationException(
                        "Cannot save changes. A record with this value already exists.", ex);
                }

         
                if (innerMessage.Contains("check constraint"))
                {
                    throw new InvalidOperationException(
                        "Cannot save changes. Data validation failed.", ex);
                }

             
                if (innerMessage.Contains("delete") || innerMessage.Contains("conflicted"))
                {
                    throw new InvalidOperationException(
                        "Cannot delete. This record is being used by other records.", ex);
                }

                throw new InvalidOperationException(
                    "An error occurred while saving to the database. Please check your data and try again.", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error occurred while saving changes");
                throw new InvalidOperationException(
                    "An unexpected error occurred while saving changes.", ex);
            }
        }

        public int SaveChanges()
        {
            try
            {
                return _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger?.LogError(ex, "Concurrency conflict occurred");
                throw new InvalidOperationException(
                    "Unable to save. The data was modified by another user.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger?.LogError(ex, "Database update error occurred");

                var innerMessage = ex.InnerException?.Message?.ToLower() ?? string.Empty;

                if (innerMessage.Contains("foreign key"))
                {
                    throw new InvalidOperationException(
                        "Cannot save. Referenced record does not exist.", ex);
                }

                if (innerMessage.Contains("unique") || innerMessage.Contains("duplicate"))
                {
                    throw new InvalidOperationException(
                        "Cannot save. A record with this value already exists.", ex);
                }

                throw new InvalidOperationException(
                    "An error occurred while saving to the database.", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error occurred");
                throw new InvalidOperationException(
                    "An unexpected error occurred while saving changes.", ex);
            }
        }

       
        public async Task BeginTransactionAsync()
        {
            try
            {
                if (_transaction == null)
                {
                    _transaction = await _context.Database.BeginTransactionAsync();
                }
                _transactionCounter++;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error beginning transaction");
                throw new InvalidOperationException("Failed to begin transaction.", ex);
            }
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("No active transaction to commit.");
                }

                _transactionCounter--;

                if (_transactionCounter == 0)
                {
                    await _context.SaveChangesAsync();
                    await _transaction.CommitAsync();
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error committing transaction");
                await RollbackTransactionAsync();
                throw;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                    await _transaction.DisposeAsync();
                }
            }
            finally
            {
                _transaction = null;
                _transactionCounter = 0;
            }
        }

        public void Dispose()
        {
            try
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during dispose");

            }
        }
    }
}