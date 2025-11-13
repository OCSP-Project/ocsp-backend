namespace OCSP.Application.DTOs.Budget
{
    public class CreateWorkItemDto
    {
        public Guid ProjectId { get; set; }
        public Guid? ParentId { get; set; }

        public string Code { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime? PlannedStartDate { get; set; }
        public int PlannedDuration { get; set; }

        public decimal? PlannedQuantity { get; set; }
        public string? Unit { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? CompactionFactor { get; set; }

        public List<Guid>? AssigneeIds { get; set; }
        public Guid? ResponsiblePersonId { get; set; }
        public List<Guid>? PrerequisiteIds { get; set; }

        public string? Notes { get; set; }
    }
}
