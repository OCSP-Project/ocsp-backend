using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class Supervisor : AuditableEntity
    {
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Navigation properties
        public Guid UserId { get; set; }
        public User? User { get; set; }

        public ICollection<Project>? Projects { get; set; }
    }
}