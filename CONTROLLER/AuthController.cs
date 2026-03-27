using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.DTOs.AUTH_DTOs;
using ProductManagement.Models;
using ProductManagement.Services;
using System.Security.Claims;

namespace ProductManagement.CONTROLLER
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        // POST /api/auth/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return StatusCode(result.StatusCode, result);
        }

        // POST /api/auth/logout  (requires valid JWT)
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Extract userId from JWT claims
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _authService.LogoutAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        // GET /api/auth/me  — returns current user info from token
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role);
            var firstName = User.FindFirstValue("firstName");

            return Ok(new { userId, email, role, firstName });
        }
    }
}
