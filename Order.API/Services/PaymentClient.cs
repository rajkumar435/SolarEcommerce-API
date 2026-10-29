using System.Net.Http;
using System.Text;
using System.Text.Json;
using Order.API.DTOs;

namespace Order.API.Services
{
    public class PaymentClient : IPaymentClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentClient> _logger;

        public PaymentClient(
            HttpClient httpClient,
            ILogger<PaymentClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PaymentResponse> ProcessPayment(
            PaymentRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(
                    nameof(request));
            }

            _logger.LogInformation(
                "Calling Payment API for OrderId {OrderId}",
                request.OrderId);

            var json =
                JsonSerializer.Serialize(request);

            var content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

            var response =
                await _httpClient.PostAsync(
                    "/api/payments",
                    content);

            var result =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Payment API failed. OrderId {OrderId}, " +
                    "StatusCode {StatusCode}, Response {Response}",
                    request.OrderId,
                    response.StatusCode,
                    result);

                throw new Exception(
                    "Payment API failed. StatusCode: " +
                    response.StatusCode +
                    ". Response: " +
                    result);
            }

            _logger.LogInformation(
                "Payment API completed for OrderId {OrderId}",
                request.OrderId);

            var paymentResponse =
                JsonSerializer.Deserialize<PaymentResponse>(
                    result,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (paymentResponse == null)
            {
                throw new Exception(
                    "Invalid response received from Payment API.");
            }

            return paymentResponse;
        }
    }
}
