// OCSP.Application/Services/Interfaces/IEscrowService.cs
using OCSP.Application.DTOs.Escrow;
using OCSP.Application.DTOs.Payments;

namespace OCSP.Application.Services.Interfaces
{
    public interface IEscrowService
    {
        Task<EscrowAccountDto> SetupEscrowAsync(SetupEscrowDto dto, Guid homeownerId, CancellationToken ct = default);
        Task<PaymentTransactionDto> FundMilestoneAsync(FundMilestoneDto dto, Guid homeownerId, CancellationToken ct = default);
        Task<MilestonePayoutResultDto> ApproveMilestoneAsync(Guid milestoneId, Guid homeownerId, CancellationToken ct = default);
        Task<MilestonePayoutResultDto> ReleaseMilestoneAsync(ReleaseMilestoneDto dto, Guid homeownerId, CancellationToken ct = default);

        Task<EscrowAccountDto> GetEscrowByContractAsync(Guid contractId, Guid userId, CancellationToken ct = default);
        Task<List<PaymentTransactionDto>> ListTransactionsByContractAsync(Guid contractId, Guid userId, CancellationToken ct = default);
    }
}
