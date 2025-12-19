using Bags_Shop_API.Services.Shared;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Bags_Shop_API.Services.PaymentServices.Commands
{
    public class CreatePaymentCommand : IRequest<Result<PaymentLinkResult>>
    {
        [Required]
        public int OrderId { get; set; }
        [Required]
        public int PaymentMethodId { get; set; }
        public string? WalletPhoneNumber { get; set; }
    }
}
