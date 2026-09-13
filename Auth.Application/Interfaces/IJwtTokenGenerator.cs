using Auth.Domain.Entities;

namespace Auth.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string Generate(User user, string role);
    }

}