using ProductManagement.DTOs.AUTH_DTOs;
using ProductManagement.DTOs.USER_DTOs;
using ProductManagement.GENERIC_REPOSITORY;
using ProductManagement.JWT_TOKEN_SERVICE;
using ProductManagement.Models;
using ProductManagement.RESPONSES;

namespace ProductManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IJwtTokenService _jwt;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepo,
            IJwtTokenService jwt,
            IConfiguration config,
            ILogger<AuthService> logger)
        {
            _userRepo = userRepo;
            _jwt = jwt;
            _config = config;
            _logger = logger;
        }

        // ── REGISTER ──────────────────────────────
        public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest req)
        {
            // Business rule: email must be unique
            var exists = await _userRepo.ExistsAsync(u => u.Email == req.Email.ToLower());
            if (exists)
                return ApiResponse<AuthResponse>.Fail("Email already registered.");

            var user = new User
            {
                Email = req.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                FirstName = req.FirstName,
                LastName = req.LastName,
                Role = "User"
            };

            await _userRepo.AddAsync(user);
            _logger.LogInformation("New user registered: {Email}", user.Email);

            return await BuildAuthResponse(user, 201);
        }

        // ── LOGIN ─────────────────────────────────
        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest req)
        {
            var user = await _userRepo.GetByEmailAsync(req.Email);

            // Use same error for "not found" and "wrong password" — prevents user enumeration
            if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return ApiResponse<AuthResponse>.Fail("Invalid email or password.", 401);

            _logger.LogInformation("User logged in: {Email}", user.Email);
            return await BuildAuthResponse(user);
        }

        // ── REFRESH TOKEN ─────────────────────────
        public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepo.GetByRefreshTokenAsync(refreshToken);

            if (user is null || user.RefreshTokenExpiry < DateTime.UtcNow)
                return ApiResponse<AuthResponse>.Unauthorized("Invalid or expired refresh token.");

            return await BuildAuthResponse(user);
        }

        // ── LOGOUT ────────────────────────────────
        public async Task<ApiResponse<bool>> LogoutAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user is null) return ApiResponse<bool>.NotFound();

            // Invalidate the refresh token in DB
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userRepo.UpdateAsync(user);

            return ApiResponse<bool>.Ok(true, "Logged out successfully.");
        }

        // ── PRIVATE HELPER ────────────────────────
        private async Task<ApiResponse<AuthResponse>> BuildAuthResponse(User user, int statusCode = 200)
        {
            var accessToken = _jwt.GenerateAccessToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();
            var expiryMinutes = int.Parse(_config["JwtSettings:ExpiryInMinutes"]!);
            var refreshDays = int.Parse(_config["JwtSettings:RefreshTokenExpiryInDays"]!);

            // Persist refresh token to DB (hashed in production — simplified here)
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshDays);
            await _userRepo.UpdateAsync(user);

            var response = new AuthResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                ExpiresAt: DateTime.UtcNow.AddMinutes(expiryMinutes),
                User: MapToDto(user)
            );

            return statusCode == 201
                ? ApiResponse<AuthResponse>.Created(response)
                : ApiResponse<AuthResponse>.Ok(response);
        }

        private static UserDto MapToDto(User u) =>
            new(u.Id, u.Email, u.FirstName, u.LastName, u.Role, u.CreatedAt);
    }
}
