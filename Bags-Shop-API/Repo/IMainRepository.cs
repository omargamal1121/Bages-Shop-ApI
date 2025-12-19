using Bags_Shop_API.Specification;

namespace Bags_Shop_API.Repo
{
    public interface IMainRepository<T> where T : class
    {
        Task<List<TResult>> GetAllAsync<TResult>(
            ISpecificationWithProjection<T, TResult> specification);
        Task<TResult?> GetByIdAsync<TResult>(
            ISpecificationWithProjection<T, TResult> specification);

       
        Task<List<T>> GetAllAsync(ISpecification<T> specification);
        Task<T?> GetByIdAsync(ISpecification<T> specification);
        Task<T?> GetByIdAsync(int id);
        Task<int> CountAsync(ISpecification<T> specification);
        Task<bool> AnyAsync(ISpecification<T> specification);

       
        Task<T> AddAsync(T item);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> items);
        void Update(T item);
        void Remove(T item);
        void RemoveRange(IEnumerable<T> items);
        Task<int> ExecuteDeleteAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
    }

}
