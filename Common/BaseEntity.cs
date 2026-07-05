namespace BuildingBlocks.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        public bool IsActive { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}