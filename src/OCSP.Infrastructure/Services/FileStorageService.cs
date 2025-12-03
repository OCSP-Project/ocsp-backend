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
            
            // Handle subfolders in fileName (e.g., "proposals/{quoteId}/file.xlsx")
            var filePath = Path.Combine(containerPath, fileName);
            var directoryPath = Path.GetDirectoryName(filePath);
            
            // Create all subdirectories if they don't exist
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, cancellationToken);
            
            // Replace backslashes with forward slashes for URL
            var urlPath = fileName.Replace('\\', '/');
            var url = _baseUrl.TrimEnd('/') + "/" + containerName + "/" + urlPath;
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

        public Task<byte[]> GetFileAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                // Parse URL to get relative path
                // URL format: /uploads/models/{containerName}/{fileName}
                // Example: /uploads/models/proposals/{quoteId}/{fileName}
                if (string.IsNullOrEmpty(fileUrl))
                    throw new ArgumentException("File URL không được để trống");

                // If it's an absolute HTTP/HTTPS URL, download it
                if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var absoluteUri) &&
                    (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
                {
                    using var http = new HttpClient();
                    return http.GetByteArrayAsync(absoluteUri, cancellationToken);
                }

                // For local storage: convert URL to file path
                // Remove base URL prefix to get relative path
                var relativePath = fileUrl;
                if (fileUrl.StartsWith(_baseUrl))
                {
                    relativePath = fileUrl.Substring(_baseUrl.Length).TrimStart('/');
                }
                else if (fileUrl.StartsWith("/uploads/"))
                {
                    // Handle legacy URLs that start with /uploads/
                    relativePath = fileUrl.Substring("/uploads/".Length);
                }

                // Convert forward slashes to platform-specific directory separator
                relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(_storagePath, relativePath);

                if (!File.Exists(fullPath))
                    throw new FileNotFoundException($"File không tồn tại: {fullPath}");

                return File.ReadAllBytesAsync(fullPath, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi đọc file: {ex.Message}", ex);
            }
        }
    }
}
