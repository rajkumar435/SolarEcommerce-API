using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using System.Collections.Generic;

namespace Order.API.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(
            DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        public DbSet<Orders> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }
    }
}