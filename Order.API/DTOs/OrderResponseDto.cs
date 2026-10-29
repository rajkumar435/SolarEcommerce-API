namespace Order.API.DTOs
{
    public class OrderResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public int OrderId { get; set; }

        public decimal TotalAmount { get; set; }

        public string OrderStatus { get; set; }

        public string PaymentStatus { get; set; }

        public string PaymentMethod { get; set; }

        public string? TransactionId { get; set; }
    }
}
