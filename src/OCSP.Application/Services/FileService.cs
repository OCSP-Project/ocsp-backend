using OCSP.Application.Services.Interfaces;
using System.IO;
using System.Net.Http;

namespace OCSP.Application.Services
{
    public class FileService : IFileService
    {
        private readonly string _uploadPath;

        public FileService()
        {
            // Tạo thư mục uploads trong project
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder)
        {
            try
            {
                // Hỗ trợ đường dẫn con trong fileName (ví dụ: projectId/drawings/abc.pdf)
                var safeFileName = Path.GetFileName(fileName);
                var subDirectory = Path.GetDirectoryName(fileName)?.Replace('\\', '/') ?? string.Empty;

                // Tạo thư mục gốc + thư mục con nếu chưa tồn tại
                var destinationDir = string.IsNullOrEmpty(subDirectory)
                    ? Path.Combine(_uploadPath, folder)
                    : Path.Combine(_uploadPath, folder, subDirectory);

                if (!Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }

                // Tạo tên file unique
                var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
                var filePath = Path.Combine(destinationDir, uniqueFileName);

                // Đảm bảo stream ở vị trí bắt đầu
                if (fileStream.CanSeek) fileStream.Position = 0;

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await fileStream.CopyToAsync(stream);
                }

                // Trả về đường dẫn tương đối (URL)
                var relativePath = string.IsNullOrEmpty(subDirectory)
                    ? $"/uploads/{folder}/{uniqueFileName}"
                    : $"/uploads/{folder}/{subDirectory}/{uniqueFileName}".Replace("\\", "/");

                return relativePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi upload file: {ex.Message}");
            }
        }

        public Task DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return Task.CompletedTask;

                // Chuyển đổi URL thành đường dẫn file system
                var relativePath = fileUrl.TrimStart('/');
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw exception để tránh ảnh hưởng đến flow chính
                Console.WriteLine($"Lỗi khi xóa file: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public async Task<byte[]> GetFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    throw new ArgumentException("File URL không được để trống");

                // Nếu là URL tuyệt đối (http/https) → tải qua HTTP
                if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var absoluteUri) &&
                    (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
                {
                    using var http = new HttpClient();
                    return await http.GetByteArrayAsync(absoluteUri);
                }

                // Ngược lại: coi như đường dẫn tương đối trong app (/uploads/...)
                var relativePath = fileUrl.TrimStart('/');
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

                if (!File.Exists(fullPath))
                    throw new FileNotFoundException("File không tồn tại");

                return await File.ReadAllBytesAsync(fullPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi đọc file: {ex.Message}");
            }
        }
    }
}
