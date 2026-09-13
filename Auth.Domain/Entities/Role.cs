using BuildingBlocks.Common;

namespace Auth.Domain.Entities
{
    public class Role : BaseEntity
    {
        // public string Name { get; set; }
        public string RoleName { get; set; } = string.Empty;

    }
}