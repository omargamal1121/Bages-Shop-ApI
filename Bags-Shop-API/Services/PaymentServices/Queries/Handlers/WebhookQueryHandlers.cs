using Bags_Shop_API.Models;
using Bags_Shop_API.Services.PaymentServices.Dtos;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Specification;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.PaymentServices.Queries.Handlers
{
    public class WebhookQueryHandlers :
        IRequestHandler<GetWebhookByIdQuery, Result<PaymentWebhookResponseDto>>,
        IRequestHandler<GetAllWebhooksQuery, Result<List<PaymentWebhookResponseDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WebhookQueryHandlers> _logger;

        public WebhookQueryHandlers(IUnitOfWork unitOfWork, ILogger<WebhookQueryHandlers> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<PaymentWebhookResponseDto>> Handle(GetWebhookByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var spec = new WebhookWithDetailsSpec(request.WebhookId);
                var webhook = await _unitOfWork.Webhook.GetByIdAsync(spec);

                if (webhook == null)
                {
                    return Result<PaymentWebhookResponseDto>.Fail("Webhook not found", 404);
                }

                var dto = MapToDto(webhook);
                return Result<PaymentWebhookResponseDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving webhook {WebhookId}", request.WebhookId);
                return Result<PaymentWebhookResponseDto>.Fail("Error retrieving webhook", 500);
            }
        }

        public async Task<Result<List<PaymentWebhookResponseDto>>> Handle(GetAllWebhooksQuery request, CancellationToken cancellationToken)
        {
            try
            {
                BaseSpecification<PaymentWebhook> spec;

                if (request.OrderId.HasValue)
                {
                    spec = new BaseSpecification<PaymentWebhook>
                    {
                        Criteria = w => w.OrderId == request.OrderId.Value
                    };
                    spec.ApplyOrderByDescending(w => w.Id);
                }
                else
                {
                    spec = new AllWebhooksSpec();
                }

                var webhooks = await _unitOfWork.Webhook.GetAllAsync(spec);

                // Apply pagination
                var paginatedWebhooks = webhooks
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var dtos = paginatedWebhooks.Select(MapToDto).ToList();
                return Result<List<PaymentWebhookResponseDto>>.Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving webhooks");
                return Result<List<PaymentWebhookResponseDto>>.Fail("Error retrieving webhooks", 500);
            }
        }

        private PaymentWebhookResponseDto MapToDto(PaymentWebhook webhook)
        {
            return new PaymentWebhookResponseDto
            {
                Id = webhook.Id,
                TransactionId = webhook.TransactionId,
                OrderId = webhook.OrderId,
                PaymentId = webhook.PaymentId,
                PaymentMethod = webhook.PaymentMethod,
                Success = webhook.Success,
                Status = webhook.Status,
                AmountCents = webhook.AmountCents,
                Currency = webhook.Currency,
                SourceSubType = webhook.SourceSubType,
                SourceIssuer = webhook.SourceIssuer,
                CardLast4 = webhook.CardLast4,
                PaymentProvider = webhook.PaymentProvider,
            
                HmacVerified = webhook.HmacVerified,
           
                ProcessedAt = webhook.ProcessedAt,
             
                PaymobOrderId = webhook.PaymobOrderId
            };
        }
    }
}
