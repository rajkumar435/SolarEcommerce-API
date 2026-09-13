using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Moq;

using Xunit;

namespace Auth.Tests
{
    public class AuthServiceTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private (
            AuthService service,
            AppDbContext context,
            Mock<IJwtTokenGenerator> jwtMock,
            Mock<IPasswordHasher<User>> passwordHasherMock
        ) CreateService()
        {
            var context = CreateContext();

            var jwtMock =
                new Mock<IJwtTokenGenerator>();

            var passwordHasherMock =
                new Mock<IPasswordHasher<User>>();

            var service = new AuthService(
                context,
                jwtMock.Object,
                passwordHasherMock.Object);

            return (
                service,
                context,
                jwtMock,
                passwordHasherMock
            );
        }


        [Fact]
        public async Task Register_ShouldCreateUser_WhenValidData()
        {
            // Arrange
            var (
                service,
                context,
                jwtMock,
                passwordHasherMock
            ) = CreateService();

            context.Roles.Add(new Role
            {
                Id = 1,
                RoleName = "Admin",
                IsActive = true
            });

            await context.SaveChangesAsync();

            var dto = new RegisterDto
            {
                Username = "admin",
                Password = "Password123",
                Email = "admin@test.com",
                RoleId = 1
            };

            passwordHasherMock
                .Setup(x => x.HashPassword(
                    It.IsAny<User>(),
                    "Password123"))
                .Returns("HASHED_PASSWORD");

            // Act
            var result = await service.Register(dto);

            // Assert
            Assert.True(result);

            var user = await context.Users
                .FirstOrDefaultAsync(x => x.Username == "admin");

            Assert.NotNull(user);
            Assert.Equal("admin@test.com", user.Email);

            var userRole = await context.UserRoles
                .FirstOrDefaultAsync(x => x.UserId == user.Id);

            Assert.NotNull(userRole);
            Assert.Equal(1, userRole.RoleId);
        }

        [Fact]
        public async Task Register_ShouldReturnFalse_WhenUsernameAlreadyExists()
        {
            // Arrange
            var (
                service,
                context,
                jwtMock,
                passwordHasherMock
            ) = CreateService();

            context.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = "HASH",
                Email = "old@test.com",
                IsActive = true
            });

            await context.SaveChangesAsync();

            var dto = new RegisterDto
            {
                Username = "admin",
                Password = "Password123",
                Email = "new@test.com",
                RoleId = 1
            };

            // Act
            var result = await service.Register(dto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Register_ShouldThrowException_WhenRoleDoesNotExist()
        {
            // Arrange
            var (
                service,
                context,
                jwtMock,
                passwordHasherMock
            ) = CreateService();

            var dto = new RegisterDto
            {
                Username = "user1",
                Password = "Password123",
                Email = "user@test.com",
                RoleId = 999
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => service.Register(dto));
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var (
                service,
                context,
                jwtMock,
                passwordHasherMock
            ) = CreateService();

            var user = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "HASHED_PASSWORD",
                Email = "admin@test.com",
                IsActive = true
            };

            context.Users.Add(user);

            context.Roles.Add(new Role
            {
                Id = 1,
                RoleName = "Admin",
                IsActive = true
            });

            context.UserRoles.Add(new UserRole
            {
                UserId = 1,
                RoleId = 1,
                IsActive = true
            });

            await context.SaveChangesAsync();

            passwordHasherMock
                .Setup(x => x.VerifyHashedPassword(
                    It.IsAny<User>(),
                    "HASHED_PASSWORD",
                    "Password123"))
                .Returns(PasswordVerificationResult.Success);

            jwtMock
                .Setup(x => x.Generate(It.IsAny<User>(), "Admin"))
                .Returns("fake-jwt-token");

            // Act
            var token = await service.Login(
                "admin",
                "Password123");

            // Assert
            Assert.NotNull(token);
            Assert.Equal("fake-jwt-token", token);

            jwtMock.Verify(
                x => x.Generate(
                    It.Is<User>(u => u.Username == "admin"),
                    "Admin"),
                Times.Once);
        }

        [Fact]
        public async Task Login_ShouldReturnNull_WhenPasswordIsWrong()
        {
            // Arrange
            var (
                service,
                context,
                jwtMock,
                passwordHasherMock
            ) = CreateService();

            var user = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "HASHED_PASSWORD",
                Email = "admin@test.com",
                IsActive = true
            };

            context.Users.Add(user);

            context.Roles.Add(new Role
            {
                Id = 1,
                RoleName = "Admin",
                IsActive = true
            });

            context.UserRoles.Add(new UserRole
            {
                UserId = 1,
                RoleId = 1,
                IsActive = true
            });

            await context.SaveChangesAsync();

            passwordHasherMock
                .Setup(x => x.VerifyHashedPassword(
                    It.IsAny<User>(),
                    "HASHED_PASSWORD",
                    "WrongPassword"))
                .Returns(PasswordVerificationResult.Failed);

            // Act
            var token = await service.Login(
                "admin",
                "WrongPassword");

            // Assert
            Assert.Null(token);

            jwtMock.Verify(
                x => x.Generate(
                    It.IsAny<User>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Login_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var (
                service,
                context,
                jwtMock,
                passwordHasherMock
            ) = CreateService();

            // Act
            var token = await service.Login(
                "unknown",
                "Password123");

            // Assert
            Assert.Null(token);

            jwtMock.Verify(
                x => x.Generate(
                    It.IsAny<User>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Login_ShouldReturnNull_WhenUserIsInactive()
        {
            // Arrange
            var (
                service,
                context,
                jwtMock,
                passwordHasherMock
            ) = CreateService();

            context.Users.Add(new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "HASHED",
                Email = "admin@test.com",
                IsActive = false
            });

            await context.SaveChangesAsync();

            // Act
            var token = await service.Login(
                "admin",
                "Password123");

            // Assert
            Assert.Null(token);
        }


    }
}