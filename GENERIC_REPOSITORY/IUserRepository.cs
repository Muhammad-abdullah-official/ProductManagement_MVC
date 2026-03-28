using ProductManagement.Models;

namespace ProductManagement.GENERIC_REPOSITORY
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
    }
}
