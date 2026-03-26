using System.ComponentModel.DataAnnotations;

namespace ProductManagement.DTOs.AUTH_DTOs
{
    public record RegisterRequest
    (
        [Required, EmailAddress] string Email,
        [Required, MinLength(6), MaxLength(100)] string Password,
        [Required] string FirstName,
        [Required] string LastName
    );
}
