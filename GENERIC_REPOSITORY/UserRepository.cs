using Microsoft.EntityFrameworkCore;
using ProductManagement.AppDbContext_EFCore;
using ProductManagement.Models;

namespace ProductManagement.GENERIC_REPOSITORY
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext db) : base(db) { }

        public async Task<User?> GetByEmailAsync(string email) =>
            await _set.FirstOrDefaultAsync(u => u.Email == email.ToLower());

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken) =>
            await _set.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }
}
