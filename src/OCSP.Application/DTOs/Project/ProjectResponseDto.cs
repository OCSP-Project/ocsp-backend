namespace OCSP.Application.DTOs.Project
{
    public record ProjectResponseDto(
        Guid Id,
        string Name,
        string Description,
        string Address,
        decimal FloorArea,
        int NumberOfFloors,
        decimal Budget,
        decimal? ActualBudget,
        DateTime StartDate,
        DateTime? EndDate,
        DateTime? EstimatedCompletionDate,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        Guid? SupervisorId,
        string? SupervisorName,
        Guid HomeownerId,
        string? HomeownerName
    );
}