using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.Shared;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bags_Shop_API.Services.PaymentServices.Commands
{
    public class CreatePaymentCommand : IRequest<Result<PaymentLinkResult>> , IInvalidateCache
    {
        [Required]
        public int OrderId { get; set; }
  
        [JsonIgnore]
		public bool InvalidateAll => true;
	}
}
