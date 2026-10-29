namespace Order.API.DTOs
{
    public class PaymentDetailsDto
    {
        public string? PaymentMethod { get; set; }

        public string? UpiId { get; set; }

        public string? CardHolderName { get; set; }

        public string? CardNumber { get; set; }

        public string? ExpiryDate { get; set; }

        public string? Cvv { get; set; }
    }
}
