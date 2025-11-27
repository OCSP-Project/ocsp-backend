using Microsoft.AspNetCore.Http;

namespace OCSP.Application.DTOs.Budget
{
    public class UpdateWorkItemDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public int? PlannedDuration { get; set; }

        public decimal? Progress { get; set; }
        public string? Status { get; set; }

        public decimal? ActualQuantity { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }

        public List<Guid>? AssigneeIds { get; set; }
        public Guid? ResponsiblePersonId { get; set; }

        public string? Notes { get; set; }
        public string? Reason { get; set; }                              // For audit trail
    }

    public class UpdateProgressDto
    {
        public decimal Progress { get; set; }                            // 0-100
        public decimal? ActualQuantity { get; set; }
        public string? Notes { get; set; }
        public List<string>? ProofPhotos { get; set; }                   // Image URLs
    }

    public class AddCommentDto
    {
        public string Content { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
        public List<IFormFile>? Attachments { get; set; }
    }
}
