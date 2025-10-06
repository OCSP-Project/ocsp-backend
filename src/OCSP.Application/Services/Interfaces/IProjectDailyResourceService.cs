using OCSP.Application.DTOs.ProjectDailyResource;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OCSP.Application.Services.Interfaces
{
    public interface IProjectDailyResourceService
    {
        // Create & Update (Supervisor, Contractor)
        Task<ProjectDailyResourceDto> CreateDailyResourceAsync(CreateProjectDailyResourceDto dto, Guid userId, CancellationToken cancellationToken = default);
        Task<ProjectDailyResourceDto> UpdateDailyResourceAsync(Guid dailyResourceId, UpdateProjectDailyResourceDto dto, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> DeleteDailyResourceAsync(Guid dailyResourceId, Guid userId, CancellationToken cancellationToken = default);

        // Read (All roles)
        Task<ProjectDailyResourceDto?> GetDailyResourceByIdAsync(Guid dailyResourceId, CancellationToken cancellationToken = default);
        Task<ProjectDailyResourceDto?> GetDailyResourceByProjectAndDateAsync(Guid projectId, DateTime resourceDate, CancellationToken cancellationToken = default);
        Task<List<ProjectDailyResourceListDto>> GetDailyResourcesByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<List<ProjectDailyResourceListDto>> GetDailyResourcesByProjectAndDateRangeAsync(Guid projectId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        // Validation
        Task<bool> CanUserAccessProjectAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CanUserModifyDailyResourceAsync(Guid dailyResourceId, Guid userId, CancellationToken cancellationToken = default);
    }
}
