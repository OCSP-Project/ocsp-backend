// IContractMilestoneRepository.cs
using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Repositories.Interfaces
{
    public interface IContractMilestoneRepository
    {
        Task<ContractMilestone?> GetByIdAsync(Guid id, bool includeContract = false, CancellationToken ct = default);
        Task<List<ContractMilestone>> ListByContractAsync(Guid contractId, CancellationToken ct = default);
        Task AddAsync(ContractMilestone entity, CancellationToken ct = default);
        Task UpdateAsync(ContractMilestone entity, CancellationToken ct = default);
    }
}
