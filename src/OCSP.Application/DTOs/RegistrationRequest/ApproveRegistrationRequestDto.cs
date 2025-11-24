// OCSP.Application/DTOs/RegistrationRequest/ApproveRegistrationRequestDto.cs
namespace OCSP.Application.DTOs.RegistrationRequest
{
    public class ApproveRegistrationRequestDto
    {
        public string Password { get; set; } = string.Empty;
        public bool SkipEmailVerification { get; set; } = false;
    }
}


