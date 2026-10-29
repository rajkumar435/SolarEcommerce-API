using System;

namespace Payment.Domain.Entities
{
    public class Payments
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int UserId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public PaymentStatus Status { get; set; }

        public string PaymentMethod { get; set; }

        public string? TransactionId { get; set; }

        public string? ProviderReference { get; set; }

        public string? RazorpayOrderId { get; set; }

        public string? RazorpayPaymentId { get; set; }

        public string? FailureReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}