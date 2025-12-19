using Bags_Shop_API.Services.ProductServices.ProductsDtos;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Specification;
using Bags_Shop_API.Services.Behaviors;

using MediatR;

namespace Bags_Shop_API.Services.ProductServices.Command
{
	public class UpdateProductCommand : IRequest<Result<ProductDto>>, IInvalidateCache
    {
        public bool InvalidateAll => true;
        public int Id { get; set; }
        public string? ArName { get; set; }
        public string? EnName { get; set; }
        public string? ArDescription { get; set; }
        public string? EnDescription { get; set; }
      
    }
  

}


