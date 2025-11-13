namespace OCSP.Application.DTOs.Budget
{
    public class WorkItemDetailDto : WorkItemDto
    {
        // Additional detailed information
        public string? Notes { get; set; }
        public string? ImportData { get; set; }
        public DateTime? ImportedAt { get; set; }

        // Children list
        public List<WorkItemDto> Children { get; set; } = new();

        // Prerequisites
        public List<PrerequisiteWorkItemDto> Prerequisites { get; set; } = new();

        // Materials
        public List<WorkItemMaterialDto> Materials { get; set; } = new();

        // Documents
        public List<WorkItemDocumentDto> Documents { get; set; } = new();

        // Comments
        public List<WorkItemCommentDto> Comments { get; set; } = new();

        // Activity Log
        public List<WorkItemActivityDto> Activities { get; set; } = new();

        // Update History
        public List<WorkItemUpdateHistoryDto> UpdateHistory { get; set; } = new();
    }

    public class PrerequisiteWorkItemDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? PlannedEndDate { get; set; }
    }

    public class WorkItemMaterialDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal PlannedQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Supplier { get; set; }
    }

    public class WorkItemDocumentDto
    {
        public Guid Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long FileSize { get; set; }
        public string FileSizeFormatted { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string UploadedByUsername { get; set; } = string.Empty;
    }

    public class WorkItemCommentDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid CreatedById { get; set; }
        public string CreatedByUsername { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ParentCommentId { get; set; }
        public List<WorkItemCommentDto> Replies { get; set; } = new();
        public List<string> Attachments { get; set; } = new();
    }

    public class WorkItemActivityDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PerformedByUsername { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Notes { get; set; }
    }

    public class WorkItemUpdateHistoryDto
    {
        public Guid Id { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string UpdatedByUsername { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string? Reason { get; set; }
    }
}
