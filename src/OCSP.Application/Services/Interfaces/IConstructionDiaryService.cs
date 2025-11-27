using OCSP.Application.DTOs.ConstructionDiary;

namespace OCSP.Application.Services.Interfaces
{
    public interface IConstructionDiaryService
    {
        // Get operations
        Task<ConstructionDiaryDetailDto?> GetDiaryByDateAsync(Guid projectId, DateTime date, CancellationToken ct = default);
        Task<List<ConstructionDiarySummaryDto>> GetDiariesByMonthAsync(Guid projectId, int year, int month, CancellationToken ct = default);
        Task<List<ConstructionDiarySummaryDto>> GetAllDiariesByProjectAsync(Guid projectId, CancellationToken ct = default);

        // Create/Update operations
        Task<ConstructionDiaryDetailDto> CreateDiaryAsync(CreateConstructionDiaryDto dto, Guid userId, CancellationToken ct = default);
        Task<ConstructionDiaryDetailDto> UpdateDiaryAsync(Guid diaryId, UpdateConstructionDiaryDto dto, Guid userId, CancellationToken ct = default);

        // Delete operation
        Task DeleteDiaryAsync(Guid diaryId, Guid userId, CancellationToken ct = default);
    }
}
