using OCSP.Application.DTOs.ProjectTimeline;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
// Use alias to disambiguate enum
using MilestoneStatusEnum = OCSP.Domain.Enums.MilestoneStatus;

namespace OCSP.Application.Services
{
        public class ProjectTimelineService : IProjectTimelineService
        {
            private readonly IProjectTimelineRepository _timelineRepo;
            private readonly IProjectRepository _projectRepo;
            private readonly IContractorRepository _contractorRepo;

            public ProjectTimelineService(
                IProjectTimelineRepository timelineRepo,
                IProjectRepository projectRepo,
                IContractorRepository contractorRepo)
            {
                _timelineRepo = timelineRepo;
                _projectRepo = projectRepo;
                _contractorRepo = contractorRepo;
            }

            public async Task<bool> UpdateDeliverableProgressAsync(UpdateDeliverableProgressDto dto, CancellationToken ct = default)
            {
                // Tìm timeline theo ProjectId
                var timeline = await _timelineRepo.GetByProjectIdWithDetailsAsync(dto.ProjectId, ct);
                if (timeline == null) return false;
                Deliverable? deliverable = null;
                foreach (var milestone in timeline.Milestones)
                {
                    deliverable = milestone.Deliverables.FirstOrDefault(d => d.Id == dto.DeliverableId);
                    if (deliverable != null)
                    {
                        // Validate progress
                        if (dto.ProgressPercentage < 0 || dto.ProgressPercentage > 100)
                            throw new ArgumentException("ProgressPercentage must be between 0 and 100");
                        // Validate ActualCompletionDate >= PlannedDueDate (nếu có)
                        if (dto.ActualCompletionDate.HasValue && dto.ActualCompletionDate.Value < deliverable.PlannedDueDate)
                            throw new ArgumentException("ActualCompletionDate cannot be earlier than PlannedDueDate");
                        deliverable.ProgressPercentage = dto.ProgressPercentage;
                        if (!string.IsNullOrWhiteSpace(dto.Status) && Enum.TryParse<DeliverableStatus>(dto.Status, true, out var status))
                            deliverable.Status = status;
                        if (dto.ActualCompletionDate.HasValue)
                            deliverable.ActualCompletionDate = dto.ActualCompletionDate;
                        break;
                    }
                }
                if (deliverable == null) return false;
                await _timelineRepo.SaveChangesAsync();
                return true;
            }

            public async Task<bool> UpdateMilestoneProgressAsync(UpdateMilestoneProgressDto dto, CancellationToken ct = default)
            {
                var timeline = await _timelineRepo.GetByProjectIdWithDetailsAsync(dto.ProjectId, ct);
                if (timeline == null) return false;
                var milestone = timeline.Milestones.FirstOrDefault(m => m.Id == dto.MilestoneId);
                if (milestone == null) return false;
                // Validate progress
                if (dto.ProgressPercentage < 0 || dto.ProgressPercentage > 100)
                    throw new ArgumentException("ProgressPercentage must be between 0 and 100");
                // Validate ActualStartDate >= PlannedStartDate
                if (dto.ActualStartDate.HasValue && dto.ActualStartDate.Value < milestone.PlannedStartDate)
                    throw new ArgumentException("ActualStartDate cannot be earlier than PlannedStartDate");
                // Validate ActualEndDate >= PlannedEndDate
                if (dto.ActualEndDate.HasValue && dto.ActualEndDate.Value < milestone.PlannedEndDate)
                    throw new ArgumentException("ActualEndDate cannot be earlier than PlannedEndDate");
                milestone.ProgressPercentage = dto.ProgressPercentage;
                if (!string.IsNullOrWhiteSpace(dto.Status) && Enum.TryParse<MilestoneStatusEnum>(dto.Status, true, out var status))
                    milestone.Status = status;
                if (dto.ActualStartDate.HasValue)
                    milestone.ActualStartDate = dto.ActualStartDate;
                if (dto.ActualEndDate.HasValue)
                    milestone.ActualEndDate = dto.ActualEndDate;
                await _timelineRepo.SaveChangesAsync();
                return true;
            }
        public async Task<ProjectTimelineGanttDto> CreateProjectTimelineAsync(CreateProjectTimelineDto dto, CancellationToken ct = default)
        {
            // Validate project and contractor exist
            var project = await _projectRepo.GetByIdAsync(dto.ProjectId, ct) ?? throw new ArgumentException("Project not found");
            var contractor = await _contractorRepo.GetByIdAsync(dto.ContractorId) ?? throw new ArgumentException("Contractor not found");

            // Create ProjectTimeline
            var timeline = new ProjectTimeline
            {
                ProjectId = dto.ProjectId,
                ContractorId = dto.ContractorId,
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Status = TimelineStatus.Planning,
                Milestones = dto.Milestones.Select(m => new Milestone
                {
                    Name = m.Name.Trim(),
                    Description = m.Description?.Trim(),
                    PlannedStartDate = m.PlannedStartDate,
                    PlannedEndDate = m.PlannedEndDate,
                    Status = MilestoneStatusEnum.Planned,
                    Deliverables = m.Deliverables.Select(d => new Deliverable
                    {
                        Name = d.Name.Trim(),
                        Description = d.Description?.Trim(),
                        PlannedDueDate = d.PlannedDueDate,
                        Status = DeliverableStatus.NotStarted
                    }).ToList()
                }).ToList()
            };

            await _timelineRepo.AddAsync(timeline);
            await _timelineRepo.SaveChangesAsync();

            // Map to GanttDto
            return MapToGanttDto(timeline);
        }

        public async Task<ProjectTimelineGanttDto?> GetProjectTimelineForGanttAsync(Guid projectId, CancellationToken ct = default)
        {
            var timeline = await _timelineRepo.GetByProjectIdWithDetailsAsync(projectId, ct);
            if (timeline == null) return null;
            return MapToGanttDto(timeline);
        }

        public async Task<ProjectTimelineGanttDto> AutoCreateTimelineFromMilestonesAsync(AutoCreateTimelineDto dto, CancellationToken ct = default)
        {
            // Validate project and contractor exist
            var project = await _projectRepo.GetByIdAsync(dto.ProjectId, ct) ?? throw new ArgumentException("Project not found");
            var contractor = await _contractorRepo.GetByIdAsync(dto.ContractorId) ?? throw new ArgumentException("Contractor not found");

            // Calculate milestone dates based on duration
            var milestones = new List<Milestone>();
            var currentDate = dto.ProjectStartDate;

            foreach (var milestoneData in dto.Milestones)
            {
                var plannedStartDate = currentDate;
                var plannedEndDate = currentDate.AddDays(milestoneData.DurationInDays);

                var milestone = new Milestone
                {
                    Name = milestoneData.Name,
                    Description = milestoneData.Description,
                    PlannedStartDate = plannedStartDate,
                    PlannedEndDate = plannedEndDate,
                    Status = MilestoneStatusEnum.Planned,
                    Deliverables = milestoneData.Deliverables.Select(deliverableName => new Deliverable
                    {
                        Name = deliverableName,
                        Description = $"Deliverable for {milestoneData.Name}",
                        PlannedDueDate = plannedEndDate, // Same as milestone end date
                        Status = DeliverableStatus.NotStarted
                    }).ToList()
                };

                milestones.Add(milestone);
                currentDate = plannedEndDate.AddDays(1); // Next milestone starts day after previous ends
            }

            // Create ProjectTimeline
            var timeline = new ProjectTimeline
            {
                ProjectId = dto.ProjectId,
                ContractorId = dto.ContractorId,
                Name = dto.TimelineName,
                Description = dto.Description,
                Status = TimelineStatus.Planning,
                Milestones = milestones
            };

            await _timelineRepo.AddAsync(timeline);
            await _timelineRepo.SaveChangesAsync();

            return MapToGanttDto(timeline);
        }

        public async Task<List<MilestoneOverdueDto>> CheckOverdueMilestonesAsync(Guid projectId, CancellationToken ct = default)
        {
            var timeline = await _timelineRepo.GetByProjectIdWithDetailsAsync(projectId, ct);
            if (timeline == null) return new List<MilestoneOverdueDto>();

            var overdueMilestones = new List<MilestoneOverdueDto>();
            var now = DateTime.UtcNow;

            foreach (var milestone in timeline.Milestones)
            {
                // Check if milestone is overdue (planned end date passed but not completed)
                if (milestone.PlannedEndDate < now && milestone.Status != MilestoneStatusEnum.Released)
                {
                    var daysOverdue = (now - milestone.PlannedEndDate).Days;
                    
                    overdueMilestones.Add(new MilestoneOverdueDto
                    {
                        MilestoneId = milestone.Id,
                        MilestoneName = milestone.Name,
                        PlannedEndDate = milestone.PlannedEndDate,
                        DaysOverdue = daysOverdue,
                        Status = milestone.Status.ToString(),
                        ProgressPercentage = milestone.ProgressPercentage,
                        IsOverdue = true
                    });
                }
                // Check if milestone is approaching deadline (within 3 days)
                else if (milestone.PlannedEndDate.AddDays(-3) <= now && milestone.Status != MilestoneStatusEnum.Released)
                {
                    var daysUntilDeadline = (milestone.PlannedEndDate - now).Days;
                    
                    overdueMilestones.Add(new MilestoneOverdueDto
                    {
                        MilestoneId = milestone.Id,
                        MilestoneName = milestone.Name,
                        PlannedEndDate = milestone.PlannedEndDate,
                        DaysOverdue = -daysUntilDeadline, // Negative for approaching deadline
                        Status = milestone.Status.ToString(),
                        ProgressPercentage = milestone.ProgressPercentage,
                        IsOverdue = false,
                        IsApproachingDeadline = true
                    });
                }
            }

            return overdueMilestones;
        }

        private static ProjectTimelineGanttDto MapToGanttDto(ProjectTimeline timeline)
        {
            return new ProjectTimelineGanttDto
            {
                Id = timeline.Id,
                ProjectId = timeline.ProjectId,
                ContractorId = timeline.ContractorId,
                Name = timeline.Name,
                Description = timeline.Description,
                Status = timeline.Status.ToString(),
                Milestones = timeline.Milestones.Select(m => new MilestoneGanttDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    PlannedStartDate = m.PlannedStartDate,
                    PlannedEndDate = m.PlannedEndDate,
                    ActualStartDate = m.ActualStartDate,
                    ActualEndDate = m.ActualEndDate,
                    Status = m.Status.ToString(),
                    ProgressPercentage = m.ProgressPercentage,
                    Deliverables = m.Deliverables.Select(d => new DeliverableGanttDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        PlannedDueDate = d.PlannedDueDate,
                        ActualCompletionDate = d.ActualCompletionDate,
                        Status = d.Status.ToString(),
                        ProgressPercentage = d.ProgressPercentage
                    }).ToList()
                }).ToList()
            };
        }
    }
}