using Microsoft.AspNetCore.Http;

namespace OCSP.Application.Helpers
{
    /// <summary>
    /// Wrapper class to convert a Stream into an IFormFile
    /// Used to bridge between Stream-based file operations and IFormFile-based storage services
    /// </summary>
    public class StreamFormFile : IFormFile
    {
        private readonly Stream _stream;
        private readonly string _fileName;
        private readonly string _contentType;

        public StreamFormFile(Stream stream, string fileName, string contentType = "application/octet-stream")
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            _contentType = contentType;
        }

        public string ContentType => _contentType;

        public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{_fileName}\"";

        public IHeaderDictionary Headers => new HeaderDictionary();

        public long Length => _stream.Length;

        public string Name => "file";

        public string FileName => _fileName;

        public void CopyTo(Stream target)
        {
            _stream.CopyTo(target);
        }

        public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            await _stream.CopyToAsync(target, cancellationToken);
        }

        public Stream OpenReadStream()
        {
            return _stream;
        }
    }
}
