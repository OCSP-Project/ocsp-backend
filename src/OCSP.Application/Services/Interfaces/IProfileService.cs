using OCSP.Application.DTOs.Profile;

namespace OCSP.Application.Services.Interfaces
{
    public interface IProfileService
    {
        // View Profile - Tất cả roles có thể xem profile của mình
        Task<ProfileDto> GetProfileAsync(Guid userId);
        
        // Edit Profile - Tất cả roles có thể chỉnh sửa profile của mình
        Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto updateDto);
        
        // Upload Profile Documents - Chỉ Contractor và Supervisor
        Task<ProfileDocumentDto> UploadProfileDocumentAsync(Guid userId, Stream fileStream, string fileName, long fileSize, UploadProfileDocumentDto documentDto);
        
        // Lấy danh sách documents của user
        Task<IEnumerable<ProfileDocumentDto>> GetUserDocumentsAsync(Guid userId);
        
        // Xóa document
        Task DeleteProfileDocumentAsync(Guid userId, Guid documentId);
    }
}
