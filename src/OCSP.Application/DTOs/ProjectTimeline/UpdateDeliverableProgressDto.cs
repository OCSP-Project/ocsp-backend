using System;

namespace OCSP.Application.DTOs.ProjectTimeline
{
    public class UpdateDeliverableProgressDto
    {
        public Guid ProjectId { get; set; }
        public Guid DeliverableId { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string? Status { get; set; } // NotStarted, InProgress, Completed, OnHold
        public DateTime? ActualCompletionDate { get; set; }
    }
    public class UpdateMilestoneProgressDto
    {
        public Guid ProjectId { get; set; }
        public Guid MilestoneId { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string? Status { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
    }
}