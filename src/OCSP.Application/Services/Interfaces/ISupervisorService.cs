// OCSP.Application/Services/Interfaces/ISupervisorService.cs
using OCSP.Application.DTOs.Common;
using OCSP.Application.DTOs.Supervisor;

namespace OCSP.Application.Services.Interfaces
{
    public interface ISupervisorService
    {
        Task<List<SupervisorListItemDto>> GetAllAsync(CancellationToken ct);

        Task<PagedResult<SupervisorListItemDto>> FilterAsync(FilterSupervisorsRequest req, CancellationToken ct);
        Task<SupervisorDetailsDto?> GetByIdAsync(Guid id, CancellationToken ct);
    }
}
