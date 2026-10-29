using Microsoft.Extensions.Logging;
using Order.API.Repositories;

namespace Order.API.Services
{
    public class OrderPaymentService
        : IOrderPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderPaymentService> _logger;

        public OrderPaymentService(
            IOrderRepository orderRepository,
            ILogger<OrderPaymentService> logger)
        {
            _orderRepository =
                orderRepository;

            _logger =
                logger;
        }

        public async Task<bool> MarkPaymentSuccessful(
            int orderId)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException(
                    "Valid OrderId is required.");
            }

            _logger.LogInformation(
                "Processing payment success for OrderId {OrderId}",
                orderId);

            var order =
                await _orderRepository.GetById(
                    orderId);

            if (order == null)
            {
                _logger.LogWarning(
                    "Order not found. OrderId: {OrderId}",
                    orderId);

                return false;
            }

            // -------------------------------------------------
            // Idempotency
            // -------------------------------------------------

            if (order.OrderStatus == "Paid")
            {
                _logger.LogInformation(
                    "Order {OrderId} is already Paid.",
                    orderId);

                return true;
            }

            // -------------------------------------------------
            // Payment must currently be pending
            // -------------------------------------------------

            if (order.OrderStatus != "PaymentPending")
            {
                _logger.LogWarning(
                    "Order {OrderId} cannot be marked Paid. " +
                    "Current status: {OrderStatus}",
                    orderId,
                    order.OrderStatus);

                return false;
            }

            order.OrderStatus = "Paid";

          //  order.UpdatedAt =
             //   DateTime.UtcNow;

            await _orderRepository.Update(order);

            await _orderRepository.SaveChanges();

            _logger.LogInformation(
                "Order {OrderId} marked as Paid.",
                orderId);

            return true;
        }
    }
}