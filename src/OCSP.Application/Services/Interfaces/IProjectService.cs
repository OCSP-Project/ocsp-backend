using OCSP.Application.DTOs.Project;

namespace OCSP.Application.Services.Interfaces
{
    public interface IProjectService
    {
        // Create Project
        Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto createDto, Guid homeownerId);
        
    }
}