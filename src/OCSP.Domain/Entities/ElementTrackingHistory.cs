using OCSP.Domain.Common;
using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{
    /// <summary>
    /// Daily tracking records for building elements
    /// Each day a supervisor/contractor updates progress creates a new record
    /// </summary>
    public class ElementTrackingHistory : AuditableEntity
    {
        // Element reference
        public Guid BuildingElementId { get; set; }
        public BuildingElement? BuildingElement { get; set; }

        // Tracking date
        public DateTime TrackingDate { get; set; }

        // Progress update
        public int PreviousPercentage { get; set; } // % before update
        public int NewPercentage { get; set; } // % after update
        public TrackingStatus? PreviousStatus { get; set; }
        public TrackingStatus NewStatus { get; set; }

        // Planned vs Actual
        public decimal? PlannedQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }

        // Materials used in this tracking
        public decimal? CementUsed { get; set; }
        public decimal? SandUsed { get; set; }
        public decimal? AggregateUsed { get; set; }

        // Evidence photos
        public ICollection<TrackingPhoto> Photos { get; set; } = new List<TrackingPhoto>();

        // Notes
        public string? Notes { get; set; }

        // Recorded by (Supervisor/Contractor)
        public Guid? RecordedById { get; set; }
        public User? RecordedBy { get; set; }
    }
}

