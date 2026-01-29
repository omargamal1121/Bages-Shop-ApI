using Bags_Shop_API.Services.PaymentServices.Dtos;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.PaymentServices.Queries
{
    public class GetWebhookByIdQuery : IRequest<Result<PaymentWebhookResponseDto>>
    {
        public int WebhookId { get; set; }

        public GetWebhookByIdQuery(int webhookId)
        {
            WebhookId = webhookId;
        }
    }
}
