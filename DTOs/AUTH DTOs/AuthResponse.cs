using ProductManagement.DTOs.USER_DTOs;

namespace ProductManagement.DTOs.AUTH_DTOs
{
    public record AuthResponse
    (
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
    );
}
