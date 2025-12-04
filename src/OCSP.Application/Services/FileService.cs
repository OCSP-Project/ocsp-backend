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
                // Use the storage service to get the file
                // This handles both local and S3 storage correctly
                return await _fileStorageService.GetFileAsync(fileUrl);
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
