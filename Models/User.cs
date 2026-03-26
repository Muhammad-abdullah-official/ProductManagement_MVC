namespace ProductManagement.Models
{
    public class User : Entity
    {
        public string FirstName { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string PasswordHash { get; set; } = String.Empty;
        public string Role { get; set; } = "user";
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}
