using Bags_Shop_API.Models;
using Bags_Shop_API.Services.Shared;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Bags_Shop_API.Services.OrderServices.Commands
{
    public class CreateOrderCommand : IRequest<Result<int>>
    {
        [Required]
        public string Address { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public List<Orderitemdto> Items { get; set; } = new List<Orderitemdto>();
  
    }
}
