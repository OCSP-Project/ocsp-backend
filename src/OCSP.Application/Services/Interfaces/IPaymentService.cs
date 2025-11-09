using OCSP.Application.DTOs.Payments;

namespace OCSP.Application.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<MomoCreatePaymentResultDto> CreateMomoPaymentAsync(MomoCreatePaymentDto dto, Guid userId, CancellationToken ct = default);
        Task HandleMomoWebhookAsync(MomoWebhookDto payload, string rawBody, CancellationToken ct = default);
        Task<decimal> GetWalletBalanceAsync(Guid userId, CancellationToken ct = default);
        Task<bool> IsCommissionPaidAsync(Guid userId, Guid contractId, CancellationToken ct = default);
        Task<bool> IsSupervisorPaymentPaidAsync(Guid userId, Guid projectId, CancellationToken ct = default);
    }
}


