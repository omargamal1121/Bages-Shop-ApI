using System.ComponentModel.DataAnnotations;

namespace Bags_Shop_API
{
	public class Image
	{
		[Key]
		public int Id { get; set; }
		public string ImageUrl { get; set; }
		public int? ProductId { get; set; }
		public Product? Product { get; set; }

		public int? CollectionId { get; set; }
		public Collection?  Collection { get; set; }

		public string CloudinaryPublicId { get; set; }

	}
}
