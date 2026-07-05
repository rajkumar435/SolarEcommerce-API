using BuildingBlocks.Common;

namespace Auth.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
    }
}