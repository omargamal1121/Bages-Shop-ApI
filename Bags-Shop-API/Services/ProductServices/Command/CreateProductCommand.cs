using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.ProductServices.Command
{
	public class CreateProductCommand:IRequest<Result<int>>, IInvalidateCache
	{
        public bool InvalidateAll => true;
      
        public string ArName { get; set; }

        public string EnName { get; set; }

        public string ArDescription { get; set; }

        public string EnDescription { get; set; }
		public decimal Price { get; set; }



	}

}


