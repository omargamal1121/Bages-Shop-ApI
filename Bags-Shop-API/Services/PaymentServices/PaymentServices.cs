using Bags_Shop_API.Models;
using Bags_Shop_API.Services.OrderServices;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Bags_Shop_API.Services.PaymentServices.Dtos;


namespace Bags_Shop_API.Services.PaymentServices
{
    public interface IPaymentServices
    {
        Task<Result<PaymentResponseDto>> CreatePaymentMethod(int orderid, CreatePaymentOfCustomer paymentdto, string userid);
        Task<Result<int>> UpdatePaymentAfterPaid(int orderid, string transactionId, long providerOrderId, PaymentStatus status);
    }

    public class PaymentServices : IPaymentServices
    {
        private readonly IOrderServices _orderservices;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IErrorNotificationService _errorNotificationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentServices> _logger;

        public PaymentServices(
            IOrderServices orderservices,
            IPaymentProcessor paymentProcessor,
            IUnitOfWork unitOfWork,
            ILogger<PaymentServices> logger,
            IBackgroundJobClient backgroundJobClient,
            IErrorNotificationService errorNotificationService)
        {
            _orderservices = orderservices;
            _paymentProcessor = paymentProcessor;
            _unitOfWork = unitOfWork;
            _backgroundJobClient = backgroundJobClient;
            _errorNotificationService = errorNotificationService;
            _logger = logger;
        }

        public async Task<Result<int>> UpdatePaymentAfterPaid(int orderId, string transactionId, long providerOrderId, PaymentStatus status)
        {
            _logger.LogInformation("Starting UpdatePaymentAfterPaid for order {OrderId} with status {Status}", orderId, status);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (string.IsNullOrEmpty(transactionId))
                {
                    _logger.LogWarning("Transaction ID is null or empty for order {OrderId}", orderId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<int>.Fail("Transaction ID is required", 400,null);
                }

                var spec = new PaymentByOrderAndProviderSpec(orderId, providerOrderId);
                var latestPayment = await _unitOfWork.Payments.GetByIdAsync(spec);

                if (latestPayment == null)
                {
                    _logger.LogWarning("Payment not found for order {OrderId} with provider order ID {ProviderOrderId}", orderId, providerOrderId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<int>.Fail("Payment not found", 404,null);
                }

                if (latestPayment.Status == status)
                {
                    _logger.LogInformation("Payment {PaymentId} already has status {Status}, no update needed", latestPayment.Id, status);
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<int>.Ok(latestPayment.Id);
                }

                latestPayment.Status = status;
                latestPayment.TransactionId = transactionId;
                latestPayment.ModifiedAt = DateTime.UtcNow;

                _logger.LogInformation("Updating payment {PaymentId} with status {Status} and transaction ID {TransactionId}",
                    latestPayment.Id, status, transactionId);

                _unitOfWork.Payments.Update(latestPayment);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Payment {PaymentId} updated successfully", latestPayment.Id);

                return Result<int>.Ok(latestPayment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment for order {OrderId}", orderId);
                await _unitOfWork.RollbackTransactionAsync();

                _backgroundJobClient.Enqueue(() =>
                    _errorNotificationService.SendErrorNotificationAsync("Error in UpdatePaymentAfterPaid", ex.Message));

                return Result<int>.Fail("Error while updating payment", 500, null);
            }
        }

        public async Task<Result<PaymentResponseDto>> CreatePaymentMethod(int ordernumber, CreatePaymentOfCustomer paymentdto, string userid)
        {
            _logger.LogInformation("Starting CreatePaymentMethod for order {OrderNumber} by user {UserId}", ordernumber, userid);

            if (paymentdto == null)
            {
                _logger.LogWarning("CreatePaymentMethod called with null DTO by user {UserId}", userid);
                return Result<PaymentResponseDto>.Fail("Payment data is required.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(ordernumber);
                if (order == null)
                {
                    _logger.LogWarning("Order not found with number {OrderNumber}", ordernumber);
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<PaymentResponseDto>.Fail("Order not found.");
                }

                var pendingSpec = new PendingPaymentSpec(order.Id);
                var ishaspendingpayment = await _unitOfWork.Payments.AnyAsync(pendingSpec);

                if (ishaspendingpayment)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<PaymentResponseDto>.Fail("There is already a pending payment for this order. Please wait until it is completed ");
                }

                if (order.Status != OrderStatus.Pending)
                {
                    _logger.LogWarning("Order {OrderId} is not eligible for payment. Current status: {Status}", order.Id, order.Status);
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<PaymentResponseDto>.Fail("This order cannot be paid due to its current status.", 400);
                }

                var payment = new Payment
                {
                    Amount = order.FinalPrice,
                    Status = PaymentStatus.Pending,
                    Method = paymentdto.PaymentMethod,
                    OrderId = order.Id,
                    CreatedAt = DateTime.UtcNow,
                    Currency = paymentdto.Currency
                };

                var response = new PaymentResponseDto
                {
                    IsRedirectRequired = false,
                    RedirectUrl = null,
                    Message = "Cash on Delivery selected. No redirect required."
                };

                if (paymentdto.PaymentMethod != PaymentMethod.CashOnDelivery)
                {
                    var onlinePaymentResult = await ProcessOnlinePayment(paymentdto, order, payment);
                    if (!onlinePaymentResult.Success)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result<PaymentResponseDto>.Fail(onlinePaymentResult.Message);
                    }
                    response = onlinePaymentResult.Data;
                }
                else
                {
                    _backgroundJobClient.Enqueue(() => _orderservices.ConfirmOrderAsync(order.Id, userid, false, true, null));
                }

                var createdPayment = await _unitOfWork.Payments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Payment created successfully with ID {Id}", createdPayment.Id);

                // Schedule status check for online payments
                if (paymentdto.PaymentMethod != PaymentMethod.CashOnDelivery)
                {
                    _backgroundJobClient.Schedule(() =>
                        CheckAndUpdatePaymentStatusAsync(createdPayment.Id),
                        TimeSpan.FromHours(1));
                }

                response!.Paymentid = createdPayment.Id;
                return Result<PaymentResponseDto>.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating payment for user {UserId}", userid);
                await _unitOfWork.RollbackTransactionAsync();

                _backgroundJobClient.Enqueue(() =>
                    _errorNotificationService.SendErrorNotificationAsync("Error in CreatePaymentMethod", ex.Message));

                return Result<PaymentResponseDto>.Fail("Internal server error.", 500);
            }
        }

        private async Task<Result<PaymentResponseDto>> ProcessOnlinePayment(CreatePaymentOfCustomer paymentdto, Order order, Payment payment)
        {
            int timeremaining = (int)(order.ExpiresAt - DateTime.UtcNow).TotalSeconds;
            if (timeremaining <= 0)
            {
                _logger.LogWarning("Order {OrderId} has expired and cannot be paid", order.Id);
                return Result<PaymentResponseDto>.Fail("This order has expired and cannot be paid.", 400);
            }

            CreatePaymentDto createPayment = new CreatePaymentDto
            {
                Amount = payment.Amount,
                Currency = paymentdto.Currency,
                Notes = paymentdto.Notes,
                Ordernumber = order.Id,
                PaymentMethod = paymentdto.PaymentMethod,
                WalletPhoneNumber = paymentdto.WalletPhoneNumber,
            };

            var onlinePaymentResult = await _paymentProcessor.GetPaymentLinkAsync(createPayment, timeremaining);

            if (!onlinePaymentResult.Success || onlinePaymentResult.Data == null)
            {
                _logger.LogError("Failed to get payment link for order {OrderId}: {Error}", order.Id, onlinePaymentResult.Message);
                return Result<PaymentResponseDto>.Fail(onlinePaymentResult.Message);
            }

            payment.ProviderOrderId = onlinePaymentResult.Data!.PaymobOrderId;

            return Result<PaymentResponseDto>.Ok(new PaymentResponseDto
            {
                IsRedirectRequired = true,
                RedirectUrl = onlinePaymentResult.Data.PaymentUrl,
                Message = "Redirect to the provided link to complete payment."
            });
        }

        public async Task CheckAndUpdatePaymentStatusAsync(int paymentId)
        {
            _logger.LogInformation("Starting CheckAndUpdatePaymentStatusAsync for payment {PaymentId}", paymentId);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return;
                }

                if (payment.Status != PaymentStatus.Pending)
                {
                    _logger.LogInformation("Payment {PaymentId} is not pending (current status: {Status})", paymentId, payment.Status);
                    await _unitOfWork.RollbackTransactionAsync();
                    return;
                }

                var statusResponse = await _paymentProcessor.GetPaymentStatusAsync(payment.ProviderOrderId);

                if (!statusResponse.Success || statusResponse.Data == null)
                {
                    _logger.LogWarning("Failed to fetch status for payment {PaymentId}: {Error}", paymentId, statusResponse.Message);
                    await _unitOfWork.RollbackTransactionAsync();
                    return;
                }

                var remoteStatus = statusResponse.Data.Status;

                PaymentStatus newStatus = remoteStatus switch
                {
                    "Paid" => PaymentStatus.Completed,
                    "Unpaid" => PaymentStatus.Failed,
                    _ => PaymentStatus.Pending
                };

                if (newStatus == payment.Status)
                {
                    _logger.LogInformation("Payment {PaymentId} status unchanged ({Status})", paymentId, newStatus);
                    await _unitOfWork.RollbackTransactionAsync();
                    return;
                }

                payment.Status = newStatus;
                payment.ModifiedAt = DateTime.UtcNow;

                var orderStatus = newStatus == PaymentStatus.Completed
                    ? OrderStatus.Paid
                    : OrderStatus.Expired;

                _backgroundJobClient.Enqueue(() => _orderservices.UpdateOrderAfterPaid(payment.OrderId, orderStatus));

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Payment {PaymentId} updated to {Status}", paymentId, newStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAndUpdatePaymentStatusAsync for payment {PaymentId}", paymentId);
                await _unitOfWork.RollbackTransactionAsync();

                // Schedule retry for failed background job
                _backgroundJobClient.Schedule(() =>
                    CheckAndUpdatePaymentStatusAsync(paymentId),
                    TimeSpan.FromMinutes(30));
            }
        }
    }
}


