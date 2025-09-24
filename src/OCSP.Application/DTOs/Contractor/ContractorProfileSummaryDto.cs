using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Contractor
{
    public class ContractorProfileSummaryDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int CompletedProjects { get; set; }
        public int YearsOfExperience { get; set; }
        public string? City { get; set; }
        public string? ContactPhone { get; set; }
        public decimal? MinProjectBudget { get; set; }
        public decimal? MaxProjectBudget { get; set; }
        public bool IsVerified { get; set; }
        public bool IsPremium { get; set; }
        public List<string> Specialties { get; set; } = new List<string>();
        public string? FeaturedImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProfileCompletionPercentage { get; set; }
    }
}

