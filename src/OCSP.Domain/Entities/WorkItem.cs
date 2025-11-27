using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public enum WorkItemStatus
    {
        NotStarted = 0,         // Not started
        InProgress = 1,         // In progress (blue)
        Completed = 2,          // Completed (green)
        Overdue = 3,            // Overdue (red)
        Paused = 4,             // Paused
        Cancelled = 5           // Cancelled
    }

    public enum WorkItemType
    {
        Phase = 1,              // Phase (Level 1)
        Task = 2,               // Task (Level 2)
        SubTask = 3,            // Sub-task (Level 3+)
        Milestone = 4           // Milestone
    }

    public class WorkItem : AuditableEntity
    {
        // Basic Information
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

        public string Code { get; set; } = string.Empty;                 // Code: "W00024", "2.5"
        public string OrderNumber { get; set; } = string.Empty;          // Order: "1", "2.1", "2.1.3"
        public string Name { get; set; } = string.Empty;                 // Work item name
        public string? Description { get; set; }                         // Description

        // Tree Structure
        public Guid? ParentId { get; set; }                              // Parent ID
        public WorkItem? Parent { get; set; }
        public ICollection<WorkItem> Children { get; set; } = new List<WorkItem>();

        public int Level { get; set; }                                   // Level (1, 2, 3...)
        public int SortOrder { get; set; }                               // Sort order
        public WorkItemType Type { get; set; } = WorkItemType.Task;

        // Timeline
        public DateTime? PlannedStartDate { get; set; }                  // Planned start date
        public DateTime? PlannedEndDate { get; set; }                    // Planned end date
        public DateTime? ActualStartDate { get; set; }                   // Actual start date
        public DateTime? ActualEndDate { get; set; }                     // Actual end date

        public int PlannedDuration { get; set; }                         // Planned days
        public int? ActualDuration { get; set; }                         // Actual days

        // Progress
        public decimal Progress { get; set; }                            // Progress % (0-100)
        public WorkItemStatus Status { get; set; } = WorkItemStatus.NotStarted;

        // K factor (compaction coefficient - for earthwork)
        public decimal? CompactionFactor { get; set; }                   // e.g., 0.95, 0.85

        // Quantity
        public decimal? PlannedQuantity { get; set; }                    // Planned quantity
        public string? Unit { get; set; }                                // Unit (m³, m², pcs...)
        public decimal? ActualQuantity { get; set; }                     // Actual quantity

        // Financial
        public decimal? UnitPrice { get; set; }                          // Unit price (VND)
        public decimal? TotalAmount { get; set; }                        // Total = quantity * price

        // Manpower
        public string? AssigneeIds { get; set; }                         // JSON array of User IDs
        public string? ResponsiblePersonId { get; set; }                 // Responsible person User ID

        // Dependencies
        public string? PrerequisiteIds { get; set; }                     // JSON array of prerequisite task IDs

        // Display state
        public bool IsExpanded { get; set; } = false;                    // Expanded/Collapsed
        public bool IsHidden { get; set; } = false;                      // Hidden/Visible
        public bool IsDeleted { get; set; } = false;                     // Soft delete

        // Alerts and notes
        public string? Notes { get; set; }                               // Notes
        public string? Warnings { get; set; }                            // Warnings (JSON array)

        // Import tracking
        public string? ImportData { get; set; }                          // JSON data from Excel import
        public DateTime? ImportedAt { get; set; }

        // Navigation Properties
        public ICollection<WorkItemActivity> Activities { get; set; } = new List<WorkItemActivity>();
        public ICollection<WorkItemDocument> Documents { get; set; } = new List<WorkItemDocument>();
        public ICollection<WorkItemMaterial> Materials { get; set; } = new List<WorkItemMaterial>();
        public ICollection<WorkItemComment> Comments { get; set; } = new List<WorkItemComment>();
        public ICollection<WorkItemUpdateHistory> UpdateHistory { get; set; } = new List<WorkItemUpdateHistory>();
    }
}
