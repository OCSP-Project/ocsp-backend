using OCSP.Application.DTOs.Project;

namespace OCSP.Application.Services.Interfaces
{
    public interface IProjectService
    {
        // Create Project
        Task<ProjectDetailDto> CreateProjectAsync(CreateProjectDto dto, Guid homeownerId);
        
        // Get Project
        Task<ProjectDetailDto?> GetProjectByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<ProjectResponseDto>> GetProjectsByHomeownerAsync(Guid homeownerId, CancellationToken ct = default);
        
        // Update Project
        Task<ProjectDetailDto> UpdateProjectAsync(Guid projectId, UpdateProjectDto dto, Guid homeownerId);
    }
}