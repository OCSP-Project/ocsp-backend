namespace OCSP.Application.DTOs.Project
{
    public record CreateProjectDto(
        string Name,
        string Description,
        string Address,
        decimal FloorArea,
        int NumberOfFloors,
        decimal Budget,
        DateTime StartDate,
        DateTime? EndDate,
        DateTime? EstimatedCompletionDate,
        string Status
    );
}