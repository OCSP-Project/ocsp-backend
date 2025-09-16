using System;
using System.Collections.Generic;

namespace OCSP.Application.DTOs.ProjectTimeline
{
    public class CreateProjectTimelineDto
    {
        public Guid ProjectId { get; set; }
        public Guid ContractorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<CreateMilestoneDto> Milestones { get; set; } = new();
    }

    public class CreateMilestoneDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public List<CreateDeliverableDto> Deliverables { get; set; } = new();
    }

    public class CreateDeliverableDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime PlannedDueDate { get; set; }
    }
}