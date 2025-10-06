// ContractMilestoneRepository.cs
using Microsoft.EntityFrameworkCore;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.Repositories.Interfaces;

namespace OCSP.Infrastructure.Repositories
{
    public class ContractMilestoneRepository : IContractMilestoneRepository
    {
        private readonly ApplicationDbContext _db;
        public ContractMilestoneRepository(ApplicationDbContext db) => _db = db;

        public async Task<ContractMilestone?> GetByIdAsync(Guid id, bool includeContract = false, CancellationToken ct = default)
        {
            var q = _db.ContractMilestones.AsQueryable();
            if (includeContract) q = q.Include(x => x.Contract);
            return await q.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public Task<List<ContractMilestone>> ListByContractAsync(Guid contractId, CancellationToken ct = default)
            => _db.ContractMilestones.Where(x => x.ContractId == contractId)
                                     .OrderBy(x => x.CreatedAt)
                                     .ToListAsync(ct);

        public async Task AddAsync(ContractMilestone e, CancellationToken ct = default)
        {
            await _db.ContractMilestones.AddAsync(e, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(ContractMilestone e, CancellationToken ct = default)
        {
            _db.ContractMilestones.Update(e);
            await _db.SaveChangesAsync(ct);
        }
    }
}
