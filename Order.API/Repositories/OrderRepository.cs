using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Order.API.Models;

namespace Order.API.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(
            OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Orders?> GetById(
            int orderId)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(
                    x => x.OrderId == orderId);
        }

        public async Task Update(
            Orders order)
        {
            _context.Orders.Update(order);

            await Task.CompletedTask;
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}