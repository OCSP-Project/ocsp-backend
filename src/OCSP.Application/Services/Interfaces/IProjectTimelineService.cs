using OCSP.Application.DTOs.ProjectTimeline;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OCSP.Application.Services.Interfaces
{
    public interface IProjectTimelineService
    {
        Task<ProjectTimelineGanttDto> CreateProjectTimelineAsync(CreateProjectTimelineDto dto, CancellationToken ct = default);
        Task<ProjectTimelineGanttDto?> GetProjectTimelineForGanttAsync(Guid projectId, CancellationToken ct = default);
        Task<bool> UpdateDeliverableProgressAsync(UpdateDeliverableProgressDto dto, CancellationToken ct = default);
        Task<bool> UpdateMilestoneProgressAsync(UpdateMilestoneProgressDto dto, CancellationToken ct = default);
    }
}