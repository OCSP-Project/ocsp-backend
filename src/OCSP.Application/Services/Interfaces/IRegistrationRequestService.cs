// OCSP.Application/Services/Interfaces/IRegistrationRequestService.cs
using OCSP.Application.DTOs.RegistrationRequest;

namespace OCSP.Application.Services.Interfaces
{
    public interface IRegistrationRequestService
    {
        Task<RegistrationRequestDto> SubmitAsync(SubmitRegistrationRequestDto dto);
        Task<List<RegistrationRequestDto>> GetAllAsync();
        Task<RegistrationRequestDto?> GetByIdAsync(Guid id);
        Task<RegistrationRequestDto> ApproveAsync(Guid id, ApproveRegistrationRequestDto dto, Guid adminUserId);
        Task<RegistrationRequestDto> RejectAsync(Guid id, RejectRegistrationRequestDto dto, Guid adminUserId);
    }
}


