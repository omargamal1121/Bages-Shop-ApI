using Microsoft.EntityFrameworkCore;

namespace Bags_Shop_API.Specification
{
	public class SpecificationWithProjectionEvaluator<T, TResult> where T : class
        {
            public static IQueryable<TResult> GetQuery(
                IQueryable<T> inputQuery,
                ISpecificationWithProjection<T, TResult> spec)
            {
                var query = inputQuery;

             
                if (spec.Criteria != null)
                    query = query.Where(spec.Criteria);
                if (spec.OrderBy != null)
                    query = query.OrderBy(spec.OrderBy);
                else if (spec.OrderByDescending != null)
                    query = query.OrderByDescending(spec.OrderByDescending);

                var projectedQuery = query.Select(spec.Projection);

               
                if (spec.Paging != null)
                    projectedQuery = projectedQuery
                        .Skip(spec.Paging.Skip)
                        .Take(spec.Paging.PageSize);

                return projectedQuery;
            }
        }
    }

