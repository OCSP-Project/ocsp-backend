using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    /// <summary>
    /// Photo evidence for tracking records
    /// </summary>
    public class TrackingPhoto : AuditableEntity
    {
        // Tracking reference
        public Guid TrackingHistoryId { get; set; }
        public ElementTrackingHistory? TrackingHistory { get; set; }

        // Photo information
        public string PhotoUrl { get; set; } = string.Empty; // Cloud storage URL
        public string? Caption { get; set; }
        public decimal FileSizeMB { get; set; }

        // Metadata
        public string? FileType { get; set; } // e.g., "image/jpeg"
        public int? Width { get; set; }
        public int? Height { get; set; }

        // Timestamp
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}

