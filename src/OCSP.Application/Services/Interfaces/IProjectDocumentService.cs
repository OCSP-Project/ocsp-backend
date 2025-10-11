using Microsoft.AspNetCore.Http;
using OCSP.Application.DTOs.Project;
namespace OCSP.Application.Services.Interfaces
{
    public interface IProjectDocumentService
    {
        /// <summary>
        /// Upload tài liệu dự án (bản vẽ hoặc giấy phép)
        /// - Drawing: Lưu file bình thường
        /// - Permit: Mã hóa + OCR trích xuất metadata
        /// </summary>
        Task<ProjectDocumentDto> UploadDocumentAsync(
            Guid projectId,
            Guid userId,
            IFormFile file,
            UploadProjectDocumentDto dto);

        /// <summary>
        /// Lấy tất cả documents của project
        /// </summary>
        Task<IEnumerable<ProjectDocumentDto>> GetProjectDocumentsAsync(Guid projectId);

        /// <summary>
        /// Lấy file để download (tự động giải mã nếu cần)
        /// </summary>
        Task<(Stream FileStream, string FileName, string ContentType)> GetDocumentFileAsync(
            Guid documentId,
            Guid userId);


        Task DeleteDocumentAsync(Guid documentId, Guid userId);

        /// <summary>
        /// Xác minh giấy phép (kiểm tra metadata)
        /// </summary>
        Task<VerificationResultDto> VerifyPermitAsync(Guid permitDocumentId);
    }
}