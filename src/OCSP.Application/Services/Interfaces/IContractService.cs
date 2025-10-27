using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OCSP.Application.DTOs.Contracts;

namespace OCSP.Application.Services.Interfaces
{
    public interface IContractService
    {
        Task<ContractDetailDto> CreateFromProposalAsync(
            CreateContractDto dto, Guid homeownerId, CancellationToken ct = default);

        Task<ContractDetailDto> GetByIdAsync(Guid contractId, Guid currentUserId, CancellationToken ct = default);

        Task<IEnumerable<ContractListItemDto>> ListByProjectAsync(
            Guid projectId, Guid currentUserId, CancellationToken ct = default);

        Task<IEnumerable<ContractListItemDto>> ListMyContractsAsync(
            Guid currentUserId, CancellationToken ct = default);

        Task<ContractDto> UpdateStatusAsync(
            UpdateContractStatusDto dto, Guid currentUserId, CancellationToken ct = default);

        Task<ContractDetailDto> GeneratePdfForContractAsync(
            Guid contractId, Guid currentUserId, CancellationToken ct = default);

        Task<ContractDetailDto> SignByHomeownerAsync(
            Guid contractId, SignContractDto dto, Guid homeownerId, CancellationToken ct = default);

        Task<ContractDetailDto> SignByContractorAsync(
            Guid contractId, SignContractDto dto, Guid contractorId, CancellationToken ct = default);

        Task<byte[]> GetContractPdfAsync(Guid contractId, Guid currentUserId, CancellationToken ct = default);
    }
}
