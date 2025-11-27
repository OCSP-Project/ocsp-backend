using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Admin
{
    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool SkipEmailVerification { get; set; } = false; // Admin có thể bỏ qua email verification
    }
}
