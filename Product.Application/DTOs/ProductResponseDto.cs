namespace Product.Application.DTOs
{
    public class ProductResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? ImagePath { get; set; }

        public int Stock { get; set; }
    }
}