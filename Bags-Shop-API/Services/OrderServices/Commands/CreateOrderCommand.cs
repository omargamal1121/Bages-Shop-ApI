using Bags_Shop_API.Models;
using Bags_Shop_API.Services.Shared;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Bags_Shop_API.Services.OrderServices.Commands
{
    public class CreateOrderCommand : IRequest<Result<int>>
    {
        [Required(ErrorMessage = "Address Is Required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Address Must Be Between 3 and 100 Characters")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Phone Is Required")]
        [RegularExpression(@"^(010|011|012|015)\d{8}$",
            ErrorMessage = "Phone Must Be Valid Egyptian Phone Number")]
        public string Phone { get; set; }

        [Required]
        public List<Orderitemdto> Items { get; set; } = new();

        public string Name { get; set; }
		public string? Userkey { get; set; }
	}

}
