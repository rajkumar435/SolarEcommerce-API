using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtTokenGenerator _jwt;
        private readonly IPasswordHasher<User>
    _passwordHasher;
        public AuthService(
      AppDbContext context,
      IJwtTokenGenerator jwt,
      IPasswordHasher<User> passwordHasher)
        {
            _context = context;

            _jwt = jwt;

            _passwordHasher =
                passwordHasher;
        }


        // =========================================================
        // REGISTER
        // =========================================================

        public async Task<bool> Register(
            RegisterDto dto)
        {
            var exists =
                await _context.Users
                    .AnyAsync(x =>
                        x.Username == dto.Username);

            if (exists)
                return false;

            var roleExists =
                await _context.Roles
                    .AnyAsync(x =>
                        x.Id == dto.RoleId &&
                        x.IsActive);

            if (!roleExists)
                throw new Exception(
                    "Invalid Role");

            var user = new User
            {
                Username = dto.Username,

                Email = dto.Email,

                CreatedBy = "System",

                CreatedAt = DateTime.UtcNow,

                IsActive = true
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    dto.Password);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            var userRole = new UserRole
            {
                UserId = user.Id,

                RoleId = dto.RoleId,

                IsActive = true,

                CreatedBy = "System",

                CreatedAt = DateTime.UtcNow
            };

            _context.UserRoles.Add(userRole);

            await _context.SaveChangesAsync();

            return true;
        }

        // =========================================================
        // LOGIN
        // =========================================================

        public async Task<string?> Login(
      string username,
      string password)
        {
            var user =
                await _context.Users
                    .FirstOrDefaultAsync(
                        x =>
                            x.Username == username &&
                            x.IsActive);

            if (user == null)
                return null;

            var passwordResult =
                _passwordHasher
                    .VerifyHashedPassword(
                        user,
                        user.PasswordHash,
                        password);

            if (passwordResult ==
                PasswordVerificationResult.Failed)
            {
                return null;
            }

            var role =
                await (
                    from ur in _context.UserRoles

                    join r in _context.Roles
                        on ur.RoleId equals r.Id

                    where
                        ur.UserId == user.Id &&
                        ur.IsActive &&
                        r.IsActive

                    select r.RoleName
                )
                .FirstOrDefaultAsync();

            if (role == null)
                return null;

            return _jwt.Generate(
                user,
                role);
        }
    }
}