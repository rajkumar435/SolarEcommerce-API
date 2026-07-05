namespace Auth.Domain.Entities
{
    public class UserRole
    {
     //   public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}