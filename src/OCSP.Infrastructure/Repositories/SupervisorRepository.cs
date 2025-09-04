// OCSP.Infrastructure/Repositories/SupervisorRepository.cs
using Microsoft.EntityFrameworkCore;
using OCSP.Domain.Entities;

using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.Repositories.Interfaces;


namespace OCSP.Infrastructure.Repositories
{
    public class SupervisorRepository : ISupervisorRepository
    {
        private readonly ApplicationDbContext _db;
        public SupervisorRepository(ApplicationDbContext db) => _db = db;

        public async Task<Supervisor?> GetByIdAsync(Guid id, bool includeUser = false)
        {
            IQueryable<Supervisor> q = _db.Set<Supervisor>();
            if (includeUser) q = q.Include(s => s.User);
            return await q.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> ExistsAsync(Guid id)
            => await _db.Set<Supervisor>().AnyAsync(s => s.Id == id);
    }
}
