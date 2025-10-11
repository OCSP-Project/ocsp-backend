using OCSP.Application.DTOs.ProgressMedia;
using Microsoft.AspNetCore.Http;

namespace OCSP.Application.Services.Interfaces
{
    public interface IProgressMediaService
    {
        Task<ProgressMediaDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ProgressMediaListDto> GetByProjectIdAsync(Guid projectId, Guid? taskId, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 20, CancellationToken ct = default);
        Task<ProgressMediaDto> UploadAsync(Guid projectId, IFormFile file, string caption, Guid? taskId, Guid? progressUpdateId, Guid userId, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
        Task<bool> IsUserAuthorizedForProjectAsync(Guid projectId, Guid userId, CancellationToken ct = default);
    }
}
