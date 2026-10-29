namespace Payment.Application.DTOs
{
    public class VerifyPaymentRequest
    {
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public int PaymentId { get; set; }

        public string RazorpayOrderId { get; set; }

        public string RazorpayPaymentId { get; set; }

        public string RazorpaySignature { get; set; }
    }
}