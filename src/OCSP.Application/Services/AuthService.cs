using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OCSP.Application.Common.Exceptions;
using OCSP.Application.Common.Helpers;
using OCSP.Application.DTOs.Auth;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.ExternalServices.Interfaces;
using Microsoft.Extensions.Configuration;
namespace OCSP.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IMapper mapper, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<TokenDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);
            if (user == null || !PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new ValidationException("Email hoặc mật khẩu không đúng");
            }

            if (!user.IsEmailVerified)
            {
                throw new ValidationException("Vui lòng xác thực email trước khi đăng nhập");
            }

            var jwtToken = JwtHelper.GenerateJwtToken(
                user.Id.ToString(),
                user.Email,
                user.Role.ToString(),
                _configuration["Jwt:SecretKey"] ?? "your-secret-key"
            );

            var refreshToken = JwtHelper.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return new TokenDto
            {
                AccessToken = jwtToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = _mapper.Map<UserResponseDto>(user)
            };
        }

        public async Task<UserResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                throw new ValidationException("Mật khẩu xác nhận không khớp");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == registerDto.Email);
            if (existingUser != null)
            {
                throw new ValidationException("Email đã được sử dụng");
            }

            var existingUsername = await _context.Users.FirstOrDefaultAsync(x => x.Username == registerDto.Username);
            if (existingUsername != null)
            {
                throw new ValidationException("Tên người dùng đã được sử dụng");
            }

            var verificationToken = PasswordHelper.GenerateRandomCode(6);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = PasswordHelper.HashPassword(registerDto.Password),
                Role = registerDto.Role,
                IsEmailVerified = false,
                EmailVerificationToken = verificationToken,
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(15)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _emailService.SendVerificationEmailAsync(user.Email, verificationToken);

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == forgotPasswordDto.Email);
            if (user == null)
            {
                // Không tiết lộ thông tin email có tồn tại hay không
                return true;
            }

            var resetToken = PasswordHelper.GenerateRandomCode(6);
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            {
                throw new ValidationException("Mật khẩu xác nhận không khớp");
            }

            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.Email == resetPasswordDto.Email &&
                x.PasswordResetToken == resetPasswordDto.Token &&
                x.PasswordResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                throw new ValidationException("Mã đặt lại mật khẩu không hợp lệ hoặc đã hết hạn");
            }

            user.PasswordHash = PasswordHelper.HashPassword(resetPasswordDto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.RefreshToken = null; // Vô hiệu hóa tất cả refresh token

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.Email == verifyEmailDto.Email &&
                x.EmailVerificationToken == verifyEmailDto.Token &&
                x.EmailVerificationTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                throw new ValidationException("Mã xác thực không hợp lệ hoặc đã hết hạn");
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<TokenDto> RefreshTokenAsync(string refreshToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.RefreshToken == refreshToken &&
                x.RefreshTokenExpiryTime > DateTime.UtcNow);

            if (user == null)
            {
                throw new ValidationException("Refresh token không hợp lệ");
            }

            var newJwtToken = JwtHelper.GenerateJwtToken(
                user.Id.ToString(),
                user.Email,
                user.Role.ToString(),
                _configuration["Jwt:SecretKey"] ?? "your-secret-key"
            );

            var newRefreshToken = JwtHelper.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return new TokenDto
            {
                AccessToken = newJwtToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = _mapper.Map<UserResponseDto>(user)
            };
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
            if (user == null)
            {
                return false;
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
