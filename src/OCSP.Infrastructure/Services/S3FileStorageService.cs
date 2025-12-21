using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace OCSP.Infrastructure.Services
{
    public class S3FileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _region;
        private readonly IConfiguration _configuration;

        public S3FileStorageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _configuration = configuration;
            _bucketName = configuration["AWS:BucketName"]
                ?? throw new InvalidOperationException("AWS:BucketName is not configured");
            _region = configuration["AWS:Region"] ?? "ap-southeast-1";
        }

        public async Task<string> UploadFileAsync(
            IFormFile file,
            string containerName,
            string? fileName = null,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            // Generate unique filename
            fileName ??= $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

            // Construct S3 key (path in bucket)
            var fileKey = $"{containerName}/{fileName}";

            try
            {
                using var stream = file.OpenReadStream();

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    Key = fileKey,
                    BucketName = _bucketName,
                    CannedACL = S3CannedACL.PublicRead, // Public files - accessible via direct URL
                    ContentType = GetContentType(file.FileName)
                };

                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(uploadRequest, cancellationToken);

                // Return the public S3 URL
                var publicUrl = $"https://{_bucketName}.s3.{_region}.amazonaws.com/{fileKey}";
                return publicUrl;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to upload file to S3: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return false;

            try
            {
                // Extract S3 key from URL if it's a full URL
                var key = ExtractS3KeyFromUrl(fileUrl);

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                // Log the error (consider using ILogger)
                Console.WriteLine($"Failed to delete file from S3: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetSignedUrlAsync(
            string fileUrl,
            TimeSpan expiration,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                throw new ArgumentException("File URL cannot be empty");

            try
            {
                // Extract S3 key from URL if it's a full URL
                var key = ExtractS3KeyFromUrl(fileUrl);

                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    Expires = DateTime.UtcNow.Add(expiration),
                    Verb = HttpVerb.GET
                };

                // Note: GetPreSignedURL is synchronous, but we wrap it in Task for interface compatibility
                var signedUrl = await Task.FromResult(_s3Client.GetPreSignedURL(request));
                return signedUrl;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate signed URL: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> GetFileAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                throw new ArgumentException("File URL cannot be empty");

            try
            {
                // Extract S3 key from URL if it's a full URL
                var key = ExtractS3KeyFromUrl(fileUrl);

                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
                using var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
                return memoryStream.ToArray();
            }
            catch (Amazon.S3.AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new FileNotFoundException($"File not found in S3: {fileUrl}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get file from S3: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extract S3 key from URL or return as-is if already a key
        /// Example: https://bucket.s3.region.amazonaws.com/folder/file.png -> folder/file.png
        /// </summary>
        private string ExtractS3KeyFromUrl(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return fileUrl;

            // If it's already a key (not a URL), return as-is
            if (!fileUrl.StartsWith("http://") && !fileUrl.StartsWith("https://"))
                return fileUrl;

            try
            {
                // Parse URL: https://bucket.s3.region.amazonaws.com/folder/file.png
                var uri = new Uri(fileUrl);

                // Extract key from path (remove leading /)
                var key = uri.AbsolutePath.TrimStart('/');
                return key;
            }
            catch
            {
                // If parsing fails, assume it's already a key
                return fileUrl;
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
