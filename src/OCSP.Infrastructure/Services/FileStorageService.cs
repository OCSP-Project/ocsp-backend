using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace OCSP.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _storagePath;
        private readonly string _baseUrl;

        public LocalFileStorageService(IConfiguration configuration)
        {
            _storagePath = configuration["FileStorage:LocalPath"] ?? Path.Combine("wwwroot", "uploads", "models");
            _baseUrl = configuration["FileStorage:BaseUrl"] ?? "/uploads/models";

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName, string? fileName = null, CancellationToken cancellationToken = default)
        {
            fileName ??= $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var containerPath = Path.Combine(_storagePath, containerName);
            if (!Directory.Exists(containerPath)) Directory.CreateDirectory(containerPath);
            var filePath = Path.Combine(containerPath, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, cancellationToken);
            var url = _baseUrl.TrimEnd('/') + "/" + containerName + "/" + fileName;
            return url;
        }

        public Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var relative = fileUrl.Replace(_baseUrl.TrimEnd('/') + "/", "").Replace("/", Path.DirectorySeparatorChar.ToString());
                var filePath = Path.Combine(_storagePath, relative);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<string> GetSignedUrlAsync(string fileUrl, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(fileUrl);
        }
    }
}
