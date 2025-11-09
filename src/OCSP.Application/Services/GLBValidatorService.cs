using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace OCSP.Application.Services.Interfaces
{
    public interface IGLBValidatorService
    {
        Task<OCSP.Application.DTOs.ModelAnalysis.GLBValidationResult> ValidateGLBFileAsync(
            IFormFile file,
            CancellationToken cancellationToken = default);
    }
}

namespace OCSP.Application.Services
{
    using OCSP.Application.DTOs.ModelAnalysis;
    using OCSP.Application.Services.Interfaces;

    public class GLBValidatorService : IGLBValidatorService
    {
        private const int HeaderSize = 12; // magic(4) + version(4) + length(4)
        private const int MinSizeBytes = 1024; // 1KB
        private const int MaxSizeBytes = 50 * 1024 * 1024; // 50MB
        private const uint GLB_MAGIC = 0x46546C67; // "glTF"
        private const uint JSON_CHUNK_TYPE = 0x4E4F534A; // "JSON"

        public async Task<GLBValidationResult> ValidateGLBFileAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                return new GLBValidationResult { IsValid = false, ErrorMessage = "File is empty" };
            }

            var fileSizeMB = Math.Round((decimal)file.Length / (1024 * 1024), 2);

            if (!file.FileName.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
            {
                return new GLBValidationResult { IsValid = false, ErrorMessage = "File must have .glb extension", FileSizeMB = fileSizeMB };
            }

            if (file.Length < MinSizeBytes)
            {
                return new GLBValidationResult { IsValid = false, ErrorMessage = $"File too small (minimum {MinSizeBytes / 1024}KB)", FileSizeMB = fileSizeMB };
            }

            if (file.Length > MaxSizeBytes)
            {
                return new GLBValidationResult { IsValid = false, ErrorMessage = $"File too large (maximum {MaxSizeBytes / (1024 * 1024)}MB)", FileSizeMB = fileSizeMB };
            }

            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new BinaryReader(stream);

                if (stream.Length < HeaderSize)
                {
                    return new GLBValidationResult { IsValid = false, ErrorMessage = "Invalid GLB file: header too short", FileSizeMB = fileSizeMB };
                }

                var magic = reader.ReadUInt32();
                var version = reader.ReadUInt32();
                var length = reader.ReadUInt32();

                if (magic != GLB_MAGIC)
                {
                    return new GLBValidationResult { IsValid = false, ErrorMessage = "Invalid GLB file: wrong magic number", FileSizeMB = fileSizeMB };
                }

                if (version != 2)
                {
                    return new GLBValidationResult { IsValid = false, ErrorMessage = $"Unsupported GLB version: {version} (only version 2 is supported)", FileSizeMB = fileSizeMB, GltfVersion = version.ToString() };
                }

                // First chunk: JSON
                if (stream.Position + 8 > stream.Length)
                {
                    return new GLBValidationResult { IsValid = false, ErrorMessage = "Invalid GLB file: missing JSON chunk header", FileSizeMB = fileSizeMB };
                }
                var jsonChunkLength = reader.ReadUInt32();
                var jsonChunkType = reader.ReadUInt32();

                if (jsonChunkType != JSON_CHUNK_TYPE)
                {
                    return new GLBValidationResult { IsValid = false, ErrorMessage = "Invalid GLB file: JSON chunk not found", FileSizeMB = fileSizeMB };
                }

                if (jsonChunkLength > stream.Length - stream.Position)
                {
                    return new GLBValidationResult { IsValid = false, ErrorMessage = "Invalid GLB file: JSON chunk length out of range", FileSizeMB = fileSizeMB };
                }

                var jsonBytes = reader.ReadBytes((int)jsonChunkLength);
                var jsonString = Encoding.UTF8.GetString(jsonBytes);

                int meshCount = 0;
                bool hasTextures = false;
                try
                {
                    using var jsonDoc = JsonDocument.Parse(jsonString);
                    var root = jsonDoc.RootElement;
                    if (root.TryGetProperty("meshes", out var meshesElement))
                    {
                        meshCount = meshesElement.ValueKind == JsonValueKind.Array ? meshesElement.GetArrayLength() : 0;
                    }
                    // Heuristic textures presence
                    if (root.TryGetProperty("textures", out var texturesElem))
                    {
                        hasTextures = texturesElem.ValueKind == JsonValueKind.Array && texturesElem.GetArrayLength() > 0;
                    }
                }
                catch (JsonException ex)
                {
                    return new GLBValidationResult { IsValid = false, ErrorMessage = $"Invalid JSON in GLB file: {ex.Message}", FileSizeMB = fileSizeMB };
                }

                return new GLBValidationResult
                {
                    IsValid = true,
                    ErrorMessage = null,
                    MeshCount = meshCount,
                    FileSizeMB = fileSizeMB,
                    HasTextures = hasTextures,
                    GltfVersion = "2.0"
                };
            }
            catch (Exception ex)
            {
                return new GLBValidationResult { IsValid = false, ErrorMessage = $"Error reading GLB file: {ex.Message}", FileSizeMB = fileSizeMB };
            }
        }
    }
}
