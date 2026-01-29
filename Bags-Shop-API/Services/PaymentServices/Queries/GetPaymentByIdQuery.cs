using Bags_Shop_API.Services.PaymentServices.Dtos;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.PaymentServices.Queries
{
    public class GetPaymentByIdQuery : IRequest<Result<PaymentDetailsDto>>
    {
        public int PaymentId { get; set; }

        public GetPaymentByIdQuery(int paymentId)
        {
            PaymentId = paymentId;
        }
    }
}
