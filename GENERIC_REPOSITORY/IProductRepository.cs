using ProductManagement.Models;

namespace ProductManagement.GENERIC_REPOSITORY
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetByUserIdAsync(Guid userId);
    }
}
