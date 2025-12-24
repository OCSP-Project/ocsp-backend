using OCSP.Domain.Common;
using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{
    /// <summary>
    /// Individual building components parsed from 3D model
    /// </summary>
    public class BuildingElement : AuditableEntity
    {
        // Override audit fields to use Guid instead of string (migrated from old schema)
        public new Guid? CreatedBy { get; set; }
        public new Guid? UpdatedBy { get; set; }

        // Model reference
        public Guid ModelId { get; set; }
        public Project3DModel? Model { get; set; }

        // Element identification
        public string Name { get; set; } = string.Empty;
        public ComponentType ElementType { get; set; }

        // Spatial dimensions
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public decimal Height { get; set; }
        public decimal CenterX { get; set; }
        public decimal CenterY { get; set; }
        public decimal CenterZ { get; set; }
        public decimal VolumeM3 { get; set; }

        // Metadata
        public int FloorLevel { get; set; }
        public TrackingStatus TrackingStatus { get; set; } = TrackingStatus.NotStarted;
        public int CompletionPercentage { get; set; } = 0;  // 0-100%
        public bool CanTrack { get; set; }

        // Visual customization
        public string Color { get; set; } = "#CCCCCC"; // Hex color for 3D visualization (default: gray)

        // Mesh data (JSONB in DB) - User selected mesh indices
        public string MeshIndices { get; set; } = "[]";  // JSON array: [0,1,2,3,...]

        // Navigation properties
        public ICollection<ElementTrackingHistory> TrackingHistory { get; set; } = new List<ElementTrackingHistory>();
    }
}

