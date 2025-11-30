namespace OCSP.Application.DTOs.ConstructionDiary
{
    // Enums matching frontend
    public enum ConstructionRatingDto
    {
        Good = 0,
        Average = 1,
        Poor = 2
    }

    public enum ImageCategoryDto
    {
        Construction = 0,
        Incident = 1,
        Material = 2
    }

    // Base DTOs for responses
    public class ConstructionDiaryDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public DateTime DiaryDate { get; set; }
        public string? ConstructionTeam { get; set; }

        // Assessment ratings
        public ConstructionRatingDto SafetyRating { get; set; }
        public ConstructionRatingDto QualityRating { get; set; }
        public ConstructionRatingDto ProgressRating { get; set; }
        public ConstructionRatingDto CleanlinessRating { get; set; }

        // Reports
        public string? IncidentReport { get; set; }
        public string? Recommendations { get; set; }
        public string? Notes { get; set; }

        // Supervisor info
        public string? SupervisorName { get; set; }
        public string? SupervisorPosition { get; set; }
        public string? ContractorName { get; set; }
        public string? SupervisorUnitName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    // Detailed DTO with all nested data
    public class ConstructionDiaryDetailDto : ConstructionDiaryDto
    {
        public List<DiaryWorkItemDto> WorkItems { get; set; } = new();
        public List<DiaryMaterialEntryDto> MaterialEntries { get; set; } = new();
        public List<DiaryWeatherPeriodDto> WeatherPeriods { get; set; } = new();
        public List<DiaryImageDto> Images { get; set; } = new();
    }

    // Work Item DTOs
    public class DiaryWorkItemDto
    {
        public Guid Id { get; set; }
        public Guid ConstructionDiaryId { get; set; }
        public Guid WorkItemId { get; set; }
        public string WorkItemName { get; set; } = string.Empty;
        public string? ConstructionArea { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal ConstructedQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public string Unit { get; set; } = string.Empty;

        public List<DiaryLaborDto> LaborEntries { get; set; } = new();
        public List<DiaryEquipmentDto> EquipmentEntries { get; set; } = new();
    }

    // Labor DTOs
    public class DiaryLaborDto
    {
        public Guid Id { get; set; }
        public Guid DiaryWorkItemId { get; set; }
        public Guid? LaborId { get; set; }
        public string LaborName { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string WorkHours { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    // Equipment DTOs
    public class DiaryEquipmentDto
    {
        public Guid Id { get; set; }
        public Guid DiaryWorkItemId { get; set; }
        public Guid? EquipmentId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public string Specifications { get; set; } = string.Empty;
        public decimal HoursUsed { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    // Material Entry DTOs
    public class DiaryMaterialEntryDto
    {
        public Guid Id { get; set; }
        public Guid ConstructionDiaryId { get; set; }
        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal ContractQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal? Variance { get; set; }
    }

    // Weather DTOs
    public class DiaryWeatherPeriodDto
    {
        public Guid Id { get; set; }
        public Guid ConstructionDiaryId { get; set; }
        public string Period { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string? Temperature { get; set; }
    }

    // Image DTOs
    public class DiaryImageDto
    {
        public Guid Id { get; set; }
        public Guid ConstructionDiaryId { get; set; }
        public string Url { get; set; } = string.Empty;
        public ImageCategoryDto Category { get; set; }
        public string? Description { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    // Create/Update DTOs
    public class CreateConstructionDiaryDto
    {
        public Guid ProjectId { get; set; }
        public DateTime DiaryDate { get; set; }
        public string? ConstructionTeam { get; set; }

        public ConstructionRatingDto SafetyRating { get; set; }
        public ConstructionRatingDto QualityRating { get; set; }
        public ConstructionRatingDto ProgressRating { get; set; }
        public ConstructionRatingDto CleanlinessRating { get; set; }

        public string? IncidentReport { get; set; }
        public string? Recommendations { get; set; }
        public string? Notes { get; set; }

        public string? SupervisorName { get; set; }
        public string? SupervisorPosition { get; set; }
        public string? ContractorName { get; set; }
        public string? SupervisorUnitName { get; set; }

        public List<CreateDiaryWorkItemDto> WorkItems { get; set; } = new();
        public List<CreateDiaryMaterialEntryDto> MaterialEntries { get; set; } = new();
        public List<CreateDiaryWeatherPeriodDto> WeatherPeriods { get; set; } = new();
        public List<CreateDiaryImageDto> Images { get; set; } = new();
    }

    public class UpdateConstructionDiaryDto
    {
        public string? ConstructionTeam { get; set; }

        public ConstructionRatingDto SafetyRating { get; set; }
        public ConstructionRatingDto QualityRating { get; set; }
        public ConstructionRatingDto ProgressRating { get; set; }
        public ConstructionRatingDto CleanlinessRating { get; set; }

        public string? IncidentReport { get; set; }
        public string? Recommendations { get; set; }
        public string? Notes { get; set; }

        public string? SupervisorName { get; set; }
        public string? SupervisorPosition { get; set; }
        public string? ContractorName { get; set; }
        public string? SupervisorUnitName { get; set; }

        public List<CreateDiaryWorkItemDto> WorkItems { get; set; } = new();
        public List<CreateDiaryMaterialEntryDto> MaterialEntries { get; set; } = new();
        public List<CreateDiaryWeatherPeriodDto> WeatherPeriods { get; set; } = new();
        public List<CreateDiaryImageDto> Images { get; set; } = new();
    }

    // Create DTOs for nested entities
    public class CreateDiaryWorkItemDto
    {
        public Guid WorkItemId { get; set; }
        public string? ConstructionArea { get; set; }
        public decimal ConstructedQuantity { get; set; }

        public List<CreateDiaryLaborDto> LaborEntries { get; set; } = new();
        public List<CreateDiaryEquipmentDto> EquipmentEntries { get; set; } = new();
    }

    public class CreateDiaryLaborDto
    {
        public Guid? LaborId { get; set; }
        public string LaborName { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string WorkHours { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public class CreateDiaryEquipmentDto
    {
        public Guid? EquipmentId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public string Specifications { get; set; } = string.Empty;
        public decimal HoursUsed { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public class CreateDiaryMaterialEntryDto
    {
        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal ContractQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal? Variance { get; set; }
    }

    public class CreateDiaryWeatherPeriodDto
    {
        public string Period { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string? Temperature { get; set; }
    }

    public class CreateDiaryImageDto
    {
        public string Url { get; set; } = string.Empty;
        public ImageCategoryDto Category { get; set; }
        public string? Description { get; set; }
    }

    // Calendar/List DTOs
    public class ConstructionDiarySummaryDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public DateTime DiaryDate { get; set; }
        public string? ConstructionTeam { get; set; }
        public int WorkItemCount { get; set; }
        public int ImageCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    // Query DTOs
    public class GetDiaryByDateQuery
    {
        public Guid ProjectId { get; set; }
        public DateTime Date { get; set; }
    }

    public class GetDiariesByMonthQuery
    {
        public Guid ProjectId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
