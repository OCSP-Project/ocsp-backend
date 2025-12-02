using OCSP.Application.Services.Interfaces;
using OCSP.Application.Helpers;
using OCSP.Infrastructure.Services;
using System.IO;
using System.Net.Http;

namespace OCSP.Application.Services
{
    public class FileService : IFileService
    {
        private readonly IFileStorageService _fileStorageService;

        public FileService(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder)
        {
            try
            {
                // Ensure stream is at the beginning
                if (fileStream.CanSeek)
                    fileStream.Position = 0;

                // Determine content type from file extension
                var contentType = GetContentType(fileName);

                // Wrap the stream as IFormFile
                var formFile = new StreamFormFile(fileStream, fileName, contentType);

                // Use the storage service to upload (handles both local and S3)
                var fileUrl = await _fileStorageService.UploadFileAsync(
                    formFile,
                    folder,
                    fileName);

                return fileUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi upload file: {ex.Message}", ex);
            }
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return;

                await _fileStorageService.DeleteFileAsync(fileUrl);
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw exception để tránh ảnh hưởng đến flow chính
                Console.WriteLine($"Lỗi khi xóa file: {ex.Message}");
            }
        }

        public async Task<byte[]> GetFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    throw new ArgumentException("File URL không được để trống");

                // If it's an HTTP/HTTPS URL (external file or signed URL from S3)
                if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var absoluteUri) &&
                    (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
                {
                    using var http = new HttpClient();
                    return await http.GetByteArrayAsync(absoluteUri);
                }

                // For S3: Generate signed URL and download
                // For Local: fileUrl is already a path we can serve directly
                // Since we're using the storage service abstraction, we need to handle this differently

                // Generate a signed URL (for S3 this creates a temporary URL, for local it returns the same path)
                var signedUrl = await _fileStorageService.GetSignedUrlAsync(
                    fileUrl,
                    TimeSpan.FromMinutes(5));

                // Download from the signed URL
                if (Uri.TryCreate(signedUrl, UriKind.Absolute, out var signedUri) &&
                    (signedUri.Scheme == Uri.UriSchemeHttp || signedUri.Scheme == Uri.UriSchemeHttps))
                {
                    using var http = new HttpClient();
                    return await http.GetByteArrayAsync(signedUri);
                }

                // If it's a local path (starts with /uploads/), read directly
                if (signedUrl.StartsWith("/uploads/"))
                {
                    var relativePath = signedUrl.TrimStart('/');
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

                    if (!File.Exists(fullPath))
                        throw new FileNotFoundException("File không tồn tại");

                    return await File.ReadAllBytesAsync(fullPath);
                }

                throw new InvalidOperationException($"Không thể đọc file từ: {fileUrl}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi đọc file: {ex.Message}", ex);
            }
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                // Images
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",

                // Documents
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",

                // 3D Models
                ".glb" => "model/gltf-binary",
                ".gltf" => "model/gltf+json",
                ".obj" => "model/obj",
                ".fbx" => "application/octet-stream",

                // Archives
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",

                // Video
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",

                // Default
                _ => "application/octet-stream"
            };
        }
    }
}
