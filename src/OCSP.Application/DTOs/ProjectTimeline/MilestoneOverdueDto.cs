using System;

namespace OCSP.Application.DTOs.ProjectTimeline
{
    public class MilestoneOverdueDto
    {
        public Guid MilestoneId { get; set; }
        public string MilestoneName { get; set; } = string.Empty;
        public DateTime PlannedEndDate { get; set; }
        public int DaysOverdue { get; set; } // Positive = overdue, Negative = approaching deadline
        public string Status { get; set; } = string.Empty;
        public decimal ProgressPercentage { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsApproachingDeadline { get; set; }
    }
}
