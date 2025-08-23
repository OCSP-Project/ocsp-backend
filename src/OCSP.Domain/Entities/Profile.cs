using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class Profile : AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // Foreign key to User
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        // Profile information
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        
        // Navigation properties
        public virtual ICollection<ProfileDocument> ProfileDocuments { get; set; } = new List<ProfileDocument>();
    }
}
