using Bags_Shop_API.Services.PaymentServices.Dtos;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.PaymentServices.Queries
{
    public class GetAllWebhooksQuery : IRequest<Result<List<PaymentWebhookResponseDto>>>
    {
        public int? OrderId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
