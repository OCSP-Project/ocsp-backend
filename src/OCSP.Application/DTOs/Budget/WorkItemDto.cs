namespace OCSP.Application.DTOs.Budget
{
    public class WorkItemDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }

        // Basic Information
        public string Code { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Tree Structure
        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int Level { get; set; }
        public int SortOrder { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool HasChildren { get; set; }
        public List<WorkItemDto>? Children { get; set; }

        // Timeline
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public int PlannedDuration { get; set; }
        public int? ActualDuration { get; set; }

        // Progress
        public decimal Progress { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;           // #4CAF50, #F44336, #2196F3

        // Quantity and Financial
        public decimal? PlannedQuantity { get; set; }
        public string? Unit { get; set; }
        public decimal? ActualQuantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? CompactionFactor { get; set; }

        // Manpower
        public List<AssigneeDto> Assignees { get; set; } = new();
        public ResponsiblePersonDto? ResponsiblePerson { get; set; }

        // Warnings
        public List<string> Warnings { get; set; } = new();
        public bool IsOverdue { get; set; }
        public int? DaysOverdue { get; set; }

        // Display state
        public bool IsExpanded { get; set; }
        public bool IsHidden { get; set; }

        // Statistics
        public int CommentCount { get; set; }
        public int DocumentCount { get; set; }
        public int MaterialCount { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedByUsername { get; set; }
        public string? UpdatedByUsername { get; set; }
        public UserBasicDto? CreatedBy { get; set; }
        public UserBasicDto? UpdatedBy { get; set; }
    }

    public class UserBasicDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
    }

    public class AssigneeDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Position { get; set; }
    }

    public class ResponsiblePersonDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
