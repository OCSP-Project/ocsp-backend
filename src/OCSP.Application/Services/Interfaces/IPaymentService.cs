using OCSP.Application.DTOs.Payments;

namespace OCSP.Application.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<MomoCreatePaymentResultDto> CreateMomoPaymentAsync(MomoCreatePaymentDto dto, Guid userId, CancellationToken ct = default);
        Task HandleMomoWebhookAsync(MomoWebhookDto payload, string rawBody, CancellationToken ct = default);
    }
}


