namespace OCSP.Application.DTOs.Profile
{
    public class ProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // User information
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public bool IsEmailVerified { get; set; }
    }

    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Bio { get; set; }
    }

    public class ProfileDocumentDto
    {
        public Guid Id { get; set; }
        public Guid ProfileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Description { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class UploadProfileDocumentDto
    {
        public string? Description { get; set; }
        public string DocumentType { get; set; } = string.Empty; // Business License, Certificate, Service Area, etc.
    }
}
