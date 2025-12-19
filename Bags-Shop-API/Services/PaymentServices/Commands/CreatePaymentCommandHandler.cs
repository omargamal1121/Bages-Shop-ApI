using Bags_Shop_API.Models;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bags_Shop_API.Services.PaymentServices.Commands
{
    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<PaymentLinkResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly ILogger<CreatePaymentCommandHandler> _logger;

        public CreatePaymentCommandHandler(
            IUnitOfWork unitOfWork,
            IPaymentProcessor paymentProcessor,
            ILogger<CreatePaymentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _paymentProcessor = paymentProcessor;
            _logger = logger;
        }

        public async Task<Result<PaymentLinkResult>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validate Order
                var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
                if (order == null)
                {
                    return Result<PaymentLinkResult>.Fail("Order not found.", 404);
                }

                if (order.Status != OrderStatus.Pending)
                {
                    return Result<PaymentLinkResult>.Fail($"Order is not eligible for payment. Status: {order.Status}", 400);
                }

                if (order.ExpiresAt < DateTime.UtcNow)
                {
                    return Result<PaymentLinkResult>.Fail("Order has expired.", 400);
                }

                // 2. Check for existing payments (Pending or Completed)
                var activePaymentSpec = new PaymentByOrderAndStatusSpec(order.Id, new List<PaymentStatus> { PaymentStatus.Pending, PaymentStatus.Completed });
                var hasActivePayment = await _unitOfWork.Payments.AnyAsync(activePaymentSpec);
                if (hasActivePayment)
                {
                    return Result<PaymentLinkResult>.Fail("This order already has a pending or completed payment.", 400);
                }

                // 3. Create Payment Entity
                var paymentEntity = new Payment
                {
                    OrderId = order.Id,
                    Amount = order.FinalPrice,
                    Method = (PaymentMethod)request.PaymentMethodId,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Currency = "EGP"
                };

                await _unitOfWork.Payments.AddAsync(paymentEntity);
                await _unitOfWork.SaveChangesAsync();

                // 4. Map to CreatePaymentDto
                var createPaymentDto = new CreatePaymentDto
                {
                    Amount = order.FinalPrice,
                    Currency = "EGP",
                    Ordernumber = order.Id,
                    PaymentMethod = (PaymentMethod)request.PaymentMethodId,
                    WalletPhoneNumber = request.WalletPhoneNumber,
                    BillingAddress = order.Address,
                    BillingPhone = order.Phone
                };

                // 5. Calculate Expiration
                int timeRemainingSeconds = (int)(order.ExpiresAt - DateTime.UtcNow).TotalSeconds;
                if (timeRemainingSeconds <= 0)
                {
                    return Result<PaymentLinkResult>.Fail("Order has expired.", 400);
                }

                // 6. Call Payment Processor
                var result = await _paymentProcessor.GetPaymentLinkAsync(createPaymentDto, timeRemainingSeconds);

                if (result.Success && result.Data != null)
                {
                    // Update Payment with Paymob OrderId
                    paymentEntity.ProviderOrderId = result.Data.PaymobOrderId;
                    await _unitOfWork.SaveChangesAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for Order {OrderId}", request.OrderId);
                return Result<PaymentLinkResult>.Fail("Internal server error while creating payment.", 500);
            }
        }
    }
}
