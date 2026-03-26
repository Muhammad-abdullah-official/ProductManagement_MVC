using System.ComponentModel.DataAnnotations;

namespace ProductManagement.DTOs.AUTH_DTOs
{
    public record RefreshTokenRequest
    (
        [Required] string RefreshToken
    );
}
