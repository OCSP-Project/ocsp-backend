using System.Security.Cryptography;
using OCSP.Application.Services.Interfaces;

namespace OCSP.Application.Services
{
    public class EncryptionService : IEncryptionService
    {
        // Trong production, dùng Azure Key Vault hoặc AWS KMS
        // Đây là demo đơn giản
        private readonly Dictionary<string, byte[]> _keyStore = new();

        public async Task<EncryptionResult> EncryptFileAsync(byte[] data)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();

            var keyId = Guid.NewGuid().ToString();

            // Lưu key + IV (trong thực tế phải lưu vào KMS)
            var keyData = new byte[aes.Key.Length + aes.IV.Length];
            Buffer.BlockCopy(aes.Key, 0, keyData, 0, aes.Key.Length);
            Buffer.BlockCopy(aes.IV, 0, keyData, aes.Key.Length, aes.IV.Length);
            _keyStore[keyId] = keyData;

            // Mã hóa
            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);

            await cs.WriteAsync(data, 0, data.Length);
            cs.FlushFinalBlock();

            return new EncryptionResult
            {
                EncryptedData = ms.ToArray(),
                KeyId = keyId
            };
        }

        public async Task<byte[]> DecryptFileAsync(byte[] encryptedData, string keyId)
        {
            if (!_keyStore.TryGetValue(keyId, out var keyData))
                throw new InvalidOperationException("Encryption key not found");

            using var aes = Aes.Create();
            aes.KeySize = 256;

            // Tách key và IV
            aes.Key = keyData.Take(32).ToArray();
            aes.IV = keyData.Skip(32).Take(16).ToArray();

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(encryptedData);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var output = new MemoryStream();

            await cs.CopyToAsync(output);
            return output.ToArray();
        }
    }
}