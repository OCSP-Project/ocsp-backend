using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Repositories.Interfaces
{
    public interface IProgressMediaRepository
    {
        Task<ProgressMedia?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<ProgressMedia>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
        Task<List<ProgressMedia>> GetByProjectIdAsync(Guid projectId, Guid? taskId, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct = default);
        Task<int> GetCountByProjectIdAsync(Guid projectId, Guid? taskId, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
        Task<ProgressMedia> AddAsync(ProgressMedia progressMedia, CancellationToken ct = default);
        Task DeleteAsync(ProgressMedia progressMedia, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
