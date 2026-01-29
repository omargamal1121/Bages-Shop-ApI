using Bags_Shop_API.Models;
using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.OrderServices;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Specification;
using Bags_Shop_API.UnitOfWorkService;
using Hangfire;
using MediatR;

namespace Bags_Shop_API.Services.PaymentServices.Commands
{
    public class CreatePaymentCommandHandler
        : IRequestHandler<CreatePaymentCommand, Result<PaymentLinkResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<CreatePaymentCommandHandler> _logger;
        private readonly IOrderServices _orderservices;
        private readonly ICacheTokenProvider _tokenProvider;

        public CreatePaymentCommandHandler(
            ICacheTokenProvider tokenProvider,
            IOrderServices orderservices,
            IBackgroundJobClient backgroundJobClient,
            IUnitOfWork unitOfWork,
            IPaymentProcessor paymentProcessor,
            ILogger<CreatePaymentCommandHandler> logger)
        {
            _tokenProvider = tokenProvider;
            _orderservices = orderservices;
            _backgroundJobClient = backgroundJobClient;
            _unitOfWork = unitOfWork;
            _paymentProcessor = paymentProcessor;
            _logger = logger;
        }

        public async Task CheckAndUpdatePaymentStatusAsync(int paymentId)
        {
            _logger.LogInformation(
                "Starting CheckAndUpdatePaymentStatusAsync for payment {PaymentId}",
                paymentId);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
                if (payment == null || payment.Status != PaymentStatus.Pending)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return;
                }

                var statusResponse =
                    await _paymentProcessor.GetPaymentStatusAsync(payment.ProviderOrderId);

                if (!statusResponse.Success || statusResponse.Data == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return;
                }

                var remoteStatus = statusResponse.Data.Status?.ToLower();

                PaymentStatus newStatus = remoteStatus switch
                {
                    "paid" => PaymentStatus.Completed,
                    "expired" => PaymentStatus.Failed,
                    "canceled" => PaymentStatus.Failed,
                    "failed" => PaymentStatus.Failed,
                    "unpaid" => PaymentStatus.Failed,
                    _ => PaymentStatus.Pending
                };

                if (newStatus == payment.Status)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return;
                }

                payment.Status = newStatus;
                payment.ModifiedAt = DateTime.UtcNow;

                var orderStatus =
                    newStatus == PaymentStatus.Completed
                        ? OrderStatus.Paid
                        : OrderStatus.Expired;

       
                _backgroundJobClient.Enqueue<ICacheTokenProvider>(x =>
                    x.Reset());

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error in CheckAndUpdatePaymentStatusAsync for payment {PaymentId}",
                    paymentId);

                await _unitOfWork.RollbackTransactionAsync();

                _backgroundJobClient.Schedule<CreatePaymentCommandHandler>(
                    x => x.CheckAndUpdatePaymentStatusAsync(paymentId),
                    TimeSpan.FromMinutes(30));
            }
        }
        public async Task<Result<PaymentLinkResult>> Handle(
            CreatePaymentCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
                if (order == null)
                    return Result<PaymentLinkResult>.Fail("Order not found.", 404);

                if (order.Status != OrderStatus.Pending)
                    return Result<PaymentLinkResult>.Fail(
                        $"Order is not eligible for payment. Status: {order.Status}", 400);

                var remainingTime = order.ExpiresAt - DateTime.UtcNow;

                if (remainingTime.TotalSeconds <= 0)
                    return Result<PaymentLinkResult>.Fail("Order has expired.", 400);

                if (remainingTime < TimeSpan.FromMinutes(5))
                {
                    return Result<PaymentLinkResult>.Fail(
                        "Order is about to expire. Please create a new order.",
                        400);
                }

                var lastPayment = (await _unitOfWork.Payments.GetAllAsync(
                        new BaseSpecification<Payment>
                        {
                            Criteria = p => p.OrderId == request.OrderId
                        }))
                    .OrderBy(p => p.Id)
                    .LastOrDefault();

                if (lastPayment != null)
                {
                    if (lastPayment.Status == PaymentStatus.Completed)
                    {
                         return Result<PaymentLinkResult>.Fail(
                        "This order already has a completed payment.",
                        400);
                    }

                    if (lastPayment.Status == PaymentStatus.Pending && 
                        !string.IsNullOrEmpty(lastPayment.PaymentLink) && 
                        lastPayment.PaymentLinkExpiresAt.HasValue &&
                        lastPayment.PaymentLinkExpiresAt > DateTime.UtcNow)
                    {
               
                         _logger.LogInformation("Returning existing valid payment link for order {OrderId}", request.OrderId);
                         
                         var resultLink = new PaymentLinkResult
                         {
                             PaymentUrl = lastPayment.PaymentLink,
                             UnifiedCheckoutUrl = lastPayment.PaymentLink, 
                             IntentionId = lastPayment.PaymentIntentionId ?? "",
                             PaymobOrderId = lastPayment.ProviderOrderId,
                             ClientSecret = "", 
                             PublicKey = "" 
                         };
                   
                         
                         await _unitOfWork.RollbackTransactionAsync();
                         return Result<PaymentLinkResult>.Ok(resultLink);
                    }
                }
               

                int expiresInSeconds = 86400;

                var paymentEntity = new Payment
                {
                    OrderId = order.Id,
                    Amount = order.FinalPrice,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Currency = "EGP"
                };

                var createPaymentDto = new CreatePaymentDto
                {
                    Amount = order.FinalPrice,
                    Currency = "EGP",
                    Ordernumber = order.Id,
               
                    BillingAddress = order.Address,
                    BillingPhone = order.Phone
                };

				await _unitOfWork.Payments.AddAsync(paymentEntity);
				await _unitOfWork.SaveChangesAsync();

				var result = await _paymentProcessor.GetPaymentLinkAsync(
                    createPaymentDto,
                    expiresInSeconds,
                    lastPayment?.ProviderOrderId,
                    paymentEntity.Id);

                if (!result.Success || result.Data == null)
                {
                    return Result<PaymentLinkResult>.Fail(
                        "Failed to create payment link.",
                        500);
                }

                paymentEntity.ProviderOrderId = result.Data.PaymobOrderId;
                paymentEntity.PaymentLink = result.Data.PaymentUrl;
                paymentEntity.PaymentIntentionId = result.Data.IntentionId;
                paymentEntity.PaymentLinkExpiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);
       
                await _unitOfWork.CommitTransactionAsync();
                _backgroundJobClient.Schedule<CreatePaymentCommandHandler>(
                    x => x.CheckAndUpdatePaymentStatusAsync(paymentEntity.Id), 
                    TimeSpan.FromMinutes(10));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating payment for Order {OrderId}",
                    request.OrderId);

                return Result<PaymentLinkResult>.Fail(
                    "Internal server error while creating payment.",
                    500);
            }
        }
    }
}
