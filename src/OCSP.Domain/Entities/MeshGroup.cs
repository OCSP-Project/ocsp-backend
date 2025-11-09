using OCSP.Domain.Common;
using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{
    /// <summary>
    /// Groups of meshes categorized by component type
    /// </summary>
    public class MeshGroup : AuditableEntity
    {
        // Model reference
        public Guid ModelId { get; set; }
        public Project3DModel? Model { get; set; }

        // Component classification
        public ComponentType ComponentType { get; set; }
        public string MeshIndicesJson { get; set; } = "[]"; // JSONB in DB

        // Visual properties
        public string Color { get; set; } = "#CCCCCC"; // Hex color

        // Volume and unit
        public decimal VolumeM3 { get; set; }
        public string Unit { get; set; } = "m3";

        // Detection metadata
        public bool IsAutoDetected { get; set; }
        public string? DetectionAlgorithm { get; set; }
    }
}

