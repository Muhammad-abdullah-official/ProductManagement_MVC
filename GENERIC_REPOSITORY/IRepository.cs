using ProductManagement.Models;
using ProductManagement.RESPONSES;
using System.Linq.Expressions;

namespace ProductManagement.GENERIC_REPOSITORY
{
    public interface IRepository<T> where T : Entity
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? filter = null);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(Guid id);                  // soft delete
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }
}
