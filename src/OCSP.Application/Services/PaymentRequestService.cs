using OCSP.Application.DTOs.Budget;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace OCSP.Application.Services
{
    public class PaymentRequestService : IPaymentRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public PaymentRequestService(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<List<PaymentRequestDto>> GetAllByProjectAsync(Guid projectId, CancellationToken ct = default)
        {
            var paymentRequests = await _context.Set<PaymentRequest>()
                .Include(p => p.Project)
                .Include(p => p.RequestedBy)
                .Include(p => p.ApprovedBy)
                .Where(p => p.ProjectId == projectId && !p.IsDeleted)
                .OrderByDescending(p => p.RequestDate)
                .ToListAsync(ct);

            return paymentRequests.Select(p => MapToDto(p)).ToList();
        }

        public async Task<PaymentRequestDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var paymentRequest = await _context.Set<PaymentRequest>()
                .Include(p => p.Project)
                .Include(p => p.RequestedBy)
                .Include(p => p.ApprovedBy)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

            return paymentRequest == null ? null : MapToDto(paymentRequest);
        }

        public async Task<PaymentRequestDto> CreateAsync(CreatePaymentRequestDto dto, Guid currentUserId, CancellationToken ct = default)
        {
            // Validate project exists
            var projectExists = await _context.Set<Project>().AnyAsync(p => p.Id == dto.ProjectId, ct);
            if (!projectExists)
                throw new ArgumentException("Project not found");

            // Validate work items if specified
            if (dto.RelatedWorkItemIds != null && dto.RelatedWorkItemIds.Any())
            {
                foreach (var workItemId in dto.RelatedWorkItemIds)
                {
                    var workItemExists = await _context.Set<WorkItem>().AnyAsync(w => w.Id == workItemId && !w.IsDeleted, ct);
                    if (!workItemExists)
                        throw new ArgumentException($"Work item {workItemId} not found");
                }
            }

            // Generate unique code
            var code = await GeneratePaymentRequestCode(dto.ProjectId, ct);

            // Handle file uploads
            var documentUrls = new List<string>();
            if (dto.SupportingDocuments != null && dto.SupportingDocuments.Any())
            {
                foreach (var file in dto.SupportingDocuments)
                {
                    using var stream = file.OpenReadStream();
                    var url = await _fileService.UploadFileAsync(stream, file.FileName, "payment-requests");
                    documentUrls.Add(url);
                }
            }

            var paymentRequest = new PaymentRequest
            {
                Id = Guid.NewGuid(),
                Code = code,
                ProjectId = dto.ProjectId,
                RequestDate = DateTime.UtcNow,
                Amount = dto.Amount,
                Description = dto.Description,
                Status = PaymentRequestStatus.Pending,
                RequestedById = currentUserId,
                RelatedWorkItemIds = dto.RelatedWorkItemIds != null ? JsonSerializer.Serialize(dto.RelatedWorkItemIds) : null,
                SupportingDocuments = documentUrls.Any() ? JsonSerializer.Serialize(documentUrls) : null,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId.ToString(),
                UpdatedBy = currentUserId.ToString()
            };

            _context.Set<PaymentRequest>().Add(paymentRequest);
            await _context.SaveChangesAsync(ct);

            return await GetByIdAsync(paymentRequest.Id, ct) ?? throw new Exception("Failed to retrieve created payment request");
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var paymentRequest = await _context.Set<PaymentRequest>()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

            if (paymentRequest == null)
                throw new ArgumentException("Payment request not found");

            // Only allow deletion of pending requests
            if (paymentRequest.Status != PaymentRequestStatus.Pending)
                throw new InvalidOperationException("Only pending payment requests can be deleted");

            paymentRequest.IsDeleted = true;
            paymentRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
        }

        public async Task<PaymentRequestDto> ApproveAsync(Guid id, Guid approverId, ApprovePaymentRequestDto dto, CancellationToken ct = default)
        {
            var paymentRequest = await _context.Set<PaymentRequest>()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

            if (paymentRequest == null)
                throw new ArgumentException("Payment request not found");

            if (paymentRequest.Status != PaymentRequestStatus.Pending)
                throw new InvalidOperationException("Only pending payment requests can be approved");

            paymentRequest.Status = PaymentRequestStatus.Approved;
            paymentRequest.ApprovedById = approverId;
            paymentRequest.ApprovedDate = DateTime.UtcNow;
            paymentRequest.Notes = dto.Notes ?? paymentRequest.Notes;
            paymentRequest.UpdatedAt = DateTime.UtcNow;
            paymentRequest.UpdatedBy = approverId.ToString();

            await _context.SaveChangesAsync(ct);

            return await GetByIdAsync(id, ct) ?? throw new Exception("Failed to retrieve updated payment request");
        }

        public async Task<PaymentRequestDto> RejectAsync(Guid id, Guid approverId, RejectPaymentRequestDto dto, CancellationToken ct = default)
        {
            var paymentRequest = await _context.Set<PaymentRequest>()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

            if (paymentRequest == null)
                throw new ArgumentException("Payment request not found");

            if (paymentRequest.Status != PaymentRequestStatus.Pending)
                throw new InvalidOperationException("Only pending payment requests can be rejected");

            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                throw new ArgumentException("Rejection reason is required");

            paymentRequest.Status = PaymentRequestStatus.Rejected;
            paymentRequest.ApprovedById = approverId;
            paymentRequest.ApprovedDate = DateTime.UtcNow;
            paymentRequest.RejectionReason = dto.RejectionReason;
            paymentRequest.UpdatedAt = DateTime.UtcNow;
            paymentRequest.UpdatedBy = approverId.ToString();

            await _context.SaveChangesAsync(ct);

            return await GetByIdAsync(id, ct) ?? throw new Exception("Failed to retrieve updated payment request");
        }

        public async Task<PaymentRequestDto> MarkAsPaidAsync(Guid id, Guid currentUserId, CancellationToken ct = default)
        {
            var paymentRequest = await _context.Set<PaymentRequest>()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

            if (paymentRequest == null)
                throw new ArgumentException("Payment request not found");

            if (paymentRequest.Status != PaymentRequestStatus.Approved)
                throw new InvalidOperationException("Only approved payment requests can be marked as paid");

            paymentRequest.Status = PaymentRequestStatus.Paid;
            paymentRequest.PaidDate = DateTime.UtcNow;
            paymentRequest.UpdatedAt = DateTime.UtcNow;
            paymentRequest.UpdatedBy = currentUserId.ToString();

            await _context.SaveChangesAsync(ct);

            return await GetByIdAsync(id, ct) ?? throw new Exception("Failed to retrieve updated payment request");
        }

        public async Task<PaymentRequestDto> CancelAsync(Guid id, Guid currentUserId, CancellationToken ct = default)
        {
            var paymentRequest = await _context.Set<PaymentRequest>()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

            if (paymentRequest == null)
                throw new ArgumentException("Payment request not found");

            if (paymentRequest.Status == PaymentRequestStatus.Paid)
                throw new InvalidOperationException("Paid payment requests cannot be cancelled");

            paymentRequest.Status = PaymentRequestStatus.Cancelled;
            paymentRequest.UpdatedAt = DateTime.UtcNow;
            paymentRequest.UpdatedBy = currentUserId.ToString();

            await _context.SaveChangesAsync(ct);

            return await GetByIdAsync(id, ct) ?? throw new Exception("Failed to retrieve updated payment request");
        }

        public async Task<PaymentStatisticsDto> GetStatisticsAsync(Guid projectId, CancellationToken ct = default)
        {
            var paymentRequests = await _context.Set<PaymentRequest>()
                .Where(p => p.ProjectId == projectId && !p.IsDeleted)
                .ToListAsync(ct);

            return new PaymentStatisticsDto
            {
                TotalRequests = paymentRequests.Count,
                PendingCount = paymentRequests.Count(p => p.Status == PaymentRequestStatus.Pending),
                ApprovedCount = paymentRequests.Count(p => p.Status == PaymentRequestStatus.Approved),
                PaidCount = paymentRequests.Count(p => p.Status == PaymentRequestStatus.Paid),
                RejectedCount = paymentRequests.Count(p => p.Status == PaymentRequestStatus.Rejected),
                TotalAmount = paymentRequests.Sum(p => p.Amount),
                PaidAmount = paymentRequests.Where(p => p.Status == PaymentRequestStatus.Paid).Sum(p => p.Amount)
            };
        }

        public async Task<byte[]> DownloadDocumentAsync(Guid id, CancellationToken ct = default)
        {
            // TODO: Implement document download logic
            throw new NotImplementedException("Document download will be implemented with proper file storage");
        }

        // Private helper methods
        private PaymentRequestDto MapToDto(PaymentRequest paymentRequest)
        {
            var relatedWorkItemIds = new List<string>();
            if (!string.IsNullOrEmpty(paymentRequest.RelatedWorkItemIds))
            {
                try
                {
                    var ids = JsonSerializer.Deserialize<List<Guid>>(paymentRequest.RelatedWorkItemIds) ?? new List<Guid>();
                    relatedWorkItemIds = ids.Select(id => id.ToString()).ToList();
                }
                catch { }
            }

            var supportingDocuments = new List<string>();
            if (!string.IsNullOrEmpty(paymentRequest.SupportingDocuments))
            {
                try
                {
                    supportingDocuments = JsonSerializer.Deserialize<List<string>>(paymentRequest.SupportingDocuments) ?? new List<string>();
                }
                catch { }
            }

            return new PaymentRequestDto
            {
                Id = paymentRequest.Id,
                Code = paymentRequest.Code,
                ProjectId = paymentRequest.ProjectId,
                ProjectName = paymentRequest.Project?.Name ?? string.Empty,
                RequestDate = paymentRequest.RequestDate,
                ApprovedDate = paymentRequest.ApprovedDate,
                PaidDate = paymentRequest.PaidDate,
                Amount = paymentRequest.Amount,
                Description = paymentRequest.Description,
                Status = paymentRequest.Status.ToString(),
                StatusLabel = GetStatusLabel(paymentRequest.Status),
                RequestedById = paymentRequest.RequestedById,
                RequestedByName = paymentRequest.RequestedBy?.Username ?? string.Empty,
                ApprovedById = paymentRequest.ApprovedById,
                ApprovedByName = paymentRequest.ApprovedBy?.Username,
                RejectionReason = paymentRequest.RejectionReason,
                RelatedWorkItemIds = relatedWorkItemIds,
                SupportingDocuments = supportingDocuments,
                Notes = paymentRequest.Notes,
                CreatedAt = paymentRequest.CreatedAt,
                UpdatedAt = paymentRequest.UpdatedAt
            };
        }

        private string GetStatusLabel(PaymentRequestStatus status)
        {
            return status switch
            {
                PaymentRequestStatus.Pending => "Pending",
                PaymentRequestStatus.Approved => "Approved",
                PaymentRequestStatus.Paid => "Paid",
                PaymentRequestStatus.Rejected => "Rejected",
                PaymentRequestStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }

        private async Task<string> GeneratePaymentRequestCode(Guid projectId, CancellationToken ct)
        {
            var count = await _context.Set<PaymentRequest>()
                .Where(p => p.ProjectId == projectId && !p.IsDeleted)
                .CountAsync(ct);

            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month;

            return $"PR{year}{month:D2}{(count + 1):D4}";
        }
    }
}
