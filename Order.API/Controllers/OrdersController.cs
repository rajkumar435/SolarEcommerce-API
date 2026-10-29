using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Services;

namespace Order.API.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                service = "Order",
                status = "Healthy"
            });
        }
    }

    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderPaymentService _orderPaymentService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
       IOrderService orderService,
       IOrderPaymentService orderPaymentService,
       ILogger<OrdersController> logger)
        {
            _orderService =
                orderService;

            _orderPaymentService =
                orderPaymentService;

            _logger =
                logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(
            [FromBody] CreateOrderDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "Order request cannot be null."
                    });
                }

                var result =
                    await _orderService.CreateOrder(dto);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Invalid order request.");

                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Order creation failed.");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Success = false,
                        Message =
                            "Unable to create order."
                    });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders =
                await _orderService.GetOrders();

            return Ok(orders);
        }

        [HttpPut("{orderId}/payment-success")]
        public async Task<IActionResult> MarkPaymentSuccessful(
      int orderId)
        {
            try
            {
                _logger.LogInformation(
                    "Payment success callback received. " +
                    "OrderId: {OrderId}",
                    orderId);

                if (orderId <= 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Valid OrderId is required."
                    });
                }

                var result =
                    await _orderPaymentService
                        .MarkPaymentSuccessful(orderId);

                if (!result)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message =
                            "Unable to mark order as Paid."
                    });
                }

                return Ok(new
                {
                    Success = true,
                    OrderId = orderId,
                    OrderStatus = "Paid",
                    Message =
                        "Order payment status updated successfully."
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Invalid payment success request. " +
                    "OrderId: {OrderId}",
                    orderId);

                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating payment status. " +
                    "OrderId: {OrderId}",
                    orderId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Success = false,
                        Message =
                            "Unable to update order payment status."
                    });
            }
        }
        //[HttpPost("test-payment")]
        //public async Task<IActionResult> TestPayment(
        //      [FromBody] PaymentRequest request)
        //{
        //    var result =
        //        await _paymentClient.ProcessPayment(request);

        //    return Ok(result);
        //}
    }
}
