using System.Linq.Expressions;

namespace Bags_Shop_API.Specification
{
    public interface ISpecification<T>
    {
        
      public  Expression<Func<T, bool>>? Criteria { get; set; }


        public Pagination? Paging { get; set; }


        public Expression<Func<T, object>>? OrderBy { get; set; }
public        Expression<Func<T, object>>? OrderByDescending { get; set; }


      public  List<Expression<Func<T, object>>>? Includes { get; set;  }
      public List<string> IncludeStrings { get; set; } 


    }


}
