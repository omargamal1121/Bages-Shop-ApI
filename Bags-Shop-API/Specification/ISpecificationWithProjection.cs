using System.Linq.Expressions;

namespace Bags_Shop_API.Specification
{
	public interface ISpecificationWithProjection<T, TResult>: ISpecification<T>
	{
        public Expression<Func<T, TResult>> Projection { get; set; }
    }
}