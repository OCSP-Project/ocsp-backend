using OCSP.Domain.Entities;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Project>> GetByHomeownerIdAsync(Guid homeownerId, CancellationToken ct = default);
    Task AddAsync(Project project, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
