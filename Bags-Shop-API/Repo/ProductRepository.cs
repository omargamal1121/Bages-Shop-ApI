using Bags_Shop_API.ContextFile;
using Bags_Shop_API.Specification;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Bags_Shop_API.Repo
{
	public class ProductRepository:MainRepository<Product>
	{
		private readonly Context _context;

		public ProductRepository(Context context):base(context)
		{
			_context = context;
		}
	
	}
}
