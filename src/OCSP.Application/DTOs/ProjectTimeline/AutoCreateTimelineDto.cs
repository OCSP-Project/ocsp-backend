using System;
using System.Collections.Generic;

namespace OCSP.Application.DTOs.ProjectTimeline
{
    public class AutoCreateTimelineDto
    {
        public Guid ProjectId { get; set; }
        public Guid ContractorId { get; set; }
        public string TimelineName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ProjectStartDate { get; set; }
        public List<MilestoneDataDto> Milestones { get; set; } = new();
    }

    public class MilestoneDataDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationInDays { get; set; }
        public List<string> Deliverables { get; set; } = new();
    }
}
