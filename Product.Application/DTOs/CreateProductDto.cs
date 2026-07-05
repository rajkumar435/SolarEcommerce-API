using Microsoft.AspNetCore.Http;


namespace Product.Application.DTOs


{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        // Image handling (recommended for your current approach)
        public IFormFile? ImageFile { get; set; }

        public string? ImageName { get; set; }
    }
}