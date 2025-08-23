namespace OCSP.Application.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder);
        Task DeleteFileAsync(string fileUrl);
        Task<byte[]> GetFileAsync(string fileUrl);
    }
}