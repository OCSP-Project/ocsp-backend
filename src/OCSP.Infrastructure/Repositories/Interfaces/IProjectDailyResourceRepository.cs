using OCSP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OCSP.Infrastructure.Repositories.Interfaces
{
    public interface IProjectDailyResourceRepository
    {
        Task<ProjectDailyResource?> GetByIdAsync(Guid dailyResourceId, CancellationToken cancellationToken = default);
        Task<ProjectDailyResource?> GetByProjectAndDateAsync(Guid projectId, DateTime resourceDate, CancellationToken cancellationToken = default);
        Task<List<ProjectDailyResource>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<List<ProjectDailyResource>> GetByProjectIdAndDateRangeAsync(Guid projectId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<ProjectDailyResource> CreateAsync(ProjectDailyResource entity, CancellationToken cancellationToken = default);
        Task<ProjectDailyResource> UpdateAsync(ProjectDailyResource entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid dailyResourceId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid projectId, DateTime resourceDate, CancellationToken cancellationToken = default);
    }
}
