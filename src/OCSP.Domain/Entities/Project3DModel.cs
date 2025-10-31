using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    /// <summary>
    /// 3D Model files (GLB format) uploaded for projects
    /// </summary>
    public class Project3DModel : AuditableEntity
    {
        // Project reference
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

        // File information
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public decimal FileSizeMB { get; set; }

        // Analysis results
        public int TotalMeshes { get; set; }
        public bool AnalysisCompleted { get; set; }
        public DateTime? AnalyzedAt { get; set; }
        public string? AnalysisResultJson { get; set; } // JSONB in DB

        // Navigation properties
        public ICollection<BuildingElement> BuildingElements { get; set; } = new List<BuildingElement>();
        public ICollection<MeshGroup> MeshGroups { get; set; } = new List<MeshGroup>();
    }
}

