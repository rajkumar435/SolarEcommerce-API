using Microsoft.EntityFrameworkCore;
using Product.Domain.Entities;
using System.Collections.Generic;

namespace Product.Infrastructure.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options)
        {
        }

        public DbSet<Products> Products { get; set; }
    }
}