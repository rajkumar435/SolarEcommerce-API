using System.ComponentModel.DataAnnotations;

namespace Order.API.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public Orders Order { get; set; }
    }
}