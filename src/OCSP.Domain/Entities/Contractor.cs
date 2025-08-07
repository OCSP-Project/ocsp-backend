using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class Contractor : AuditableEntity
    {
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;

        // Navigation properties
        public Guid UserId { get; set; }
        public User? User { get; set; }

        public ICollection<Contract>? Contracts { get; set; }
    }
}