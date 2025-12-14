// OCSP.Application/Services/RegistrationRequestService.cs
using Microsoft.EntityFrameworkCore;
using OCSP.Application.Common.Exceptions;
using OCSP.Application.Common.Helpers;
using OCSP.Application.DTOs.RegistrationRequest;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.ExternalServices.Interfaces;

namespace OCSP.Application.Services
{
    public class RegistrationRequestService : IRegistrationRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public RegistrationRequestService(
            ApplicationDbContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<RegistrationRequestDto> SubmitAsync(SubmitRegistrationRequestDto dto)
        {
            // Validate role
            if (dto.RequestedRole != UserRole.Supervisor && dto.RequestedRole != UserRole.Contractor)
            {
                throw new ValidationException("Chỉ có thể đăng ký cho vai trò Giám sát viên hoặc Nhà thầu");
            }

            // Normalize email before query (EF Core can't translate ToLowerInvariant to SQL)
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            // Validate email unique
            var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);
            if (existingUser != null)
            {
                throw new ValidationException("Email đã được sử dụng");
            }

            // Validate username unique
            var existingUsername = await _context.Users.FirstOrDefaultAsync(x => x.Username == dto.Username);
            if (existingUsername != null)
            {
                throw new ValidationException("Tên người dùng đã được sử dụng");
            }

            // Check if there's already a pending request for this email
            var existingRequest = await _context.RegistrationRequests
                .FirstOrDefaultAsync(x => x.Email == normalizedEmail && x.Status == Domain.Entities.RegistrationRequestStatus.Pending);
            if (existingRequest != null)
            {
                throw new ValidationException("Đã có yêu cầu đăng ký đang chờ xử lý cho email này");
            }

            // Validate role-specific fields
            if (dto.RequestedRole == UserRole.Supervisor)
            {
                if (string.IsNullOrWhiteSpace(dto.Department))
                    throw new ValidationException("Phòng ban là bắt buộc cho giám sát viên");
                if (string.IsNullOrWhiteSpace(dto.Position))
                    throw new ValidationException("Chức vụ là bắt buộc cho giám sát viên");
            }
            else if (dto.RequestedRole == UserRole.Contractor)
            {
                if (string.IsNullOrWhiteSpace(dto.CompanyName))
                    throw new ValidationException("Tên công ty là bắt buộc cho nhà thầu");
            }

            var request = new RegistrationRequest
            {
                Id = Guid.NewGuid(),
                Username = dto.Username.Trim(),
                Email = normalizedEmail, // Use already normalized email
                Phone = dto.Phone.Trim(),
                RequestedRole = dto.RequestedRole,
                Status = Domain.Entities.RegistrationRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                // Supervisor fields
                Department = dto.Department,
                Position = dto.Position,
                District = dto.District,
                MinRate = dto.MinRate,
                MaxRate = dto.MaxRate,
                // Contractor fields
                CompanyName = dto.CompanyName,
                BusinessLicense = dto.BusinessLicense,
                TaxCode = dto.TaxCode,
                Description = dto.Description,
                Website = dto.Website,
                Address = dto.Address,
                City = dto.City,
                Province = dto.Province,
                YearsOfExperience = dto.YearsOfExperience,
                TeamSize = dto.TeamSize,
                CompletedProjects = dto.CompletedProjects,
                MinProjectBudget = dto.MinProjectBudget,
                MaxProjectBudget = dto.MaxProjectBudget
            };

            _context.RegistrationRequests.Add(request);
            await _context.SaveChangesAsync();

            return MapToDto(request);
        }

        public async Task<List<RegistrationRequestDto>> GetAllAsync()
        {
            var requests = await _context.RegistrationRequests
                .Include(r => r.ReviewedByUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests.Select(MapToDto).ToList();
        }

        public async Task<RegistrationRequestDto?> GetByIdAsync(Guid id)
        {
            var request = await _context.RegistrationRequests
                .Include(r => r.ReviewedByUser)
                .FirstOrDefaultAsync(r => r.Id == id);

            return request == null ? null : MapToDto(request);
        }

        public async Task<RegistrationRequestDto> ApproveAsync(Guid id, ApproveRegistrationRequestDto dto, Guid adminUserId)
        {
            var request = await _context.RegistrationRequests
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                throw new ValidationException("Yêu cầu đăng ký không tồn tại");

            if (request.Status != Domain.Entities.RegistrationRequestStatus.Pending)
                throw new ValidationException("Yêu cầu này đã được xử lý");

            // Validate password
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            {
                throw new ValidationException("Mật khẩu phải có ít nhất 6 ký tự");
            }

            // Check if email/username still available
            var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (existingUser != null)
            {
                throw new ValidationException("Email đã được sử dụng");
            }

            var existingUsername = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.Username);
            if (existingUsername != null)
            {
                throw new ValidationException("Tên người dùng đã được sử dụng");
            }

            // Create User
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                Role = request.RequestedRole,
                IsEmailVerified = dto.SkipEmailVerification,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (!dto.SkipEmailVerification)
            {
                var verificationToken = PasswordHelper.GenerateRandomCode(6);
                user.EmailVerificationToken = verificationToken;
                user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddDays(7);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create Supervisor or Contractor based on role
            if (request.RequestedRole == UserRole.Supervisor)
            {
                var supervisor = new Supervisor
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Department = request.Department ?? string.Empty,
                    Position = request.Position ?? string.Empty,
                    Phone = request.Phone,
                    District = request.District,
                    MinRate = request.MinRate,
                    MaxRate = request.MaxRate,
                    AvailableNow = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Supervisors.Add(supervisor);
            }
            else if (request.RequestedRole == UserRole.Contractor)
            {
                var contractor = new Contractor
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    CompanyName = request.CompanyName ?? string.Empty,
                    BusinessLicense = request.BusinessLicense ?? string.Empty, // Will be updated by user later
                    TaxCode = request.TaxCode ?? string.Empty,
                    Description = request.Description ?? string.Empty,
                    Website = request.Website ?? string.Empty,
                    ContactPhone = request.Phone,
                    ContactEmail = request.Email,
                    Address = request.Address,
                    City = request.City ?? "Da Nang",
                    Province = request.Province ?? "Da Nang",
                    YearsOfExperience = request.YearsOfExperience ?? 0,
                    TeamSize = request.TeamSize ?? 1,
                    CompletedProjects = request.CompletedProjects ?? 0,
                    MinProjectBudget = request.MinProjectBudget,
                    MaxProjectBudget = request.MaxProjectBudget,
                    IsActive = true,
                    IsVerified = true, // Verified by admin when approving registration request
                    VerifiedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Contractors.Add(contractor);
            }

            // Update request status
            request.Status = Domain.Entities.RegistrationRequestStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByUserId = adminUserId;
            request.CreatedUserId = user.Id;
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send email with account information
            try
            {
                var emailSubject = "Tài khoản của bạn đã được tạo thành công - OCSP";
                var emailBody = $@"
                    <h2>Chào mừng đến với OCSP!</h2>
                    <p>Tài khoản của bạn đã được tạo thành công.</p>
                    <p><strong>Thông tin đăng nhập:</strong></p>
                    <ul>
                        <li>Email: {user.Email}</li>
                        <li>Tên người dùng: {user.Username}</li>
                        <li>Mật khẩu: {dto.Password}</li>
                    </ul>
                    <p>Vui lòng đăng nhập và đổi mật khẩu ngay sau khi đăng nhập lần đầu.</p>
                    {(dto.SkipEmailVerification ? "" : "<p>Vui lòng xác thực email của bạn để sử dụng đầy đủ các tính năng.</p>")}
                    <p>Trân trọng,<br/>Đội ngũ OCSP</p>
                ";

                await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                Console.WriteLine($"Failed to send account creation email: {ex.Message}");
            }

            return MapToDto(request);
        }

        public async Task<RegistrationRequestDto> RejectAsync(Guid id, RejectRegistrationRequestDto dto, Guid adminUserId)
        {
            var request = await _context.RegistrationRequests
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                throw new ValidationException("Yêu cầu đăng ký không tồn tại");

            if (request.Status != Domain.Entities.RegistrationRequestStatus.Pending)
                throw new ValidationException("Yêu cầu này đã được xử lý");

            request.Status = Domain.Entities.RegistrationRequestStatus.Rejected;
            request.RejectionReason = dto.RejectionReason;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByUserId = adminUserId;
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send rejection email
            try
            {
                var emailSubject = "Thông báo về yêu cầu đăng ký tài khoản - OCSP";
                var emailBody = $@"
                    <div style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
                        <h2 style=""color: #1890ff;"">Kính gửi Quý khách,</h2>
                        <p>Cảm ơn Quý khách đã quan tâm và gửi yêu cầu đăng ký tài khoản {(request.RequestedRole == UserRole.Supervisor ? "Giám sát viên" : "Nhà thầu")} trên nền tảng OCSP.</p>
                        <p>Sau khi xem xét kỹ lưỡng hồ sơ đăng ký của Quý khách, chúng tôi rất tiếc phải thông báo rằng yêu cầu của Quý khách hiện tại chưa đáp ứng đủ các tiêu chí của chúng tôi.</p>
                        <div style=""background-color: #fff7e6; border-left: 4px solid #faad14; padding: 15px; margin: 20px 0;"">
                            <p style=""margin: 0; font-weight: bold; color: #d46b08;"">Lý do cụ thể:</p>
                            <p style=""margin: 10px 0 0 0;"">{dto.RejectionReason}</p>
                        </div>
                        <p>Chúng tôi hiểu rằng thông tin này có thể không như Quý khách mong đợi. Tuy nhiên, chúng tôi luôn sẵn sàng hỗ trợ và tư vấn để Quý khách có thể cải thiện hồ sơ và đăng ký lại trong tương lai.</p>
                        <p>Nếu Quý khách có bất kỳ thắc mắc nào hoặc muốn được tư vấn thêm về quy trình đăng ký, vui lòng liên hệ với chúng tôi qua email hoặc hotline. Chúng tôi sẽ rất vui được hỗ trợ Quý khách.</p>
                        <p>Một lần nữa, chúng tôi xin chân thành cảm ơn sự quan tâm của Quý khách dành cho OCSP.</p>
                        <p style=""margin-top: 30px;"">
                            Trân trọng,<br/>
                            <strong>Đội ngũ OCSP</strong><br/>
                            <span style=""color: #888; font-size: 0.9em;"">Nền tảng quản lý dự án xây dựng</span>
                        </p>
                    </div>
                ";

                await _emailService.SendEmailAsync(request.Email, emailSubject, emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send rejection email: {ex.Message}");
            }

            return MapToDto(request);
        }

        private RegistrationRequestDto MapToDto(RegistrationRequest request)
        {
            return new RegistrationRequestDto
            {
                Id = request.Id,
                Username = request.Username,
                Email = request.Email,
                Phone = request.Phone,
                RequestedRole = request.RequestedRole,
                Status = (DTOs.RegistrationRequest.RegistrationRequestStatus)request.Status,
                RejectionReason = request.RejectionReason,
                ReviewedAt = request.ReviewedAt,
                ReviewedByUserId = request.ReviewedByUserId,
                ReviewedByUsername = request.ReviewedByUser?.Username,
                CreatedAt = request.CreatedAt,
                CreatedUserId = request.CreatedUserId,
                Department = request.Department,
                Position = request.Position,
                District = request.District,
                MinRate = request.MinRate,
                MaxRate = request.MaxRate,
                CompanyName = request.CompanyName,
                BusinessLicense = request.BusinessLicense,
                TaxCode = request.TaxCode,
                Description = request.Description,
                Website = request.Website,
                Address = request.Address,
                City = request.City,
                Province = request.Province,
                YearsOfExperience = request.YearsOfExperience,
                TeamSize = request.TeamSize,
                CompletedProjects = request.CompletedProjects,
                MinProjectBudget = request.MinProjectBudget,
                MaxProjectBudget = request.MaxProjectBudget
            };
        }
    }
}

