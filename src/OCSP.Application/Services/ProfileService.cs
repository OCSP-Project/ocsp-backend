using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OCSP.Application.Common.Exceptions;
using OCSP.Application.DTOs.Profile;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;
using System.IO;

namespace OCSP.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;

        public ProfileService(ApplicationDbContext context, IMapper mapper, IFileService fileService)
        {
            _context = context;
            _mapper = mapper;
            _fileService = fileService;
        }

        // View Profile - Tất cả roles có thể xem profile của mình
        public async Task<ProfileDto> GetProfileAsync(Guid userId)
        {
            var profile = await _context.Profiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                // Tạo profile mới nếu chưa có
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    throw new ValidationException("Người dùng không tồn tại");

                profile = new OCSP.Domain.Entities.Profile
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Profiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            var profileDto = _mapper.Map<ProfileDto>(profile);
            profileDto.Username = profile.User.Username;
            profileDto.Email = profile.User.Email;
            profileDto.Role = (int)profile.User.Role;
            profileDto.IsEmailVerified = profile.User.IsEmailVerified;

            return profileDto;
        }

        // Edit Profile - Tất cả roles có thể chỉnh sửa profile của mình
        public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto updateDto)
        {
            var profile = await _context.Profiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                throw new ValidationException("Profile không tồn tại");

            // Cập nhật thông tin profile
            if (!string.IsNullOrEmpty(updateDto.FirstName))
                profile.FirstName = updateDto.FirstName;
            
            if (!string.IsNullOrEmpty(updateDto.LastName))
                profile.LastName = updateDto.LastName;
            
            if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                profile.PhoneNumber = updateDto.PhoneNumber;
            
            if (!string.IsNullOrEmpty(updateDto.Address))
                profile.Address = updateDto.Address;
            
            if (!string.IsNullOrEmpty(updateDto.City))
                profile.City = updateDto.City;
            
            if (!string.IsNullOrEmpty(updateDto.State))
                profile.State = updateDto.State;
            
            if (!string.IsNullOrEmpty(updateDto.Country))
                profile.Country = updateDto.Country;
            
            if (!string.IsNullOrEmpty(updateDto.Bio))
                profile.Bio = updateDto.Bio;

            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updatedProfile = _mapper.Map<ProfileDto>(profile);
            updatedProfile.Username = profile.User.Username;
            updatedProfile.Email = profile.User.Email;
            updatedProfile.Role = (int)profile.User.Role;
            updatedProfile.IsEmailVerified = profile.User.IsEmailVerified;

            return updatedProfile;
        }

        // Upload Profile Documents - Chỉ Contractor và Supervisor
        public async Task<ProfileDocumentDto> UploadProfileDocumentAsync(Guid userId, Stream fileStream, string fileName, long fileSize, UploadProfileDocumentDto documentDto)
        {
            var profile = await _context.Profiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                throw new ValidationException("Profile không tồn tại");

            // Kiểm tra quyền: Chỉ Contractor và Supervisor mới được upload documents
            if (profile.User.Role != UserRole.Contractor && profile.User.Role != UserRole.Supervisor)
                throw new ValidationException("Chỉ nhà thầu và giám sát viên mới được tải lên tài liệu");

            if (fileStream == null || fileSize == 0)
                throw new ValidationException("File không được để trống");

            // Kiểm tra kích thước file (giới hạn 10MB)
            if (fileSize > 10 * 1024 * 1024)
                throw new ValidationException("File quá lớn. Kích thước tối đa là 10MB");

            // Kiểm tra loại file được phép
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
                throw new ValidationException("Loại file không được hỗ trợ. Chỉ chấp nhận: PDF, DOC, DOCX, JPG, JPEG, PNG");

            // Upload file
            var fileUrl = await _fileService.UploadFileAsync(fileStream, fileName, "profile-documents");

            // Lưu thông tin document vào database
            var profileDocument = new OCSP.Domain.Entities.ProfileDocument
            {
                Id = Guid.NewGuid(),
                ProfileId = profile.Id,
                FileName = fileName,
                FileUrl = fileUrl,
                FileType = fileExtension,
                FileSize = fileSize,
                Description = documentDto.Description,
                DocumentType = documentDto.DocumentType,
                UploadedAt = DateTime.UtcNow
            };

            _context.ProfileDocuments.Add(profileDocument);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<ProfileDocumentDto>(profileDocument);
            return resultDto;
        }

        // Lấy danh sách documents của user
        public async Task<IEnumerable<ProfileDocumentDto>> GetUserDocumentsAsync(Guid userId)
        {
            var profile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return new List<ProfileDocumentDto>();

            var documents = await _context.ProfileDocuments
                .Where(d => d.ProfileId == profile.Id)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            var documentDtos = _mapper.Map<IEnumerable<ProfileDocumentDto>>(documents);
            return documentDtos;
        }

        // Xóa document
        public async Task DeleteProfileDocumentAsync(Guid userId, Guid documentId)
        {
            var profile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                throw new ValidationException("Profile không tồn tại");

            var document = await _context.ProfileDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.ProfileId == profile.Id);

            if (document == null)
                throw new ValidationException("Tài liệu không tồn tại hoặc bạn không có quyền xóa");

            // Xóa file từ storage
            await _fileService.DeleteFileAsync(document.FileUrl);

            // Xóa record từ database
            _context.ProfileDocuments.Remove(document);
            await _context.SaveChangesAsync();
        }
    }
}
