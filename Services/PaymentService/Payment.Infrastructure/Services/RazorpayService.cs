using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;


namespace Payment.Infrastructure.Services
{
    public class RazorpayService : IRazorpayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RazorpayService> _logger;
        private readonly IPaymentService _paymentService;

        public RazorpayService(
            IConfiguration configuration,
            ILogger<RazorpayService> logger,
            IPaymentService paymentService)
        {
            _configuration = configuration;
            _logger = logger;
            _paymentService = paymentService;

        }

        public async Task<RazorpayOrderResponse> CreateOrder(
            CreateRazorpayOrderRequest request)
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "Razorpay order request cannot be null.");
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

            var keyId =
                _configuration["Razorpay:KeyId"];

            var keySecret =
                _configuration["Razorpay:KeySecret"];

            if (string.IsNullOrWhiteSpace(keyId) ||
                string.IsNullOrWhiteSpace(keySecret))
            {
                throw new Exception(
                    "Razorpay credentials are not configured.");
            }

            var amountInPaise =
                Convert.ToInt32(
                    Math.Round(
                        request.Amount * 100,
                        0));

            var client =
                new RazorpayClient(
                    keyId,
                    keySecret);

            var options =
                new Dictionary<string, object>();

            options.Add(
                "amount",
                amountInPaise);

            options.Add(
                "currency",
                request.Currency);

            options.Add(
                "receipt",
                "ORDER_" +
                request.OrderId);

            var razorpayOrder =
                client.Order.Create(options);

            string razorpayOrderId =
                Convert.ToString(
                    razorpayOrder["id"]);

            if (string.IsNullOrWhiteSpace(
                razorpayOrderId))
            {
                throw new Exception(
                    "Razorpay did not return an order ID.");
            }

            _logger.LogInformation(
                "Razorpay order created. " +
                "OrderId {OrderId}, " +
                "RazorpayOrderId {RazorpayOrderId}",
                request.OrderId,
                razorpayOrderId);

            return await Task.FromResult(
                new RazorpayOrderResponse
                {
                    RazorpayOrderId =
                        razorpayOrderId,

                    Amount =
                        amountInPaise,

                    Currency =
                        request.Currency,

                    KeyId =
                        keyId
                });
        }

        public  bool  VerifyPayment(
            VerifyPaymentRequest request)
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "Verification request cannot be null.");
            }

            var keySecret =
                _configuration[
                    "Razorpay:KeySecret"];

            if (string.IsNullOrWhiteSpace(
                keySecret))
            {
                throw new Exception(
                    "Razorpay secret is not configured.");
            }

            var payload =
                request.RazorpayOrderId +
                "|" +
                request.RazorpayPaymentId;

            using (
                var hmac =
                    new HMACSHA256(
                        Encoding.UTF8.GetBytes(
                            keySecret)))
            {
                var hash =
                    hmac.ComputeHash(
                        Encoding.UTF8.GetBytes(
                            payload));

                var generatedSignature =
                    BitConverter
                        .ToString(hash)
                        .Replace("-", "")
                        .ToLowerInvariant();

                var isValid =
                    generatedSignature ==
                    request.RazorpaySignature;
                var payment =    _paymentService.RecordSuccessfulRazorpayPayment(request);

                _logger.LogInformation(
                    "Razorpay verification result: {IsValid}",
                    isValid);

                return isValid;
            }
        }
    }
}
