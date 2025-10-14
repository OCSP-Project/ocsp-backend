using Microsoft.AspNetCore.Http;
using OCSP.Application.DTOs.Project;

namespace OCSP.Application.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectDetailDto> CreateProjectWithFilesAsync(
            CreateProjectDto dto,
            IFormFile drawingFile,
            IFormFile permitFile,
            Guid homeownerId);

        // Get Project
        Task<ProjectDetailDto?> GetProjectByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<ProjectResponseDto>> GetProjectsByHomeownerAsync(Guid homeownerId, CancellationToken ct = default);

        // Update Project
        Task<ProjectDetailDto> UpdateProjectAsync(Guid projectId, UpdateProjectDto dto, Guid homeownerId);

        // Download Drawing for Contractor
        Task<(Stream FileStream, string FileName, string ContentType)> DownloadDrawingAsync(Guid projectId, Guid contractorId);
    }
}