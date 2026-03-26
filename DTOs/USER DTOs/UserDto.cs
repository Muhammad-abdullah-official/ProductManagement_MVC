namespace ProductManagement.DTOs.USER_DTOs
{
    public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime CreatedAt
);
}
