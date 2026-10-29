using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(
            DbContextOptions<PaymentDbContext> options)
            : base(options)
        {
        }

        public DbSet<Payments> Payments { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payments>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payments>()
                .Property(x => x.Currency)
                .HasMaxLength(10);

            modelBuilder.Entity<Payments>()
                .Property(x => x.PaymentMethod)
                .HasMaxLength(50);

            modelBuilder.Entity<Payments>()
                .Property(x => x.TransactionId)
                .HasMaxLength(200);

            modelBuilder.Entity<Payments>()
                .Property(x => x.ProviderReference)
                .HasMaxLength(200);

            modelBuilder.Entity<Payments>()
                .Property(x => x.FailureReason)
                .HasMaxLength(500);
        }
    }
}