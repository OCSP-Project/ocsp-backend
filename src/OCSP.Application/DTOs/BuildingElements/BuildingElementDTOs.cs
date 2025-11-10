using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OCSP.Application.DTOs.BuildingElements
{
    public class CreateBuildingElementRequest
    {
        [Required]
        public Guid ModelId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [Required]
        public int ElementType { get; set; }
        public int FloorLevel { get; set; }
        [Required]
        public List<int> MeshIndices { get; set; } = new();
    }

    public class UpdateBuildingElementRequest
    {
        [MaxLength(200)]
        public string? Name { get; set; }
        public int? ElementType { get; set; }
        public int? FloorLevel { get; set; }
        public List<int>? MeshIndices { get; set; }
        [MaxLength(7)]
        public string? Color { get; set; } // Hex color (e.g., "#FF0000")
    }

    public class BuildingElementDto
    {
        public Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ElementType { get; set; }
        public int FloorLevel { get; set; }
        public string MeshIndicesJson { get; set; } = "[]";
        public int TrackingStatus { get; set; }
        public int CompletionPercentage { get; set; }
        public string Color { get; set; } = "#CCCCCC"; // Hex color for 3D visualization
        public DateTime CreatedAt { get; set; }
    }

    public class AddTrackingRecordRequest
    {
        [Required]
        public int NewStatus { get; set; }
        [Range(0, 100)]
        public int NewPercentage { get; set; }
        public decimal? PlannedQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }
        public decimal? CementUsed { get; set; }
        public decimal? SandUsed { get; set; }
        public decimal? AggregateUsed { get; set; }
        public string? Notes { get; set; }
    }

    public class TrackingHistoryDto
    {
        public Guid Id { get; set; }
        public Guid BuildingElementId { get; set; }
        public DateTime TrackingDate { get; set; }
        public int PreviousPercentage { get; set; }
        public int NewPercentage { get; set; }
        public int? PreviousStatus { get; set; }
        public int NewStatus { get; set; }
        public string? Notes { get; set; }
    }

    public class AddTrackingPhotoRequest
    {
        [Required]
        [MaxLength(2000)]
        public string PhotoUrl { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Caption { get; set; }
        public decimal FileSizeMB { get; set; }
        [MaxLength(50)]
        public string? FileType { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

    public class TrackingPhotoDto
    {
        public Guid Id { get; set; }
        public Guid TrackingHistoryId { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public decimal FileSizeMB { get; set; }
        public string? FileType { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class ModelSummaryDto
    {
        public int TotalElements { get; set; }
        public int NotStarted { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int OnHold { get; set; }
        public decimal AverageCompletion { get; set; }
    }
}
