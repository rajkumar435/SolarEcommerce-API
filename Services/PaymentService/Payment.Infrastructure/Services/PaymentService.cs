using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Infrastructure.Repositories;

namespace Payment.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository repository,
            ILogger<PaymentService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<PaymentResponse> CreatePayment(
            CreatePaymentRequest request)
        {
            _logger.LogInformation(
                "Starting payment for OrderId {OrderId}, Amount {Amount}",
                request.OrderId,
                request.Amount);

            // -------------------------------------------------
            // CHECK EXISTING PAYMENT
            // -------------------------------------------------

            var existingPayment =
                await _repository.GetByOrderId(
                    request.OrderId);

            if (existingPayment != null)
            {
                _logger.LogWarning(
                    "Payment already exists for OrderId {OrderId}",
                    request.OrderId);

                return MapToResponse(existingPayment);
            }

            // -------------------------------------------------
            // CREATE PAYMENT
            // -------------------------------------------------
            // -------------------------------------------------
            // VALIDATE PAYMENT DETAILS
            // -------------------------------------------------

            if (string.IsNullOrWhiteSpace(request.PaymentMethod))
            {
                throw new ArgumentException(
                    "Payment method is required.");
            }


            if (request.PaymentMethod == "UPI")
            {
                if (string.IsNullOrWhiteSpace(request.UpiId))
                {
                    throw new ArgumentException(
                        "UPI ID is required.");
                }
            }


            if (request.PaymentMethod == "CARD")
            {
                if (string.IsNullOrWhiteSpace(request.CardHolderName))
                {
                    throw new ArgumentException(
                        "Card holder name is required.");
                }

                if (string.IsNullOrWhiteSpace(request.CardNumber))
                {
                    throw new ArgumentException(
                        "Card number is required.");
                }

                if (string.IsNullOrWhiteSpace(request.ExpiryDate))
                {
                    throw new ArgumentException(
                        "Card expiry date is required.");
                }

                if (string.IsNullOrWhiteSpace(request.Cvv))
                {
                    throw new ArgumentException(
                        "CVV is required.");
                }
            }
            var payment = new Payments
            {
                OrderId = request.OrderId,
                UserId = request.UserId,
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethod = request.PaymentMethod,



                Status = PaymentStatus.Processing,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.Add(payment);

            await _repository.SaveChanges();

            _logger.LogInformation(
                "Payment {PaymentId} created with Processing status",
                payment.Id);

            // -------------------------------------------------
            // MOCK PAYMENT PROCESSING
            // -------------------------------------------------

            bool paymentSuccessful = true;

            if (paymentSuccessful)
            {
                payment.Status =
                    PaymentStatus.Successful;

                payment.TransactionId =
                    "TXN-" +
                    Guid.NewGuid()
                        .ToString("N")
                        .Substring(0, 12)
                        .ToUpper();

                payment.ProviderReference =
                    "MOCK-" +
                    Guid.NewGuid()
                        .ToString("N")
                        .Substring(0, 10)
                        .ToUpper();

                payment.ProcessedAt =
                    DateTime.UtcNow;

                payment.UpdatedAt =
                    DateTime.UtcNow;

                await _repository.SaveChanges();

                _logger.LogInformation(
                    "Payment {PaymentId} successfully completed. TransactionId {TransactionId}",
                    payment.Id,
                    payment.TransactionId);
            }
            else
            {
                payment.Status =
                    PaymentStatus.Failed;

                payment.FailureReason =
                    "Mock payment failed";

                payment.UpdatedAt =
                    DateTime.UtcNow;

                await _repository.SaveChanges();

                _logger.LogWarning(
                    "Payment {PaymentId} failed",
                    payment.Id);
            }

            return MapToResponse(payment);
        }

        public async Task<PaymentResponse?> GetPaymentById(
            int id)
        {
            var payment =
                await _repository.GetById(id);

            if (payment == null)
            {
                return null;
            }

            return MapToResponse(payment);
        }

        public async Task<PaymentResponse?> GetPaymentByOrderId(
            int orderId)
        {
            var payment =
                await _repository.GetByOrderId(orderId);

            if (payment == null)
            {
                return null;
            }

            return MapToResponse(payment);
        }

        private PaymentResponse MapToResponse(
            Payments payment)
        {
            return new PaymentResponse
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                UserId = payment.UserId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status,
                PaymentMethod = payment.PaymentMethod,
                TransactionId = payment.TransactionId,
                FailureReason = payment.FailureReason,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt
            };
        }


        public async Task<PaymentResponse>
       RecordSuccessfulRazorpayPayment(
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

            //if (request.UserId <= 0)
            //{
            //    throw new ArgumentException(
            //        "Valid UserId is required.");
            //}

            //if (request.Amount <= 0)
            //{
            //    throw new ArgumentException(
            //        "Payment amount must be greater than zero.");
            //}

            var payment =
                await _repository
                    .RecordSuccessfulRazorpayPayment(
                        request);

            return new PaymentResponse
            {
                Id =
                    payment.Id,

                OrderId =
                    payment.OrderId,

                UserId =
                    payment.UserId,

                Amount =
                    payment.Amount,

                Currency =
                    payment.Currency,

                Status =
                    payment.Status,

                PaymentMethod =
                    payment.PaymentMethod,

                TransactionId =
                    payment.TransactionId,

                FailureReason =
                    payment.FailureReason,

                CreatedAt =
                    payment.CreatedAt,

                ProcessedAt =
                    payment.ProcessedAt
            };
        }
    }
}