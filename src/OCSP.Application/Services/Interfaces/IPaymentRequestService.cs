using OCSP.Application.DTOs.Budget;
using Microsoft.AspNetCore.Http;

namespace OCSP.Application.Services.Interfaces
{
    public interface IPaymentRequestService
    {
        // Get operations
        Task<List<PaymentRequestDto>> GetAllByProjectAsync(Guid projectId, CancellationToken ct = default);
        Task<PaymentRequestDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

        // CRUD operations
        Task<PaymentRequestDto> CreateAsync(CreatePaymentRequestDto dto, Guid currentUserId, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);

        // Workflow operations
        Task<PaymentRequestDto> ApproveAsync(Guid id, Guid approverId, ApprovePaymentRequestDto dto, CancellationToken ct = default);
        Task<PaymentRequestDto> RejectAsync(Guid id, Guid approverId, RejectPaymentRequestDto dto, CancellationToken ct = default);
        Task<PaymentRequestDto> MarkAsPaidAsync(Guid id, Guid currentUserId, CancellationToken ct = default);
        Task<PaymentRequestDto> CancelAsync(Guid id, Guid currentUserId, CancellationToken ct = default);

        // Statistics
        Task<PaymentStatisticsDto> GetStatisticsAsync(Guid projectId, CancellationToken ct = default);

        // Document operations
        Task<byte[]> DownloadDocumentAsync(Guid id, CancellationToken ct = default);
    }
}
