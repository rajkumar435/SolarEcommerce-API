using BuildingBlocks.Common;

namespace Product.Domain.Entities
{
    public class Products : BaseEntity
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public string? ImagePath { get; set; }

        public string? ImageBase64 { get; set; }

        public int Stock { get; set; }
    }
}