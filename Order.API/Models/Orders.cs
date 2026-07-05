using System.ComponentModel.DataAnnotations;

namespace Order.API.Models
{
    public class Orders
    {
        [Key]
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime? OrderDate { get; set; }

        public string? FullName { get; set; }

        public string? Mobile { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Pincode { get; set; }

        public string? OrderStatus { get; set; }
        public ICollection<OrderItem> OrderItems
        {
            get;
            set;
        } = new List<OrderItem>();   // IMPORTANT
    }
}