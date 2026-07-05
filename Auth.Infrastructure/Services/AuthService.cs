using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtTokenGenerator _jwt;

        public AuthService(AppDbContext context, IJwtTokenGenerator jwt)
        {
            _context = context;
            _jwt = jwt;
        }


        public async Task<bool> Register(RegisterDto dto)
        {
            var exists = await _context.Users
                .AnyAsync(x => x.Username == dto.Username);

            if (exists)
                return false;

            var roleExists = await _context.Roles
                .AnyAsync(x => x.Id == dto.RoleId);

            if (!roleExists)
                throw new Exception("Invalid Role");

            User user = new User
            {
                Username = dto.Username,
                PasswordHash = dto.Password,
                Email = dto.Email,

                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            UserRole userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = dto.RoleId,

                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.UserRoles.Add(userRole);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string?> Login(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Username == username);

            if (user == null || user.PasswordHash != password)
                return null;

            return _jwt.Generate(user);
        }
    }
}