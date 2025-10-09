using OCSP.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OCSP.Infrastructure.Repositories.Interfaces
{
    public interface IProjectTimelineRepository
    {
        Task AddAsync(ProjectTimeline timeline);
        Task SaveChangesAsync();
        Task<ProjectTimeline?> GetByProjectIdWithDetailsAsync(Guid projectId, CancellationToken ct = default);
    }
}