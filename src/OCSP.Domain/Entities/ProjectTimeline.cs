using OCSP.Domain.Common;
using System;
using System.Collections.Generic;

namespace OCSP.Domain.Entities
{
    public class ProjectTimeline : AuditableEntity
    {
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public Guid ContractorId { get; set; }
        public Contractor Contractor { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TimelineStatus Status { get; set; } = TimelineStatus.Planning;
        public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    }

    public class Milestone : AuditableEntity
    {
        public Guid ProjectTimelineId { get; set; }
        public ProjectTimeline ProjectTimeline { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public MilestoneStatus Status { get; set; } = MilestoneStatus.NotStarted;
        public decimal ProgressPercentage { get; set; } = 0;
        public ICollection<Deliverable> Deliverables { get; set; } = new List<Deliverable>();
    }

    public class Deliverable : AuditableEntity
    {
        public Guid MilestoneId { get; set; }
        public Milestone Milestone { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime PlannedDueDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public DeliverableStatus Status { get; set; } = DeliverableStatus.NotStarted;
        public decimal ProgressPercentage { get; set; } = 0;
    }

    public enum TimelineStatus
    {
        Planning = 0,
        InProgress = 1,
        Completed = 2,
        OnHold = 3
    }

    public enum MilestoneStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        OnHold = 3
    }

    public enum DeliverableStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        OnHold = 3
    }
}
