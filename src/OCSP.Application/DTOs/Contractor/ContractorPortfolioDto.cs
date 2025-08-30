 using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Contractor
{
 public class ContractorPortfolioDto
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? ProjectDescription { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? ProjectValue { get; set; }
        public DateTime CompletedDate { get; set; }
        public string? ClientTestimonial { get; set; }
        public int DisplayOrder { get; set; }
    }}