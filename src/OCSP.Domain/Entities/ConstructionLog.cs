using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class ConstructionLog : AuditableEntity
    {
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

        public DateTime Date { get; set; }
        public string? Weather { get; set; }                             // Weather
        public int? Temperature { get; set; }                            // Temperature (°C)

        public int WorkersOnSite { get; set; }                           // Number of workers
        public int? MachinesOnSite { get; set; }                         // Number of machines

        public string WorkDescription { get; set; } = string.Empty;      // Work performed
        public string? Issues { get; set; }                              // Issues encountered
        public string? Notes { get; set; }

        public Guid RecordedById { get; set; }
        public User? RecordedBy { get; set; }

        // Site photos
        public string? SitePhotos { get; set; }                          // JSON array of image URLs
    }
}
