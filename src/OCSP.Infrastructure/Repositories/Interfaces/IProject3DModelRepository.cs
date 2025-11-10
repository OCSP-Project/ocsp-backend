using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Repositories.Interfaces
{
    public interface IProject3DModelRepository
    {
        Task<Project3DModel?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<List<Project3DModel>> ListByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<Project3DModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Project3DModel?> GetLatestByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task CreateAsync(Project3DModel model, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
