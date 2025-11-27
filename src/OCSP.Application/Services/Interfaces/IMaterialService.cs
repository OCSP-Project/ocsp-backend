using OCSP.Application.DTOs.Material;
using Microsoft.AspNetCore.Http;

namespace OCSP.Application.Services.Interfaces
{
    public interface IMaterialService
    {
        // Material Request operations
        Task<MaterialRequestDetailDto> CreateRequestAsync(Guid projectId, Guid contractorId, CancellationToken ct = default);
        Task<List<MaterialRequestDto>> GetAllRequestsAsync(Guid projectId, CancellationToken ct = default);
        Task<MaterialRequestDetailDto?> GetRequestByIdAsync(Guid requestId, CancellationToken ct = default);

        // Import materials from Excel
        Task<MaterialRequestDetailDto> ImportMaterialsFromExcelAsync(Guid requestId, IFormFile file, CancellationToken ct = default);

        // Approval operations
        Task<MaterialRequestDetailDto> ApproveByHomeownerAsync(Guid requestId, Guid homeownerId, ApproveMaterialRequestDto dto, CancellationToken ct = default);
        Task<MaterialRequestDetailDto> ApproveBySupervisorAsync(Guid requestId, Guid supervisorId, ApproveMaterialRequestDto dto, CancellationToken ct = default);
        Task<MaterialRequestDetailDto> RejectRequestAsync(Guid requestId, Guid userId, RejectMaterialRequestDto dto, CancellationToken ct = default);
        Task DeleteRequestAsync(Guid requestId, Guid userId, CancellationToken ct = default);
        Task ClearImportedMaterialsAsync(Guid requestId, Guid userId, CancellationToken ct = default);

        // Material operations
        Task<List<MaterialDto>> GetMaterialsByProjectAsync(Guid projectId, CancellationToken ct = default);
        Task<MaterialDetailDto?> GetMaterialByIdAsync(Guid materialId, CancellationToken ct = default);
        Task<MaterialDto> UpdateMaterialAsync(Guid materialId, UpdateMaterialDto dto, CancellationToken ct = default);
        Task<MaterialDto> UpdateActualQuantityAsync(Guid materialId, UpdateActualQuantityDto dto, Guid supervisorId, CancellationToken ct = default);

        // Payment operations
        Task<MaterialPaymentDto> CreatePaymentAsync(CreateMaterialPaymentDto dto, Guid currentUserId, CancellationToken ct = default);
        Task<List<MaterialPaymentDto>> GetPaymentsByMaterialAsync(Guid materialId, CancellationToken ct = default);
        Task<List<MaterialPaymentDto>> GetPaymentsByProjectAsync(Guid projectId, CancellationToken ct = default);
    }
}
