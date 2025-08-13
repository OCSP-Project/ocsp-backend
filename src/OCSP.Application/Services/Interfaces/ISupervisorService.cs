using OCSP.Application.DTOs.Supervisor;

namespace OCSP.Application.Services.Interfaces
{
    public interface ISupervisorService
    {
        Task<List<SupervisorListDto>> GetAllAsync();
        Task<SupervisorDetailDto?> GetByIdAsync(Guid id);
    }
}