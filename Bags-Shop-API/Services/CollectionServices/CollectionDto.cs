using Bags_Shop_API.Services.ImageServices;
using Bags_Shop_API.Services.ProductServices.ProductsDtos;

namespace Bags_Shop_API.Services.CollectionServices
{
    public class CollectionDto
    {
        public int Id { get; set; }
        public string ArName { get; set; }
        public string EnName { get; set; }
        public string ArDescription { get; set; }
        public string EnDescription { get; set; }
        public bool IsActive { get; set; }
		public List<ProductDto> products { get; set; }
		public List<ImageDto> Images { get; set; }
	}
}
