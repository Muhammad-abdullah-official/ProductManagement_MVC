using ProductManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductManagement.AppDbContext_EFCore
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ── Tables ───────────────────────────────
        public DbSet<User> Users => Set<User>();
        public DbSet<Product> Products => Set<Product>();

        public object ChangeTracker { get; private set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ── USER TABLE ───────────────────────
            builder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();   // no duplicate emails
                entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
                entity.Property(u => u.Role).HasDefaultValue("User");

                // Global query filter: automatically exclude soft-deleted rows
                entity.HasQueryFilter(u => !u.IsDeleted);
            });

            // ── PRODUCT TABLE ────────────────────
            builder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Price).HasPrecision(18, 2);

                // Relationship: Product → User (many-to-one)
                entity.HasOne(p => p.User)
                      .WithMany(u => u.Products)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);   // don't cascade delete

                entity.HasQueryFilter(p => !p.IsDeleted);
            });
        }

        // ── Auto-update UpdatedAt before every save ──
        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            return base.SaveChangesAsync(ct);
        }
    }
}
