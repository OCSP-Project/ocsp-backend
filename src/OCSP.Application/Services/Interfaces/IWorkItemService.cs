using OCSP.Application.DTOs.Budget;
using Microsoft.AspNetCore.Http;

namespace OCSP.Application.Services.Interfaces
{
    public interface IWorkItemService
    {
        // Get operations
        Task<List<WorkItemDto>> GetAllByProjectAsync(Guid projectId, bool rootLevelOnly = false, bool includeChildren = true, CancellationToken ct = default);
        Task<WorkItemDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<WorkItemDto>> GetChildrenAsync(Guid parentId, CancellationToken ct = default);

        // CRUD operations
        Task<WorkItemDto> CreateAsync(CreateWorkItemDto dto, Guid currentUserId, CancellationToken ct = default);
        Task<WorkItemDto> UpdateAsync(Guid id, UpdateWorkItemDto dto, Guid currentUserId, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task HardDeleteAllByProjectAsync(Guid projectId, CancellationToken ct = default);

        // Progress operations
        Task<WorkItemDto> UpdateProgressAsync(Guid id, UpdateProgressDto dto, Guid currentUserId, CancellationToken ct = default);
        Task<WorkItemDto> UpdateStatusAsync(Guid id, string status, Guid currentUserId, CancellationToken ct = default);

        // User assignment operations
        Task<WorkItemDto> AssignUsersAsync(Guid id, List<Guid> userIds, Guid currentUserId, CancellationToken ct = default);
        Task<WorkItemDto> UnassignUserAsync(Guid id, Guid userId, Guid currentUserId, CancellationToken ct = default);

        // Comment operations
        Task<WorkItemCommentDto> AddCommentAsync(Guid workItemId, AddCommentDto dto, Guid currentUserId, CancellationToken ct = default);
        Task<List<WorkItemCommentDto>> GetCommentsAsync(Guid workItemId, CancellationToken ct = default);

        // Document operations
        Task<WorkItemDocumentDto> AddDocumentAsync(Guid workItemId, IFormFile file, string documentType, string? description, Guid currentUserId, CancellationToken ct = default);
        Task<List<WorkItemDocumentDto>> GetDocumentsAsync(Guid workItemId, CancellationToken ct = default);

        // Activity and history
        Task<List<WorkItemActivityDto>> GetActivitiesAsync(Guid workItemId, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
        Task<List<WorkItemUpdateHistoryDto>> GetUpdateHistoryAsync(Guid workItemId, CancellationToken ct = default);

        // Import/Export
        Task<ImportBudgetResponseDto> ImportFromExcelAsync(Guid projectId, IFormFile file, bool overwriteExisting, Guid currentUserId, CancellationToken ct = default);
        Task<byte[]> ExportToExcelAsync(Guid projectId, CancellationToken ct = default);

        // Gantt Chart
        Task<GanttChartDataDto> GetGanttChartDataAsync(Guid projectId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);

        // Status calculations
        Task UpdateWorkItemStatusesAsync(Guid projectId, CancellationToken ct = default);
    }
}
