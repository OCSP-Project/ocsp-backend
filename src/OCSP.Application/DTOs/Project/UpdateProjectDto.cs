using OCSP.Domain.Entities;

namespace OCSP.Application.DTOs.Project
{ 
    public record UpdateProjectDto(
        string? Name,
        string? Description,
        string? Address,
        decimal? FloorArea,
        int? NumberOfFloors,
        decimal? Budget,
        DateTime? StartDate,
        DateTime? EstimatedCompletionDate,
        ProjectStatus? Status
    ); 
}