using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                service = "Auth",
                status = "Healthy"
            });
        }
    }


    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        // ============================
        // REGISTER
        // ============================

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _service.Register(dto);

            if (!result)
                return BadRequest("Username already exists");

            return Ok(new
            {
                message = "User Registered Successfully"
            });
        }


        // ============================
        // LOGIN
        // ============================

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var token = await _service.Login(
                dto.Username,
                dto.Password);

            if (token == null)
                return Unauthorized(new
                {
                    message = "Invalid username or password"
                });

            return Ok(new
            {
                token = token
            });
        }


        // ============================
        // AUTHENTICATION TEST
        // ============================

        [Authorize]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                message = "Authentication successful",

                userId =
                    User.FindFirst(
                        ClaimTypes.NameIdentifier)?.Value,

                username =
                    User.FindFirst(
                        ClaimTypes.Name)?.Value,

                role =
                    User.FindFirst(
                        ClaimTypes.Role)?.Value
            });
        }


        // ============================
        // ADMIN TEST
        // ============================

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-test")]
        public IActionResult AdminTest()
        {
            return Ok(new
            {
                message = "Admin authorization successful",

                userId =
                    User.FindFirst(
                        ClaimTypes.NameIdentifier)?.Value,

                username =
                    User.FindFirst(
                        ClaimTypes.Name)?.Value,

                role =
                    User.FindFirst(
                        ClaimTypes.Role)?.Value
            });
        }


        // ============================
        // CUSTOMER TEST
        // ============================

        [Authorize(Roles = "Customer")]
        [HttpGet("customer-test")]
        public IActionResult CustomerTest()
        {
            return Ok(new
            {
                message = "Customer authorization successful",

                userId =
                    User.FindFirst(
                        ClaimTypes.NameIdentifier)?.Value,

                username =
                    User.FindFirst(
                        ClaimTypes.Name)?.Value,

                role =
                    User.FindFirst(
                        ClaimTypes.Role)?.Value
            });
        }
    }


    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}