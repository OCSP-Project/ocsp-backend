using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;

namespace OCSP.Infrastructure.Services
{
    public interface IS3StorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder);
        Task<Stream> DownloadFileAsync(string fileKey);
        Task<bool> DeleteFileAsync(string fileKey);
        Task<string> GetPresignedUrlAsync(string fileKey, int expirationMinutes = 60);
    }

    public class S3StorageService : IS3StorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3StorageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:S3:BucketName"]
                ?? throw new ArgumentNullException("S3 Bucket name not configured");
        }

        /// <summary>
        /// Upload file lên S3
        /// </summary>
        /// <param name="fileStream">File stream</param>
        /// <param name="fileName">Tên file gốc</param>
        /// <param name="folder">Thư mục: images/profiles, documents/contracts, etc.</param>
        /// <returns>S3 key (đường dẫn file trong S3)</returns>
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder)
        {
            // Tạo unique file name để tránh trùng
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var fileKey = $"{folder}/{uniqueFileName}";

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = fileKey,
                BucketName = _bucketName,
                CannedACL = S3CannedACL.Private, // Không public
                ContentType = GetContentType(fileName)
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            return fileKey;
        }

        /// <summary>
        /// Download file từ S3
        /// </summary>
        public async Task<Stream> DownloadFileAsync(string fileKey)
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = fileKey
            };

            var response = await _s3Client.GetObjectAsync(request);
            return response.ResponseStream;
        }

        /// <summary>
        /// Xóa file từ S3
        /// </summary>
        public async Task<bool> DeleteFileAsync(string fileKey)
        {
            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileKey
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tạo signed URL để download file (có thời hạn)
        /// </summary>
        public async Task<string> GetPresignedUrlAsync(string fileKey, int expirationMinutes = 60)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileKey,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            return await Task.FromResult(_s3Client.GetPreSignedURL(request));
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".ifc" => "application/x-step",
                _ => "application/octet-stream"
            };
        }
    }
}