using Bags_Shop_API.Models;
using System.ComponentModel.DataAnnotations;

namespace Bags_Shop_API.Services.PaymentServices.Dtos
{
    public class CreatePaymentOfCustomer
    {
        public string? WalletPhoneNumber { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        [StringLength(3, ErrorMessage = "Currency code should be 3 letters.")]
        public string Currency { get; set; } = "EGP";

        [StringLength(250)]
        public string? Notes { get; set; }
    }
}
