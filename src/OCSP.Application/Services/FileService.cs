using OCSP.Application.Services.Interfaces;
using System.IO;

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
                // Tạo thư mục con nếu chưa tồn tại
                var folderPath = Path.Combine(_uploadPath, folder);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Tạo tên file unique
                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                var filePath = Path.Combine(folderPath, uniqueFileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(stream);
                }

                // Trả về đường dẫn tương đối
                return $"/uploads/{folder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi upload file: {ex.Message}");
            }
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return;

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
        }

        public async Task<byte[]> GetFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    throw new ArgumentException("File URL không được để trống");

                // Chuyển đổi URL thành đường dẫn file system
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
