// ContractRepository.cs
using Microsoft.EntityFrameworkCore;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.Repositories.Interfaces;

namespace OCSP.Infrastructure.Repositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly ApplicationDbContext _db;
        public ContractRepository(ApplicationDbContext db) => _db = db;

        public async Task<Contract?> GetByIdAsync(Guid id, bool includeMilestones = false, CancellationToken ct = default)
        {
            var q = _db.Contracts.AsQueryable();
            if (includeMilestones) q = q.Include(c => c.Milestones);
            return await q.FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public Task<List<Contract>> ListByProjectAsync(Guid projectId, CancellationToken ct = default)
            => _db.Contracts.Where(c => c.ProjectId == projectId)
                            .OrderByDescending(c => c.CreatedAt)
                            .ToListAsync(ct);

        public async Task AddAsync(Contract e, CancellationToken ct = default)
        {
            await _db.Contracts.AddAsync(e, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Contract e, CancellationToken ct = default)
        {
            _db.Contracts.Update(e);
            await _db.SaveChangesAsync(ct);
        }
    }
}
