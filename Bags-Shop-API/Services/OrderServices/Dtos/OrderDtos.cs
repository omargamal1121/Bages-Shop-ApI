using Bags_Shop_API.Models;

namespace Bags_Shop_API.Services.OrderServices.Dtos
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public decimal FinalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
		public string ?Userkey { get; set; }
		public  string Name { get; set; }
		public List<OrderItemSummaryDto> OrderItems { get; set; } = new();
        public List<PaymentSummaryDto> Payments { get; set; } = new();
    }

    public class OrderItemSummaryDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderProductDto? Product { get; set; }
    }

    public class OrderProductDto
    {
        public int Id { get; set; }
        public string ArName { get; set; }
        public string EnName { get; set; }
        public string ArDescription { get; set; }
        public string EnDescription { get; set; }
        public List<OrderProductImageDto> Images { get; set; } = new();
    }

    public class OrderProductImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string CloudinaryPublicId { get; set; }
    }

    public class PaymentSummaryDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
        public string? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PaymentLink { get; set; }
        public string? PaymentIntentionId { get; set; }
        public DateTime? PaymentLinkExpiresAt { get; set; }
    }
}
