using System.ComponentModel.DataAnnotations;

namespace ProductManagement.DTOs.AUTH_DTOs
{
    public record LoginRequest
    (
        [Required, EmailAddress] string Email,
        [Required] string Password
    );
}
