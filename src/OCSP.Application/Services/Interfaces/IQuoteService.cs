// OCSP.Application/Services/Interfaces/IQuoteService.cs
using OCSP.Application.DTOs.Quotes;



namespace OCSP.Application.Services.Interfaces
{
    public interface IQuoteService
    {
        Task<QuoteRequestDto> CreateAsync(CreateQuoteRequestDto dto, Guid homeownerId, CancellationToken ct = default);
        Task AddInviteeAsync(Guid quoteId, Guid contractorUserId, Guid homeownerId, CancellationToken ct = default);
        Task SendAsync(Guid quoteId, Guid homeownerId, CancellationToken ct = default);
        Task SendToAllContractorsAsync(Guid quoteId, Guid homeownerId, CancellationToken ct = default);
        Task SendToContractorAsync(Guid quoteId, Guid contractorUserId, Guid homeownerId, CancellationToken ct = default);
        Task<IEnumerable<QuoteRequestDto>> ListByProjectAsync(Guid projectId, Guid userId, CancellationToken ct = default);
        Task<IEnumerable<QuoteRequestDto>> ListMyInvitesAsync(Guid contractorUserId, CancellationToken ct = default);
        Task<QuoteRequestDto> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
        Task<IEnumerable<QuoteRequestDetailDto>> ListMyInvitesDetailedAsync(Guid contractorUserId, CancellationToken ct = default);
        Task<QuoteRequestDetailDto> GetDetailForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    }
}
