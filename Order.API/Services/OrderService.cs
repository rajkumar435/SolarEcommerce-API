using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Order.API.DTOs;
using Order.API.Models;

namespace Order.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderDbContext _context;

        public OrderService(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Orders> CreateOrder(
            CreateOrderDto dto)
        {
            var order = new Orders
            {
                UserId = dto.UserId,
                TotalAmount = dto.TotalAmount,
                OrderDate = DateTime.Now,

                FullName = dto.FullName,
                Mobile = dto.Mobile,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                Pincode = dto.Pincode,

                OrderItems = new List<OrderItem>()   // IMPORTANT
            };

            foreach (var item in dto.Items)
            {
                order.OrderItems.Add(
                    new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Price = item.Price,
                        Quantity = item.Quantity
                    });
            }

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            return order;
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