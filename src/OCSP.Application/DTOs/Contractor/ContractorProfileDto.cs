 using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Contractor
{
  public class ContractorProfileDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        
        // Business Information
        public int YearsOfExperience { get; set; }
        public int TeamSize { get; set; }
        public decimal? MinProjectBudget { get; set; }
        public decimal? MaxProjectBudget { get; set; }
        
        // Performance Metrics
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int CompletedProjects { get; set; }
        public int OngoingProjects { get; set; }
        
        // Status
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public bool IsPremium { get; set; }
        public int ProfileCompletionPercentage { get; set; }
        
        // Related Data
        public List<ContractorSpecialtyDto> Specialties { get; set; } = new List<ContractorSpecialtyDto>();
        public List<ContractorDocumentDto> Documents { get; set; } = new List<ContractorDocumentDto>();
        public List<ContractorPortfolioDto> Portfolios { get; set; } = new List<ContractorPortfolioDto>();
        public List<ReviewSummaryDto> RecentReviews { get; set; } = new List<ReviewSummaryDto>();
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }}