namespace OCSP.Application.Services.Interfaces
{
    public interface IEncryptionService
    {
        Task<EncryptionResult> EncryptFileAsync(byte[] data);
        Task<byte[]> DecryptFileAsync(byte[] encryptedData, string keyId);
    }

    public class EncryptionResult
    {
        public byte[] EncryptedData { get; set; } = Array.Empty<byte>();
        public string KeyId { get; set; } = string.Empty;
    }
}