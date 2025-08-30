using OCSP.Domain.Common;
using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{
    public class Contractor : AuditableEntity
    {
        // Remove duplicate Id property since AuditableEntity already has it
        public Guid UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string BusinessLicense { get; set; } = string.Empty;
        public string? TaxCode { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; } = "Da Nang";
        public string? Province { get; set; } = "Da Nang";
        
        // Business Info
        public int YearsOfExperience { get; set; } = 0;
        public int TeamSize { get; set; } = 1;
        public decimal? MinProjectBudget { get; set; }
        public decimal? MaxProjectBudget { get; set; }
        
        // Ratings & Reviews
        public decimal AverageRating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public int CompletedProjects { get; set; } = 0;
        public int OngoingProjects { get; set; } = 0;
        
        // Verification Status
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsPremium { get; set; } = false;
        public DateTime? PremiumExpiryDate { get; set; }
        
        // Profile Completion
        public int ProfileCompletionPercentage { get; set; } = 0;
        
        // Anti-Circumvention Tracking
        public int WarningCount { get; set; } = 0;
        public DateTime? LastWarningDate { get; set; }
        public bool IsRestricted { get; set; } = false;
        public DateTime? RestrictionExpiryDate { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<ContractorSpecialty> Specialties { get; set; } = new List<ContractorSpecialty>();
        public ICollection<ContractorDocument> Documents { get; set; } = new List<ContractorDocument>();
        public ICollection<ContractorPortfolio> Portfolios { get; set; } = new List<ContractorPortfolio>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<Review> ReceivedReviews { get; set; } = new List<Review>();
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public ICollection<Communication> Communications { get; set; } = new List<Communication>();
    }

    public class ContractorSpecialty : BaseEntity
    {
        // Remove duplicate Id - BaseEntity already has it
        public Guid ContractorId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ExperienceYears { get; set; } = 0;
        
        public Contractor Contractor { get; set; } = null!;
    }

    public class ContractorDocument : BaseEntity
    {
        // Remove duplicate Id - BaseEntity already has it  
        public Guid ContractorId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsVerified { get; set; } = false;
        
        public Contractor Contractor { get; set; } = null!;
    }

    public class ContractorPortfolio : BaseEntity
    {
        // Remove duplicate Id - BaseEntity already has it
        public Guid ContractorId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? ProjectDescription { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? ProjectValue { get; set; }
        public DateTime CompletedDate { get; set; }
        public string? ClientTestimonial { get; set; }
        public int DisplayOrder { get; set; } = 0;
        
        public Contractor Contractor { get; set; } = null!;
    }

    public class Communication : AuditableEntity
    {
        // Remove duplicate Id - AuditableEntity already has it
        public Guid FromUserId { get; set; }
        public Guid ToUserId { get; set; }
        public Guid? ProjectId { get; set; }
        public string Content { get; set; } = string.Empty;
        public CommunicationType Type { get; set; }
        public bool ContainsContactInfo { get; set; } = false;
        public bool IsFlagged { get; set; } = false;
        public bool IsReviewed { get; set; } = false;
        public string? FlagReason { get; set; }
        
        public User FromUser { get; set; } = null!;
        public User ToUser { get; set; } = null!;
        public Project? Project { get; set; }
    }
}