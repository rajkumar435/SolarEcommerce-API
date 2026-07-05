using Order.API.DTOs;

public class CreateOrderDto
{
    public int UserId { get; set; }

    public decimal TotalAmount { get; set; }

    public string FullName { get; set; }

    public string Mobile { get; set; }

    public string Address { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    public string Pincode { get; set; }

    public List<OrderItemDto> Items { get; set; }
}