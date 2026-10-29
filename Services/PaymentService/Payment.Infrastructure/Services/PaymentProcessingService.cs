using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;

namespace Payment.Infrastructure.Services
{
    public class PaymentProcessingService
        : IPaymentProcessingService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderClient _orderClient;
        private readonly ILogger<PaymentProcessingService> _logger;

        public PaymentProcessingService(
            IPaymentRepository paymentRepository,
            IOrderClient orderClient,
            ILogger<PaymentProcessingService> logger)
        {
            _paymentRepository =
                paymentRepository;

            _orderClient =
                orderClient;

            _logger =
                logger;
        }

        public async Task<bool>
            ProcessSuccessfulRazorpayPayment(
                VerifyPaymentRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(
                    nameof(request));
            }

            if (request.OrderId <= 0)
            {
                throw new ArgumentException(
                    "Valid OrderId is required.");
            }

            if (request.UserId <= 0)
            {
                throw new ArgumentException(
                    "Valid UserId is required.");
            }

            if (request.Amount <= 0)
            {
                throw new ArgumentException(
                    "Amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(
                request.RazorpayOrderId))
            {
                throw new ArgumentException(
                    "RazorpayOrderId is required.");
            }

            if (string.IsNullOrWhiteSpace(
                request.RazorpayPaymentId))
            {
                throw new ArgumentException(
                    "RazorpayPaymentId is required.");
            }

            _logger.LogInformation(
                "Recording successful Razorpay payment. " +
                "OrderId: {OrderId}",
                request.OrderId);

            // -------------------------------------------------
            // STEP 1
            // Record payment
            // -------------------------------------------------

            var payment =
                await _paymentRepository
                    .RecordSuccessfulRazorpayPayment(
                        request);

            if (payment == null)
            {
                throw new Exception(
                    "Unable to record payment.");
            }

            _logger.LogInformation(
                "Payment recorded successfully. " +
                "PaymentId: {PaymentId}, OrderId: {OrderId}",
                payment.Id,
                payment.OrderId);

            // -------------------------------------------------
            // STEP 2
            // Update Order API
            // -------------------------------------------------

            var orderUpdated =
                await _orderClient
                    .MarkPaymentSuccessful(
                        request.OrderId);

            if (!orderUpdated)
            {
                _logger.LogError(
                    "Payment recorded but Order API " +
                    "could not be updated. " +
                    "OrderId: {OrderId}",
                    request.OrderId);

                /*
                 * IMPORTANT:
                 *
                 * Do NOT delete the payment.
                 *
                 * Payment succeeded at Razorpay and is already
                 * stored in our database.
                 *
                 * This is a distributed-system consistency issue.
                 *
                 * Later we can solve this with RabbitMQ/Outbox.
                 */

                return false;
            }

            _logger.LogInformation(
                "Order marked Paid successfully. " +
                "OrderId: {OrderId}",
                request.OrderId);

            return true;
        }
    }
}