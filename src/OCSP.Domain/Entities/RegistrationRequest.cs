// OCSP.Domain/Entities/RegistrationRequest.cs
using OCSP.Domain.Common;
using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{
    public class RegistrationRequest : AuditableEntity
    {
        // User information
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public UserRole RequestedRole { get; set; } // Supervisor (1) or Contractor (2)

        // Status
        public RegistrationRequestStatus Status { get; set; } = RegistrationRequestStatus.Pending;
        public string? RejectionReason { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public User? ReviewedByUser { get; set; }

        // Supervisor specific fields
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? District { get; set; }
        public decimal? MinRate { get; set; }
        public decimal? MaxRate { get; set; }

        // Contractor specific fields
        public string? CompanyName { get; set; }
        public string? BusinessLicense { get; set; }
        public string? TaxCode { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public int? YearsOfExperience { get; set; }
        public int? TeamSize { get; set; }
        public int? CompletedProjects { get; set; }
        public decimal? MinProjectBudget { get; set; }
        public decimal? MaxProjectBudget { get; set; }

        // Created user (after approval)
        public Guid? CreatedUserId { get; set; }
        public User? CreatedUser { get; set; }
    }

    public enum RegistrationRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}


