
using System;
using System.ComponentModel.DataAnnotations;

namespace Order.API.DTOs
{
    //public class PaymentRequest
    //{
    //    [Required]
    //    [Range(1, int.MaxValue)]
    //    public int OrderId { get; set; }

    //    [Required]
    //    [Range(1, int.MaxValue)]
    //    public int UserId { get; set; }

    //    [Required]
    //    [Range(typeof(decimal), "0.01", "999999999999999999")]
    //    public decimal Amount { get; set; }

    //    [Required]
    //    public string Currency { get; set; }

    //    [Required]
    //    public string PaymentMethod { get; set; }

    //}
    public class PaymentRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int OrderId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "999999999999999999")]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        public string? UpiId { get; set; }

        public string? CardHolderName { get; set; }

        public string? CardNumber { get; set; }

        public string? ExpiryDate { get; set; }

        public string? Cvv { get; set; }
    }
}