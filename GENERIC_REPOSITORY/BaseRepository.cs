using ProductManagement.AppDbContext_EFCore;
using ProductManagement.Models;
using ProductManagement.RESPONSES;
using System.Linq.Expressions;

namespace ProductManagement.GENERIC_REPOSITORY
{
    public class BaseRepository<T> : IRepository<T> where T : Entity
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<T> _set;

        public BaseRepository(AppDbContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id) =>
            await _set.FirstOrDefaultAsync(e => e.Id == id);

        public virtual async Task<IEnumerable<T>> GetAllAsync() =>
            await _set.AsNoTracking().ToListAsync();

        // Paginated query — takes an optional filter lambda
        public virtual async Task<PagedResult<T>> GetPagedAsync(
            int page, int pageSize,
            Expression<Func<T, bool>>? filter = null)
        {
            var query = _set.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _set.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            _set.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        // Soft delete — sets IsDeleted = true, never removes the row
        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Entity {id} not found.");
            entity.IsDeleted = true;
            await _db.SaveChangesAsync();
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) =>
            await _set.AnyAsync(predicate);
    }

}
