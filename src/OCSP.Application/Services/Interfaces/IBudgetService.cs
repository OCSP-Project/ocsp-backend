using OCSP.Application.DTOs.Budget;

namespace OCSP.Application.Services.Interfaces
{
    public interface IBudgetService
    {
        // Budget Detail operations
        Task<List<BudgetDetailDto>> GetAllByProjectAsync(Guid projectId, CancellationToken ct = default);
        Task<BudgetDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<BudgetDetailDto> CreateAsync(CreateBudgetDetailDto dto, Guid currentUserId, CancellationToken ct = default);
        Task<BudgetDetailDto> UpdateAsync(Guid id, UpdateBudgetDetailDto dto, Guid currentUserId, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);

        // Summary and Analytics
        Task<BudgetSummaryDto> GetSummaryAsync(Guid projectId, CancellationToken ct = default);
        Task<List<BudgetByCategoryDto>> GetByCategoryAsync(Guid projectId, CancellationToken ct = default);

        // Calculations
        Task RecalculateProjectBudgetAsync(Guid projectId, CancellationToken ct = default);
    }
}
