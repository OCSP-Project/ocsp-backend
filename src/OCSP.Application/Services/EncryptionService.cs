using System.Security.Cryptography;
using OCSP.Application.Services.Interfaces;
using System.IO;

namespace OCSP.Application.Services
{
    public class EncryptionService : IEncryptionService
    {
        // Trong production, dùng Azure Key Vault hoặc AWS KMS
        // Đây là demo đơn giản
        private readonly Dictionary<string, byte[]> _keyStore = new();
        private readonly string _keysDir;

        public EncryptionService()
        {
            // Lưu key ra đĩa để không bị mất sau khi restart (chỉ demo/dev)
            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
            _keysDir = Path.Combine(uploads, "keys");
            if (!Directory.Exists(_keysDir)) Directory.CreateDirectory(_keysDir);
        }

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

            // Persist key to disk for later decryption (dev only)
            var keyPath = Path.Combine(_keysDir, keyId + ".bin");
            await File.WriteAllBytesAsync(keyPath, keyData);

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
            {
                // Try load from disk (dev persistence)
                var keyPath = Path.Combine(_keysDir, keyId + ".bin");
                if (File.Exists(keyPath))
                {
                    keyData = await File.ReadAllBytesAsync(keyPath);
                    _keyStore[keyId] = keyData;
                }
                else
                {
                    throw new InvalidOperationException("Encryption key not found");
                }
            }

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