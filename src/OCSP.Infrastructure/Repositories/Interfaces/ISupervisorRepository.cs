
using OCSP.Domain.Entities;
namespace OCSP.Infrastructure.Repositories.Interfaces
{
    public interface ISupervisorRepository
    {
        Task<Supervisor?> GetByIdAsync(Guid id, bool includeUser = false);
        Task<bool> ExistsAsync(Guid id);
    }
}