using Microsoft.EntityFrameworkCore;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.Repositories.Interfaces;

namespace OCSP.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<User?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }
    }
}