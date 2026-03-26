namespace ProductManagement.Models
{
    public class Product : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }

        // Foreign key → User (creator)
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
