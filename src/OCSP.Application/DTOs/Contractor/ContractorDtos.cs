using System;
using System.Collections.Generic;
using OCSP.Domain.Enums;


namespace OCSP.Application.DTOs.Contractor
{
    public class BulkContractorRequestDto
    {
        public List<CreateContractorDto> Contractors { get; set; } = new List<CreateContractorDto>();
    }

    public class CreateContractorDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string BusinessLicense { get; set; } = string.Empty;
        public string? TaxCode { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public int YearsOfExperience { get; set; } = 0;
        public int TeamSize { get; set; } = 1;
        public decimal? MinProjectBudget { get; set; }
        public decimal? MaxProjectBudget { get; set; }
        public decimal? AverageRating { get; set; }
        public int? TotalReviews { get; set; }
        public int? CompletedProjects { get; set; }
        public int? OngoingProjects { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsPremium { get; set; }
        public List<string>? Specialties { get; set; }
    }

    public class BulkContractorResponseDto
    {
        public List<ContractorSummaryDto> CreatedContractors { get; set; } = new List<ContractorSummaryDto>();
        public List<string> Errors { get; set; } = new List<string>();
        public int SuccessfulCount { get; set; }
        public int FailedCount { get; set; }
    }
    public class AIRecommendationRequestDto
    {
        public string ProjectDescription { get; set; } = string.Empty;
        public decimal? Budget { get; set; }
        public string? PreferredLocation { get; set; }
        public List<string>? RequiredSpecialties { get; set; }
        public DateTime? PreferredStartDate { get; set; }
        public ProjectComplexity Complexity { get; set; } = ProjectComplexity.Medium;

    }

    public class CommunicationWarningDto
    {
        public string Message { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public int WarningLevel { get; set; }
        public bool RequiresAcknowledgment { get; set; } = false;
    }

    public class ContractorDocumentDto
    {
        public Guid Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsVerified { get; set; }
    }

    public class ContractorListResponseDto
    {
        public List<ContractorSummaryDto> Contractors { get; set; } = new List<ContractorSummaryDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

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
    }

    public class ContractorProfileDto
    {
        public Guid Id { get; set; }
        public Guid? OwnerUserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }

        public int YearsOfExperience { get; set; }
        public int TeamSize { get; set; }
        public decimal? MinProjectBudget { get; set; }
        public decimal? MaxProjectBudget { get; set; }

        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int CompletedProjects { get; set; }
        public int OngoingProjects { get; set; }

        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public bool IsPremium { get; set; }
        public int ProfileCompletionPercentage { get; set; }

        public List<ContractorSpecialtyDto> Specialties { get; set; } = new List<ContractorSpecialtyDto>();
        public List<ContractorDocumentDto> Documents { get; set; } = new List<ContractorDocumentDto>();
        public List<ContractorPortfolioDto> Portfolios { get; set; } = new List<ContractorPortfolioDto>();
        public List<ReviewSummaryDto> RecentReviews { get; set; } = new List<ReviewSummaryDto>();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

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

    public class ContractorRecommendationDto
    {
        public ContractorSummaryDto Contractor { get; set; } = null!;
        public decimal MatchScore { get; set; }
        public string MatchReason { get; set; } = string.Empty;
        public List<string> MatchingFactors { get; set; } = new List<string>();
    }

    public class ContractorSearchDto
    {
        public string? Query { get; set; }
        public string? Location { get; set; }
        public List<string>? Specialties { get; set; }
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
        public decimal? MinRating { get; set; }
        public int? MinYearsExperience { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsPremium { get; set; }
        public SearchSortBy SortBy { get; set; } = SearchSortBy.Relevance;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ContractorSpecialtyDto
    {
        public Guid Id { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ExperienceYears { get; set; }
    }

    public class ContractorSummaryDto
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

    public class ReviewSummaryDto
    {
        public Guid Id { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProjectName { get; set; }
    }
}
