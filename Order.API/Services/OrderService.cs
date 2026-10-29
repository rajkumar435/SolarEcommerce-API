using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Order.API.DTOs;
using Order.API.Models;

namespace Order.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            OrderDbContext context,
            ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OrderResponseDto> CreateOrder(
            CreateOrderDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(
                    nameof(dto),
                    "Order request cannot be null.");
            }

            if (dto.UserId <= 0)
            {
                throw new ArgumentException(
                    "Valid UserId is required.");
            }

            if (dto.Items == null ||
                dto.Items.Count == 0)
            {
                throw new ArgumentException(
                    "Order must contain at least one item.");
            }

            if (dto.TotalAmount <= 0)
            {
                throw new ArgumentException(
                    "Order total amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(dto.PaymentMethod))
            {
                throw new ArgumentException(
                    "Payment method is required.");
            }

            var paymentMethod =
                dto.PaymentMethod
                    .Trim()
                    .ToUpperInvariant();

            if (paymentMethod != "RAZORPAY" &&
                paymentMethod != "COD")
            {
                throw new ArgumentException(
                    "Invalid payment method. " +
                    "Allowed values are RAZORPAY and COD.");
            }

            var order =
                new Orders
                {
                    UserId = dto.UserId,

                    TotalAmount =
                        dto.TotalAmount,

                    OrderDate =
                        DateTime.UtcNow,

                    FullName =
                        dto.FullName,

                    Mobile =
                        dto.Mobile,

                    Address =
                        dto.Address,

                    City =
                        dto.City,

                    State =
                        dto.State,

                    Pincode =
                        dto.Pincode,

                    OrderStatus =
                        "PaymentPending",

                    OrderItems =
                        new List<OrderItem>()
                };

            foreach (var item in dto.Items)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.ProductId <= 0)
                {
                    throw new ArgumentException(
                        "ProductId must be greater than zero.");
                }

                if (item.Quantity <= 0)
                {
                    throw new ArgumentException(
                        "Product quantity must be greater than zero.");
                }

                if (item.Price < 0)
                {
                    throw new ArgumentException(
                        "Product price cannot be negative.");
                }

                order.OrderItems.Add(
                    new OrderItem
                    {
                        ProductId =
                            item.ProductId,

                        ProductName =
                            item.ProductName,

                        Price =
                            item.Price,

                        Quantity =
                            item.Quantity
                    });
            }

            if (order.OrderItems.Count == 0)
            {
                throw new ArgumentException(
                    "Order must contain at least one valid item.");
            }

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Order {OrderId} created with PaymentPending status",
                order.OrderId);

            // =========================================================
            // COD
            // =========================================================

            if (paymentMethod == "COD")
            {
                order.OrderStatus =
                    "Confirmed";

                await _context.SaveChangesAsync();

                return new OrderResponseDto
                {
                    Success = true,

                    Message =
                        "Order placed successfully.",

                    OrderId =
                        order.OrderId,

                    TotalAmount =
                        order.TotalAmount,

                    OrderStatus =
                        order.OrderStatus,

                    PaymentStatus =
                        "Pay on Delivery",

                    PaymentMethod =
                        "COD",

                    TransactionId =
                        null
                };
            }

            // =========================================================
            // RAZORPAY
            // =========================================================

            return new OrderResponseDto
            {
                Success = true,

                Message =
                    "Order created. Payment is pending.",

                OrderId =
                    order.OrderId,

                TotalAmount =
                    order.TotalAmount,

                OrderStatus =
                    order.OrderStatus,

                PaymentStatus =
                    "Pending",

                PaymentMethod =
                    "RAZORPAY",

                TransactionId =
                    null
            };
        }

        public async Task<bool> MarkPaymentSuccessful(
    int orderId)
        {
            var order =
                await _context.Orders
                    .FirstOrDefaultAsync(
                        x => x.OrderId == orderId);

            if (order == null)
            {
                return false;
            }

            order.OrderStatus = "Confirmed";

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Order {OrderId} marked as Confirmed after successful payment.",
                orderId);

            return true;
        }
        public async Task<List<Orders>> GetOrders()
        {
            return await _context.Orders
                .Include(x => x.OrderItems)
                .OrderByDescending(x => x.OrderId)
                .ToListAsync();
        }
    }
}
