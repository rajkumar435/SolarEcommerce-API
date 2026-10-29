using Microsoft.Extensions.Logging;
using Payment.Application.Interfaces;

namespace Payment.Infrastructure.Services
{
    public class OrderClient : IOrderClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderClient> _logger;

        public OrderClient(
            HttpClient httpClient,
            ILogger<OrderClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> MarkPaymentSuccessful(
            int orderId)
        {
            try
            {
                _logger.LogInformation(
                    "Updating Order API payment success. OrderId: {OrderId}",
                    orderId);

                var response =
                    await _httpClient.PutAsync(
                        "api/orders/" +
                        orderId +
                        "/payment-success",
                        null);

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody =
                        await response.Content.ReadAsStringAsync();

                    _logger.LogError(
                        "Failed to update Order API. " +
                        "OrderId: {OrderId}, StatusCode: {StatusCode}, Response: {Response}",
                        orderId,
                        response.StatusCode,
                        responseBody);

                    return false;
                }

                _logger.LogInformation(
                    "Order API updated successfully. " +
                    "OrderId: {OrderId}",
                    orderId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception while updating Order API. " +
                    "OrderId: {OrderId}",
                    orderId);

                return false;
            }
        }
    }
}