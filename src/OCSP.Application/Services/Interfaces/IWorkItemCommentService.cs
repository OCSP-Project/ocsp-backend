using OCSP.Application.DTOs.Budget;

namespace OCSP.Application.Services.Interfaces
{
    public interface IWorkItemCommentService
    {
        Task<WorkItemCommentDto> CreateAsync(CreateWorkItemCommentDto dto, Guid userId, CancellationToken ct = default);
        Task<WorkItemCommentDto> UpdateAsync(Guid commentId, UpdateWorkItemCommentDto dto, Guid userId, CancellationToken ct = default);
        Task DeleteAsync(Guid commentId, Guid userId, CancellationToken ct = default);
        Task<List<WorkItemCommentDto>> GetByWorkItemIdAsync(Guid workItemId, CancellationToken ct = default);
        Task<WorkItemCommentDto?> GetByIdAsync(Guid commentId, CancellationToken ct = default);
    }
}
