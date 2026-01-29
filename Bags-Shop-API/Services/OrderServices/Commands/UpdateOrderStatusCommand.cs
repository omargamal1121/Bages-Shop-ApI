using Bags_Shop_API.Models;
using Bags_Shop_API.Services.Shared;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bags_Shop_API.Services.OrderServices.Commands
{
    public class UpdateOrderStatusCommand : IRequest<Result<bool>>
    {
        [Required(ErrorMessage = "Order ID is required")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public OrderStatus Status { get; set; }
    
	}
}
