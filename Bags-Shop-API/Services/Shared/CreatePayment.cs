using Bags_Shop_API.Models;

namespace ApplicationLayer.DtoModels.PaymentDtos
{
    public class CreatePayment
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public int Ordernumber { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? WalletPhoneNumber { get; set; }
        public string? Notes { get; set; }
       
        public string BillingAddress { get; set; }
        public string BillingPhone { get; set; }

    }
}
