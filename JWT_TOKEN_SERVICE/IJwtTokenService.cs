using ProductManagement.Models;
using System.Security.Claims;

namespace ProductManagement.JWT_TOKEN_SERVICE
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateAccessToken(string token);
    }
}
