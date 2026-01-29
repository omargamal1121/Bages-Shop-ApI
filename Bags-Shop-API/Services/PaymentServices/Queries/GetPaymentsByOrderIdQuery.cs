using Bags_Shop_API.Services.PaymentServices.Dtos;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.PaymentServices.Queries
{
    public class GetPaymentsByOrderIdQuery : IRequest<Result<List<PaymentDetailsDto>>>
    {
        public int OrderId { get; set; }

        public GetPaymentsByOrderIdQuery(int orderId)
        {
            OrderId = orderId;
        }
    }
}
