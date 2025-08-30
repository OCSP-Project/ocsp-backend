 using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Contractor
{
  public class ReviewSummaryDto
    {
        public Guid Id { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProjectName { get; set; }
    }}