using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OCSP.Application.DTOs.Project;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;

namespace OCSP.Application.Services
{
    public class ProjectDocumentService : IProjectDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IEncryptionService _encryptionService;

        public ProjectDocumentService(
            ApplicationDbContext context,
            IFileService fileService,
            IEncryptionService encryptionService)
        {
            _context = context;
            _fileService = fileService;
            _encryptionService = encryptionService;
        }

        #region Upload Document

        public async Task<ProjectDocumentDto> UploadDocumentAsync(
            Guid projectId,
            Guid userId,
            IFormFile file,
            UploadProjectDocumentDto dto)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            // 1. Validate project exists and user has permission
            var project = await _context.Projects
                .Include(p => p.Homeowner)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new Exception("Dự án không tồn tại");

            if (project.HomeownerId != userId)
                throw new Exception("Bạn không có quyền upload tài liệu cho dự án này");

            // 2. Validate file
            var docType = (ProjectDocumentType)dto.DocumentType;
            ValidateFile(file, docType);

            // 3. Read file into memory
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            // 4. Calculate hash (for duplicate check & integrity)
            var fileHash = ComputeSHA256(fileBytes);

            // 5. Check duplicate
            var existingDoc = await _context.ProjectDocuments
                .FirstOrDefaultAsync(d => d.ProjectId == projectId
                                       && d.DocumentType == docType
                                       && d.FileHash == fileHash);
            if (existingDoc != null)
                throw new Exception("File này đã được upload trước đó");

            // 6. Process by document type
            string fileUrl;
            bool isEncrypted = false;
            string? encryptionKeyId = null;

            if (docType == ProjectDocumentType.Permit)
            {
                // ✅ PERMIT: Encrypt only (no OCR - data comes from frontend)

                // 6a. Encrypt file
                var encryptionResult = await _encryptionService.EncryptFileAsync(fileBytes);
                isEncrypted = true;
                encryptionKeyId = encryptionResult.KeyId;

                // 6b. Upload ENCRYPTED file
                using var encryptedStream = new MemoryStream(encryptionResult.EncryptedData);
                fileUrl = await _fileService.UploadFileAsync(
                    encryptedStream,
                    $"{projectId}/permits/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}",
                    "project-documents"
                );
            }
            else
            {
                // ✅ DRAWING: Normal upload, NO encryption
                using var fileStream = new MemoryStream(fileBytes);
                fileUrl = await _fileService.UploadFileAsync(
                    fileStream,
                    $"{projectId}/drawings/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}",
                    "project-documents"
                );
            }

            // 7. Versioning and demote previous latest for same type
            var prevLatest = await _context.ProjectDocuments
                .Where(d => d.ProjectId == projectId && d.DocumentType == docType && d.IsLatest)
                .OrderByDescending(d => d.Version)
                .FirstOrDefaultAsync();

            int nextVersion = (prevLatest?.Version ?? 0) + 1;
            if (prevLatest != null)
            {
                prevLatest.IsLatest = false;
                _context.ProjectDocuments.Update(prevLatest);
            }

            // 8. Save to database
            var document = new ProjectDocument
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                DocumentType = docType,
                FileName = file.FileName,
                FileUrl = fileUrl,
                FileType = Path.GetExtension(file.FileName).ToLowerInvariant(),
                FileSize = file.Length,
                IsEncrypted = isEncrypted,
                EncryptionKeyId = encryptionKeyId,
                FileHash = fileHash,
                ExtractedDataJson = null, // No OCR data - comes from frontend
                UploadedByUserId = userId,
                UploadedAt = DateTime.UtcNow,
                Version = nextVersion,
                IsLatest = true
            };

            _context.ProjectDocuments.Add(document);

            // Note: PermitMetadata will be created separately when OCR data is available from frontend
            // For now, we just store the encrypted permit file

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            // 9. Return DTO
            var result = await GetDocumentDtoAsync(document.Id);
            return result;
        }

        #endregion

        #region Get Documents

        public async Task<IEnumerable<ProjectDocumentDto>> GetProjectDocumentsAsync(Guid projectId)
        {
            var documents = await _context.ProjectDocuments
                .Include(d => d.UploadedBy)
                .Where(d => d.ProjectId == projectId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            var dtos = new List<ProjectDocumentDto>();
            foreach (var doc in documents)
            {
                dtos.Add(await GetDocumentDtoAsync(doc.Id));
            }

            return dtos;
        }

        #endregion

        #region Download Document

        public async Task<(Stream FileStream, string FileName, string ContentType)> GetDocumentFileAsync(
            Guid documentId,
            Guid userId)
        {
            var document = await _context.ProjectDocuments
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                throw new Exception("Tài liệu không tồn tại");

            // TODO: Check permission (user has access to this project's document)
            // For now, we'll skip this check

            // 1. Get file from storage
            var fileBytes = await _fileService.GetFileAsync(document.FileUrl);

            // 2. If encrypted → decrypt
            if (document.IsEncrypted)
            {
                if (string.IsNullOrEmpty(document.EncryptionKeyId))
                    throw new InvalidOperationException("File bị mã hóa nhưng không có key");

                fileBytes = await _encryptionService.DecryptFileAsync(
                    fileBytes,
                    document.EncryptionKeyId
                );
            }

            // 3. Return file stream
            var stream = new MemoryStream(fileBytes);
            var contentType = GetContentType(document.FileType);

            return (stream, document.FileName, contentType);
        }

        #endregion

        #region Delete Document

        public async Task DeleteDocumentAsync(Guid documentId, Guid userId)
        {
            var document = await _context.ProjectDocuments
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                throw new Exception("Tài liệu không tồn tại");

            if (document.Project.HomeownerId != userId)
                throw new Exception("Bạn không có quyền xóa tài liệu này");

            // Delete file from storage
            await _fileService.DeleteFileAsync(document.FileUrl);

            // Delete from database (cascade delete will handle PermitMetadata)
            _context.ProjectDocuments.Remove(document);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Verify Permit

        public async Task<VerificationResultDto> VerifyPermitAsync(Guid permitDocumentId)
        {
            var document = await _context.ProjectDocuments
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d => d.Id == permitDocumentId);

            if (document == null)
                throw new Exception("Tài liệu không tồn tại");

            if (document.DocumentType != ProjectDocumentType.Permit)
                throw new Exception("Chỉ có thể xác minh giấy phép xây dựng");

            var metadata = await _context.PermitMetadata
                .FirstOrDefaultAsync(m => m.ProjectDocumentId == permitDocumentId);

            if (metadata == null)
                throw new Exception("Không tìm thấy metadata của giấy phép");

            // Verification rules
            var rules = new List<VerificationRuleDto>();

            // Rule 1: Permit Number
            rules.Add(new VerificationRuleDto
            {
                Name = "PermitNumber",
                IsValid = !string.IsNullOrWhiteSpace(metadata.PermitNumber),
                Message = !string.IsNullOrWhiteSpace(metadata.PermitNumber)
                    ? "✓ Số giấy phép hợp lệ"
                    : "✗ Thiếu số giấy phép",
                Severity = !string.IsNullOrWhiteSpace(metadata.PermitNumber) ? "info" : "error"
            });

            // Rule 2: Area validation
            var areaValid = metadata.Area.HasValue && metadata.Area >= 20 && metadata.Area <= 100000;
            rules.Add(new VerificationRuleDto
            {
                Name = "Area",
                IsValid = areaValid,
                Message = areaValid
                    ? $"✓ Diện tích hợp lệ ({metadata.Area} m²)"
                    : metadata.Area.HasValue
                        ? $"✗ Diện tích ngoài phạm vi cho phép ({metadata.Area} m²)"
                        : "✗ Thiếu thông tin diện tích",
                Severity = areaValid ? "info" : "error"
            });

            // Rule 3: Address validation
            var addressValid = !string.IsNullOrWhiteSpace(metadata.Address)
                            && metadata.Address.Length >= 10;
            rules.Add(new VerificationRuleDto
            {
                Name = "Address",
                IsValid = addressValid,
                Message = addressValid
                    ? "✓ Địa chỉ hợp lệ"
                    : "✗ Địa chỉ không hợp lệ hoặc quá ngắn",
                Severity = addressValid ? "info" : "warning"
            });

            // Rule 4: Owner validation
            var ownerValid = !string.IsNullOrWhiteSpace(metadata.Owner);
            rules.Add(new VerificationRuleDto
            {
                Name = "Owner",
                IsValid = ownerValid,
                Message = ownerValid
                    ? "✓ Thông tin chủ đầu tư đầy đủ"
                    : "✗ Thiếu thông tin chủ đầu tư",
                Severity = ownerValid ? "info" : "warning"
            });

            // Rule 5: Expiry date validation
            if (metadata.ExpiryDate.HasValue)
            {
                var daysLeft = (metadata.ExpiryDate.Value - DateTime.Now).TotalDays;

                if (daysLeft < 0)
                {
                    rules.Add(new VerificationRuleDto
                    {
                        Name = "ExpiryDate",
                        IsValid = false,
                        Message = "✗ Giấy phép đã hết hạn",
                        Severity = "error"
                    });
                }
                else if (daysLeft < 30)
                {
                    rules.Add(new VerificationRuleDto
                    {
                        Name = "ExpiryDate",
                        IsValid = true,
                        Message = $"⚠ Giấy phép sắp hết hạn ({Math.Round(daysLeft)} ngày)",
                        Severity = "warning"
                    });
                }
                else
                {
                    rules.Add(new VerificationRuleDto
                    {
                        Name = "ExpiryDate",
                        IsValid = true,
                        Message = $"✓ Giấy phép còn hiệu lực ({Math.Round(daysLeft)} ngày)",
                        Severity = "info"
                    });
                }
            }
            else
            {
                rules.Add(new VerificationRuleDto
                {
                    Name = "ExpiryDate",
                    IsValid = true,
                    Message = "ℹ Không có thông tin ngày hết hạn",
                    Severity = "info"
                });
            }

            // Rule 6: OCR confidence validation
            if (metadata.OcrConfidence.HasValue)
            {
                var confidenceValid = metadata.OcrConfidence >= 0.7f;
                rules.Add(new VerificationRuleDto
                {
                    Name = "OcrConfidence",
                    IsValid = confidenceValid,
                    Message = confidenceValid
                        ? $"✓ Độ chính xác OCR tốt ({metadata.OcrConfidence:P0})"
                        : $"⚠ Độ chính xác OCR thấp ({metadata.OcrConfidence:P0}). Nên kiểm tra lại thủ công",
                    Severity = confidenceValid ? "info" : "warning"
                });
            }

            // Calculate overall result
            var hasError = rules.Any(r => r.Severity == "error" && !r.IsValid);
            var hasWarning = rules.Any(r => r.Severity == "warning" && !r.IsValid);

            string status;
            string message;

            if (hasError)
            {
                status = "fail";
                message = "Xác minh KHÔNG THÀNH CÔNG. Có lỗi cần sửa.";
            }
            else if (hasWarning)
            {
                status = "warning";
                message = "Xác minh THÀNH CÔNG với một số cảnh báo.";
            }
            else
            {
                status = "pass";
                message = "Xác minh THÀNH CÔNG. Giấy phép hợp lệ.";
            }

            // Update verification status
            metadata.IsVerified = !hasError;
            metadata.VerifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new VerificationResultDto
            {
                IsValid = !hasError,
                Status = status,
                Rules = rules,
                Message = message
            };
        }

        #endregion

        #region Helper Methods

        private void ValidateFile(IFormFile file, ProjectDocumentType docType)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File không được để trống");

            // Check file size
            long maxSize = docType == ProjectDocumentType.Drawing
                ? 100 * 1024 * 1024  // 100MB for drawings
                : 10 * 1024 * 1024; // 10MB for permits

            if (file.Length > maxSize)
                throw new Exception($"File quá lớn. Tối đa {maxSize / (1024 * 1024)}MB");

            // Check file extension
            var allowedExtensions = docType == ProjectDocumentType.Drawing
                ? new[] { ".pdf" }
                : new[] { ".pdf", ".jpg", ".jpeg", ".png" };

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                throw new Exception($"Chỉ chấp nhận file: {string.Join(", ", allowedExtensions)}");

            // TODO: Validate magic bytes (prevent fake extensions)
        }

        private string ComputeSHA256(byte[] data)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }

        private async Task<ProjectDocumentDto> GetDocumentDtoAsync(Guid documentId)
        {
            var document = await _context.ProjectDocuments
                .Include(d => d.UploadedBy)
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                throw new Exception("Tài liệu không tồn tại");

            var dto = new ProjectDocumentDto
            {
                Id = document.Id,
                ProjectId = document.ProjectId,
                DocumentType = (int)document.DocumentType,
                DocumentTypeName = document.DocumentType.ToString(),
                FileName = document.FileName,
                FileUrl = document.FileUrl,
                FileType = document.FileType,
                FileSize = document.FileSize,
                FileSizeFormatted = FormatFileSize(document.FileSize),
                IsEncrypted = document.IsEncrypted,
                FileHash = document.FileHash,
                UploadedByUserId = document.UploadedByUserId,
                UploadedByUsername = document.UploadedBy.Username,
                UploadedAt = document.UploadedAt,
                Version = document.Version,
                IsLatest = document.IsLatest
            };

            // If Permit → load metadata
            if (document.DocumentType == ProjectDocumentType.Permit)
            {
                var metadata = await _context.PermitMetadata
                    .FirstOrDefaultAsync(m => m.ProjectDocumentId == documentId);

                if (metadata != null)
                {
                    dto.PermitMetadata = new PermitMetadataDto
                    {
                        Id = metadata.Id,
                        PermitNumber = metadata.PermitNumber,
                        Area = metadata.Area,
                        Address = metadata.Address,
                        Owner = metadata.Owner,
                        IssueDate = metadata.IssueDate,
                        ExpiryDate = metadata.ExpiryDate,
                        OcrConfidence = metadata.OcrConfidence,
                        IsVerified = metadata.IsVerified,
                        ExtractedAt = metadata.ExtractedAt,
                        Warnings = GetPermitWarnings(metadata)
                    };
                }
            }

            return dto;
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private List<string> GetPermitWarnings(PermitMetadata metadata)
        {
            var warnings = new List<string>();

            // Warning 1: Expiry date
            if (metadata.ExpiryDate.HasValue)
            {
                var daysLeft = (metadata.ExpiryDate.Value - DateTime.Now).TotalDays;
                if (daysLeft < 0)
                    warnings.Add("⚠️ Giấy phép đã hết hạn");
                else if (daysLeft < 30)
                    warnings.Add($"⚠️ Giấy phép sắp hết hạn ({Math.Round(daysLeft)} ngày)");
            }

            // Warning 2: Low OCR confidence
            if (metadata.OcrConfidence.HasValue && metadata.OcrConfidence < 0.7f)
                warnings.Add("⚠️ Độ chính xác OCR thấp. Vui lòng kiểm tra lại thông tin");

            // Warning 3: Not verified
            if (!metadata.IsVerified)
                warnings.Add("ℹ️ Chưa được xác minh bởi hệ thống");

            return warnings;
        }

        #endregion
    }
}