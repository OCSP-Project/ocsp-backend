using Microsoft.AspNetCore.Http;

namespace OCSP.Application.DTOs.ModelAnalysis
{
    public class UploadGLBRequest
    {
        public Guid ProjectId { get; set; }
        public IFormFile File { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class UploadGLBResponse
    {
        public Guid ModelId { get; set; }
        public Guid ProjectId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public decimal FileSizeMB { get; set; }
        public int TotalMeshes { get; set; }
        public bool IsValid { get; set; }
        public string? ValidationMessage { get; set; }
        public bool AnalysisCompleted { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class Project3DModelDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public decimal FileSizeMB { get; set; }
        public int TotalMeshes { get; set; }
        public bool AnalysisCompleted { get; set; }
        public DateTime? AnalyzedAt { get; set; }
        public ModelAnalysisResult? AnalysisResult { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class ModelAnalysisResult
    {
        public int EstimatedElements { get; set; }
        public int MeshGroups { get; set; }
        public bool HasTextures { get; set; }
        public string? Notes { get; set; }
    }

    public class GLBValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public int MeshCount { get; set; }
        public decimal FileSizeMB { get; set; }
        public bool HasTextures { get; set; }
        public string? GltfVersion { get; set; }
    }
}
