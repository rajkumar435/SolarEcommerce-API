
using System;
using Payment.Domain.Entities;

namespace Payment.Application.DTOs
{
    public class PaymentResponse
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int UserId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public PaymentStatus Status { get; set; }

        public string PaymentMethod { get; set; }

        public string TransactionId { get; set; }

        public string FailureReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }
    }

    //public enum PaymentStatus
    //{
    //    Pending = 1,
    //    Processing = 2,
    //    Successful = 3,
    //    Failed = 4,
    //    Cancelled = 5,
    //    Refunded = 6
    //}
}