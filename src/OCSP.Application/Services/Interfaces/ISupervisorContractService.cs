using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OCSP.Application.DTOs.Contracts;

namespace OCSP.Application.Services.Interfaces
{
    public interface ISupervisorContractService
    {
        Task<SupervisorContractDto> CreateAsync(Guid projectId, Guid supervisorId, decimal monthlyPrice, CancellationToken ct = default);
        Task<SupervisorContractDto> CreateForProjectAsync(Guid projectId, Guid homeownerId, decimal monthlyPrice, CancellationToken ct = default);
        Task<SupervisorContractDto> GetByIdAsync(Guid contractId, Guid currentUserId, CancellationToken ct = default);
        Task<SupervisorContractDto?> GetByProjectIdAsync(Guid projectId, Guid currentUserId, CancellationToken ct = default);
        Task<IEnumerable<SupervisorContractListItemDto>> ListMyContractsAsync(Guid userId, CancellationToken ct = default);
        Task<SupervisorContractDto> SignByHomeownerAsync(Guid contractId, SignSupervisorContractDto dto, Guid homeownerId, CancellationToken ct = default);
        Task<SupervisorContractDto> SignBySupervisorAsync(Guid contractId, SignSupervisorContractDto dto, Guid supervisorId, CancellationToken ct = default);
        Task<byte[]> GeneratePdfAsync(Guid contractId, Guid userId, CancellationToken ct = default);
        Task<SupervisorContractDto> GeneratePdfForContractAsync(Guid contractId, Guid userId, CancellationToken ct = default);
    }
}
