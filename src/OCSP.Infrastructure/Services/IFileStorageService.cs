using Microsoft.AspNetCore.Http;

namespace OCSP.Infrastructure.Services
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(
            IFormFile file,
            string containerName,
            string? fileName = null,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteFileAsync(
            string fileUrl,
            CancellationToken cancellationToken = default);

        Task<string> GetSignedUrlAsync(
            string fileUrl,
            TimeSpan expiration,
            CancellationToken cancellationToken = default);
    }
}




