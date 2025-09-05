using OCSP.Application.DTOs.Proposals;

namespace OCSP.Application.Services.Interfaces
{
    public interface IProposalService
    {
        // contractor tạo proposal (Draft)
        Task<ProposalDto> CreateAsync(CreateProposalDto dto, Guid contractorUserId, CancellationToken ct = default);

        // contractor submit proposal
        Task SubmitAsync(Guid proposalId, Guid contractorUserId, CancellationToken ct = default);

        // homeowner xem danh sách proposal theo quote
        Task<IEnumerable<ProposalDto>> ListByQuoteAsync(Guid quoteId, Guid homeownerId, CancellationToken ct = default);

        // homeowner accept 1 proposal (reject các proposal còn lại, đóng quote)
        Task AcceptAsync(Guid proposalId, Guid homeownerId, CancellationToken ct = default);

        // contractor: lấy/sửa draft của chính mình
        Task<ProposalDto> GetMyByIdAsync(Guid id, Guid contractorUserId, CancellationToken ct = default);
        Task<ProposalDto?> GetMyByQuoteAsync(Guid quoteId, Guid contractorUserId, CancellationToken ct = default);
        Task<ProposalDto> UpdateDraftAsync(Guid id, UpdateProposalDto dto, Guid contractorUserId, CancellationToken ct = default);
    }
}
