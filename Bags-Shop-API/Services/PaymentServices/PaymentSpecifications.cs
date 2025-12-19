using Bags_Shop_API.Models;
using Bags_Shop_API.Specification;

namespace Bags_Shop_API.Services.PaymentServices
{
    public class PaymentByOrderAndProviderSpec : BaseSpecification<Payment>
    {
        public PaymentByOrderAndProviderSpec(int orderId, long providerOrderId)
            : base(p => p.OrderId == orderId && p.ProviderOrderId == providerOrderId)
        {
            ApplyOrderByDescending(p => p.Id);
        }
    }

    public class PendingPaymentSpec : BaseSpecification<Payment>
    {
        public PendingPaymentSpec(int orderId)
            : base(p => p.OrderId == orderId && p.Status == PaymentStatus.Pending)
        {
        }
    }

    public class PaymentByOrderAndStatusSpec : BaseSpecification<Payment>
    {
        public PaymentByOrderAndStatusSpec(int orderId, List<PaymentStatus> statuses)
            : base(p => p.OrderId == orderId && statuses.Contains(p.Status))
        {
        }
    }

    public class WebhookByTransactionSpec : BaseSpecification<PaymentWebhook>
    {
        public WebhookByTransactionSpec(long transactionId)
            : base(w => w.TransactionId == transactionId)
        {
        }
    }
}
