using Bags_Shop_API.Models;
using Bags_Shop_API.Services.PaymentServices.Dtos;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.PaymentServices.Queries.Handlers
{
    public class PaymentQueryHandlers :
        IRequestHandler<GetPaymentByIdQuery, Result<PaymentDetailsDto>>,
        IRequestHandler<GetPaymentsByOrderIdQuery, Result<List<PaymentDetailsDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentQueryHandlers> _logger;

        public PaymentQueryHandlers(IUnitOfWork unitOfWork, ILogger<PaymentQueryHandlers> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<PaymentDetailsDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var payment = await _unitOfWork.Payments.GetByIdAsync(request.PaymentId);
                
                if (payment == null)
                {
                    return Result<PaymentDetailsDto>.Fail("Payment not found", 404);
                }

                var dto = MapToDto(payment);
                return Result<PaymentDetailsDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment {PaymentId}", request.PaymentId);
                return Result<PaymentDetailsDto>.Fail("Error retrieving payment", 500);
            }
        }

        public async Task<Result<List<PaymentDetailsDto>>> Handle(GetPaymentsByOrderIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var spec = new PaymentSpecifications.PaymentByOrderSpec(request.OrderId);
                var payments = await _unitOfWork.Payments.GetAllAsync(spec);

                var dtos = payments.Select(MapToDto).OrderByDescending(p => p.CreatedAt).ToList();
                return Result<List<PaymentDetailsDto>>.Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for order {OrderId}", request.OrderId);
                return Result<List<PaymentDetailsDto>>.Fail("Error retrieving payments", 500);
            }
        }

        private PaymentDetailsDto MapToDto(Payment payment)
        {
            return new PaymentDetailsDto
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status,
                Method = payment.Method,
                TransactionId = payment.TransactionId,
                ProviderOrderId = payment.ProviderOrderId,
                PaymentLink = payment.PaymentLink,
                PaymentIntentionId = payment.PaymentIntentionId,
                PaymentLinkExpiresAt = payment.PaymentLinkExpiresAt,
                CreatedAt = payment.CreatedAt,
                ModifiedAt = payment.ModifiedAt
            };
        }
    }
}
