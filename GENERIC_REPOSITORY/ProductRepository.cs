using Microsoft.EntityFrameworkCore;
using ProductManagement.AppDbContext_EFCore;
using ProductManagement.Models;

namespace ProductManagement.GENERIC_REPOSITORY
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext db) : base(db) { }

        // Override to include related User data (eager loading)
        public override async Task<Product?> GetByIdAsync(Guid id) =>
            await _set.Include(p => p.User)
                      .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Product>> GetByUserIdAsync(Guid userId) =>
            await _set.Where(p => p.UserId == userId)
                      .Include(p => p.User)
                      .AsNoTracking()
                      .ToListAsync();
    }
}
