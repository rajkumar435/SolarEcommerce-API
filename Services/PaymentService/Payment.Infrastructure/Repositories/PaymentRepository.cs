using Microsoft.EntityFrameworkCore;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Infrastructure.Data;

namespace Payment.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentRepository(
            PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<Payments?> GetById(int id)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Payments?> GetByOrderId(
            int orderId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(
                    x => x.OrderId == orderId);
        }

        public async Task Add(Payments payment)
        {
            await _context.Payments.AddAsync(payment);
        }

        public async Task<Payments> RecordSuccessfulRazorpayPayment(
            VerifyPaymentRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(
                    nameof(request));
            }

            var existingPayment =
                await _context.Payments
                    .FirstOrDefaultAsync(
                        x => x.OrderId == request.OrderId);

            // -----------------------------------------------------
            // If payment already exists, update it.
            // This also helps prevent duplicate payment records.
            // -----------------------------------------------------

            if (existingPayment != null)
            {
                existingPayment.Status =
                    PaymentStatus.Successful;

                existingPayment.TransactionId =
                    request.RazorpayPaymentId;

                existingPayment.ProviderReference =
                    request.RazorpayOrderId;

                existingPayment.ProcessedAt =
                    DateTime.UtcNow;

                existingPayment.UpdatedAt =
                    DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return existingPayment;
            }

            // -----------------------------------------------------
            // Create new payment record
            // -----------------------------------------------------

            var payment = new Payments
            {
                OrderId =
                    request.OrderId,

                UserId =
                    request.UserId,

                Amount =
                    request.Amount,

                Currency =
                    string.IsNullOrWhiteSpace(
                        request.Currency)
                        ? "INR"
                        : request.Currency,

                Status =
                    PaymentStatus.Successful,

                PaymentMethod =
                    "Razorpay",

                TransactionId =
                    request.RazorpayPaymentId,

                ProviderReference =
                    request.RazorpayOrderId,

                FailureReason =
                    null,

                CreatedAt =
                    DateTime.UtcNow,

                ProcessedAt =
                    DateTime.UtcNow,

                UpdatedAt =
                    DateTime.UtcNow
            };

            await _context.Payments.AddAsync(payment);

            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}