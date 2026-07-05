using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(
        RegisterDto dto)
        {
            var result =
                await _service.Register(dto);

            if (!result)
                return BadRequest("Username already exists");

            return Ok("User Registered");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var token = await _service.Login(dto.Username, dto.Password);

            if (token == null)
                return Unauthorized();

            return Ok(new { token });
        }

        [Authorize]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Authorized");
        }
    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}