using System;
using System.Collections.Generic;

namespace OCSP.Application.DTOs.ProjectTimeline
{
    public class ProjectTimelineGanttDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid ContractorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<MilestoneGanttDto> Milestones { get; set; } = new();
    }

    public class MilestoneGanttDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal ProgressPercentage { get; set; }
        public List<DeliverableGanttDto> Deliverables { get; set; } = new();
    }

    public class DeliverableGanttDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime PlannedDueDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal ProgressPercentage { get; set; }
    }
}