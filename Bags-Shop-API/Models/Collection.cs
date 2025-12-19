using Bags_Shop_API.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bags_Shop_API
{
	public class Collection
	{
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Need Arabic Name For Collection")]
        [Range(3, 100, ErrorMessage = "Collection Name Must Be Between 3 and 100 Characters")]

        public string ArName { get; set; }
        [Required(ErrorMessage = "Need English Name For Collection")]
        [Range(3, 100, ErrorMessage = "Collection Name Must Be Between 3 and 100 Characters")]
        public string EnName { get; set; }
        [Required(ErrorMessage = "Need Arabic Description For Collection")]
        [Range(10, 500, ErrorMessage = "Collection Description Must Be Between 10 and 500 Characters")]

        public string ArDescription { get; set; }
        [Required(ErrorMessage = "Need English Description For Collection")]
        [Range(10, 500, ErrorMessage = "Collection Description Must Be Between 10 and 500 Characters")]
        public string EnDescription { get; set; }
        public bool IsActive { get; set; } = false;


		public ICollection<Product> Products { get; set; }
        public ICollection<Image> Images { get; set; }




    }
}
