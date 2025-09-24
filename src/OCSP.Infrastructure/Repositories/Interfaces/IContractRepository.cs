// IContractRepository.cs
using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Repositories.Interfaces
{
    public interface IContractRepository
    {
        Task<Contract?> GetByIdAsync(Guid id, bool includeMilestones = false, CancellationToken ct = default);
        Task<List<Contract>> ListByProjectAsync(Guid projectId, CancellationToken ct = default);
        Task AddAsync(Contract entity, CancellationToken ct = default);
        Task UpdateAsync(Contract entity, CancellationToken ct = default);
    }
}
