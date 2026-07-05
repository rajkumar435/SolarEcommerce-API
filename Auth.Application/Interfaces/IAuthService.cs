using Auth.Application.DTOs;

namespace Auth.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string?> Login(string username, string password);
        Task<bool> Register(RegisterDto dto);
    }
}