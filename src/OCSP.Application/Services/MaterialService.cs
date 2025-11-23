using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using OCSP.Application.DTOs.Material;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.ExternalServices.Interfaces;

namespace OCSP.Application.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public MaterialService(ApplicationDbContext context, IEmailService emailService, INotificationService notificationService)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        #region Material Request Operations

        public async Task<MaterialRequestDetailDto> CreateRequestAsync(Guid projectId, Guid contractorId, CancellationToken ct = default)
        {
            var project = await _context.Projects.FindAsync(new object[] { projectId }, ct);
            if (project == null)
                throw new ArgumentException("Project not found");

            var request = new MaterialRequest
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                ContractorId = contractorId,
                RequestDate = DateTime.UtcNow,
                Status = MaterialRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.MaterialRequests.Add(request);
            await _context.SaveChangesAsync(ct);

            return await MapToRequestDetailDto(request, ct);
        }

        public async Task<List<MaterialRequestDto>> GetAllRequestsAsync(Guid projectId, CancellationToken ct = default)
        {
            var requests = await _context.MaterialRequests
                .Where(r => r.ProjectId == projectId)
                .Include(r => r.Materials)
                .Include(r => r.Project) // Include project for delegation setting
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync(ct);

            return requests.Select(r => MapToRequestDto(r)).ToList();
        }

        public async Task<MaterialRequestDetailDto?> GetRequestByIdAsync(Guid requestId, CancellationToken ct = default)
        {
            var request = await _context.MaterialRequests
                .Include(r => r.Materials)
                .Include(r => r.ApprovalHistories)
                .FirstOrDefaultAsync(r => r.Id == requestId, ct);

            if (request == null) return null;

            return await MapToRequestDetailDto(request, ct);
        }

        #endregion

        #region Import Excel

        public async Task<MaterialRequestDetailDto> ImportMaterialsFromExcelAsync(Guid requestId, IFormFile file, CancellationToken ct = default)
        {
            var request = await _context.MaterialRequests
                .Include(r => r.Materials)
                .FirstOrDefaultAsync(r => r.Id == requestId, ct);

            if (request == null)
                throw new ArgumentException("Material request not found");

            if (request.Status != MaterialRequestStatus.Pending)
                throw new InvalidOperationException("Can only import materials for pending requests");

            using var stream = file.OpenReadStream();
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            if (rowCount < 2) // Header + at least 1 data row
                throw new ArgumentException("Excel file is empty or invalid");

            // Clear existing materials if reimporting
            if (request.Materials.Any())
            {
                _context.Materials.RemoveRange(request.Materials);
            }

            var materials = new List<Material>();
            int sortOrder = 0;

            // Start from row 2 (assuming row 1 is header)
            // Column A (1): STT | B (2): Mã số | C (3): Hạng mục | D (4): Đơn vị | E (5): Đơn giá | F (6): Khối lượng theo HĐ
            for (int row = 2; row <= rowCount; row++)
            {
                var code = worksheet.Cells[row, 2].Text?.Trim(); // Mã số (column B)
                var name = worksheet.Cells[row, 3].Text?.Trim(); // Hạng mục (column C)

                if (string.IsNullOrEmpty(name)) continue;

                var material = new Material
                {
                    Id = Guid.NewGuid(),
                    MaterialRequestId = requestId,
                    ProjectId = request.ProjectId,
                    Code = code ?? $"MAT{sortOrder:D5}",
                    Name = name,
                    Unit = worksheet.Cells[row, 4].Text?.Trim() ?? "m³", // Đơn vị (column D)
                    SortOrder = sortOrder++,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Parse unit price (Đơn giá - column E/5)
                if (decimal.TryParse(worksheet.Cells[row, 5].Text?.Replace(",", ""), out decimal unitPrice))
                {
                    material.UnitPrice = unitPrice;
                }

                // Parse contract quantity (Khối lượng theo HĐ - column F/6)
                if (decimal.TryParse(worksheet.Cells[row, 6].Text?.Replace(",", ""), out decimal contractQty))
                {
                    material.ContractQuantity = contractQty;
                    material.ContractAmount = contractQty * material.UnitPrice;
                }

                // Parse estimated quantity (Khối lượng theo NKTC - column 9 if exists)
                if (decimal.TryParse(worksheet.Cells[row, 9].Text?.Replace(",", ""), out decimal estimatedQty))
                {
                    material.EstimatedQuantity = estimatedQty;
                    material.EstimatedAmount = estimatedQty * material.UnitPrice;
                }
                else
                {
                    // If no NKTC quantity, use contract quantity
                    material.EstimatedQuantity = material.ContractQuantity ?? 0;
                    material.EstimatedAmount = material.EstimatedQuantity * material.UnitPrice;
                }

                materials.Add(material);
            }

            _context.Materials.AddRange(materials);

            // Update request file info
            request.FileName = file.FileName;
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            // Send email notifications to Homeowner and Supervisor
            await SendApprovalRequestEmailsAsync(request, ct);

            // Create notifications for all project participants (except contractor)
            var project = await _context.Projects.FindAsync(new object[] { request.ProjectId }, ct);
            var contractor = await _context.Users.FindAsync(new object[] { request.ContractorId }, ct);
            await _notificationService.CreateForProjectParticipantsAsync(
                request.ProjectId,
                "Nhà thầu đã tạo phiếu xuất vật tư mới",
                $"Nhà thầu {contractor?.Username ?? "N/A"} đã tạo phiếu xuất vật tư cho dự án {project?.Name ?? "N/A"}. Vui lòng kiểm tra và phê duyệt.",
                NotificationType.MaterialRequestUploaded,
                request.Id,
                $"/projects/{request.ProjectId}/materials",
                request.ContractorId, // Exclude contractor from notification
                ct
            );

            return await MapToRequestDetailDto(request, ct);
        }

        #endregion

        #region Approval Operations

        public async Task<MaterialRequestDetailDto> ApproveByHomeownerAsync(Guid requestId, Guid homeownerId, ApproveMaterialRequestDto dto, CancellationToken ct = default)
        {
            var request = await _context.MaterialRequests
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.Id == requestId, ct);

            if (request == null)
                throw new ArgumentException("Material request not found");

            if (request.Project.HomeownerId != homeownerId)
                throw new UnauthorizedAccessException("Only project homeowner can approve");

            if (request.Status == MaterialRequestStatus.Rejected)
                throw new InvalidOperationException("Cannot approve rejected request");

            request.ApprovedByHomeowner = true;
            request.ApprovedByHomeownerId = homeownerId;
            request.ApprovedByHomeownerAt = DateTime.UtcNow;

            // Explicitly mark properties as modified to ensure EF Core tracks changes
            _context.Entry(request).Property(r => r.ApprovedByHomeowner).IsModified = true;
            _context.Entry(request).Property(r => r.ApprovedByHomeownerId).IsModified = true;

            // Create approval history
            var history = new MaterialApprovalHistory
            {
                Id = Guid.NewGuid(),
                MaterialRequestId = requestId,
                ApprovedById = homeownerId,
                ApproverRole = ApproverRole.Homeowner,
                Action = ApprovalAction.Approved,
                ActionDate = DateTime.UtcNow,
                Comments = dto.Comments,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.MaterialApprovalHistories.Add(history);

            // Update status if both approved
            if (request.ApprovedBySupervisor)
            {
                request.Status = MaterialRequestStatus.Approved;
            }
            else
            {
                request.Status = MaterialRequestStatus.PartiallyApproved;
            }

            // Explicitly mark Status as modified
            _context.Entry(request).Property(r => r.Status).IsModified = true;

            request.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            // Create notifications for all project participants
            var homeowner = await _context.Users.FindAsync(new object[] { homeownerId }, ct);
            var notificationType = request.Status == MaterialRequestStatus.Approved
                ? NotificationType.MaterialRequestApproved
                : NotificationType.MaterialRequestPartiallyApproved;
            var title = request.Status == MaterialRequestStatus.Approved
                ? "Phiếu xuất vật tư đã được phê duyệt"
                : "Phiếu xuất vật tư đã được phê duyệt một phần";
            var message = $"Chủ nhà {homeowner?.Username ?? "N/A"} đã phê duyệt phiếu xuất vật tư cho dự án {request.Project?.Name ?? "N/A"}.";

            await _notificationService.CreateForProjectParticipantsAsync(
                request.ProjectId,
                title,
                message,
                notificationType,
                requestId,
                $"/projects/{request.ProjectId}/materials",
                homeownerId, // Exclude homeowner from notification
                ct
            );

            return await MapToRequestDetailDto(request, ct);
        }

        public async Task<MaterialRequestDetailDto> ApproveBySupervisorAsync(Guid requestId, Guid supervisorId, ApproveMaterialRequestDto dto, CancellationToken ct = default)
        {
            var request = await _context.MaterialRequests
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.Id == requestId, ct);

            if (request == null)
                throw new ArgumentException("Material request not found");

            // Check if user is supervisor of this project
            var participant = await _context.ProjectParticipants
                .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId &&
                                        p.UserId == supervisorId &&
                                        p.DetailedRole == ProjectParticipantRole.MainSupervisor, ct);

            if (participant == null)
                throw new UnauthorizedAccessException("Only chief supervisor can approve");

            if (request.Status == MaterialRequestStatus.Rejected)
                throw new InvalidOperationException("Cannot approve rejected request");

            request.ApprovedBySupervisor = true;
            request.ApprovedBySupervisorId = supervisorId;
            request.ApprovedBySupervisorAt = DateTime.UtcNow;

            // Explicitly mark properties as modified to ensure EF Core tracks changes
            _context.Entry(request).Property(r => r.ApprovedBySupervisor).IsModified = true;
            _context.Entry(request).Property(r => r.ApprovedBySupervisorId).IsModified = true;

            // Create approval history
            var history = new MaterialApprovalHistory
            {
                Id = Guid.NewGuid(),
                MaterialRequestId = requestId,
                ApprovedById = supervisorId,
                ApproverRole = ApproverRole.Supervisor,
                Action = ApprovalAction.Approved,
                ActionDate = DateTime.UtcNow,
                Comments = dto.Comments,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.MaterialApprovalHistories.Add(history);

            // Update status based on delegation setting
            if (request.Project.DelegateApprovalToSupervisor)
            {
                // If delegated, supervisor approval alone is sufficient
                // Automatically approve on behalf of homeowner
                request.ApprovedByHomeowner = true;
                request.ApprovedByHomeownerId = request.Project.HomeownerId;
                request.ApprovedByHomeownerAt = DateTime.UtcNow;
                _context.Entry(request).Property(r => r.ApprovedByHomeowner).IsModified = true;
                _context.Entry(request).Property(r => r.ApprovedByHomeownerId).IsModified = true;

                request.Status = MaterialRequestStatus.Approved;
            }
            else
            {
                // Normal flow: need both approvals
                if (request.ApprovedByHomeowner)
                {
                    request.Status = MaterialRequestStatus.Approved;
                }
                else
                {
                    request.Status = MaterialRequestStatus.PartiallyApproved;
                }
            }

            // Explicitly mark Status as modified
            _context.Entry(request).Property(r => r.Status).IsModified = true;

            request.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            // Create notifications for all project participants
            var supervisor = await _context.Users.FindAsync(new object[] { supervisorId }, ct);
            var notificationType = request.Status == MaterialRequestStatus.Approved
                ? NotificationType.MaterialRequestApproved
                : NotificationType.MaterialRequestPartiallyApproved;
            var title = request.Status == MaterialRequestStatus.Approved
                ? "Phiếu xuất vật tư đã được phê duyệt"
                : "Phiếu xuất vật tư đã được phê duyệt một phần";
            var message = $"Giám sát {supervisor?.Username ?? "N/A"} đã phê duyệt phiếu xuất vật tư cho dự án {request.Project?.Name ?? "N/A"}.";

            await _notificationService.CreateForProjectParticipantsAsync(
                request.ProjectId,
                title,
                message,
                notificationType,
                requestId,
                $"/projects/{request.ProjectId}/materials",
                supervisorId, // Exclude supervisor from notification
                ct
            );

            return await MapToRequestDetailDto(request, ct);
        }

        public async Task<MaterialRequestDetailDto> RejectRequestAsync(Guid requestId, Guid userId, RejectMaterialRequestDto dto, CancellationToken ct = default)
        {
            var request = await _context.MaterialRequests
                .Include(r => r.Project)
                .Include(r => r.Contractor)
                .FirstOrDefaultAsync(r => r.Id == requestId, ct);

            if (request == null)
                throw new ArgumentException("Material request not found");

            // Determine approver role
            ApproverRole role;
            string approverName = "";
            if (request.Project.HomeownerId == userId)
            {
                role = ApproverRole.Homeowner;
                var homeowner = await _context.Users.FindAsync(new object[] { userId }, ct);
                approverName = homeowner?.Username ?? "Chủ nhà";
            }
            else
            {
                var participant = await _context.ProjectParticipants
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId &&
                                            p.UserId == userId &&
                                            p.DetailedRole == ProjectParticipantRole.MainSupervisor, ct);
                if (participant == null)
                    throw new UnauthorizedAccessException("Only homeowner or chief supervisor can reject");

                role = ApproverRole.Supervisor;
                approverName = participant.User?.Username ?? "Giám sát";
            }

            request.Status = MaterialRequestStatus.Rejected;
            request.RejectionReason = dto.Reason;
            request.RejectedById = userId;
            request.RejectedAt = DateTime.UtcNow;

            // Create rejection history
            var history = new MaterialApprovalHistory
            {
                Id = Guid.NewGuid(),
                MaterialRequestId = requestId,
                ApprovedById = userId,
                ApproverRole = role,
                Action = ApprovalAction.Rejected,
                ActionDate = DateTime.UtcNow,
                Comments = dto.Comments,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.MaterialApprovalHistories.Add(history);
            request.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            // Create notifications for all project participants
            var notificationTitle = $"Phiếu xuất vật tư bị từ chối";
            var notificationMessage = $"{approverName} đã từ chối phiếu xuất vật tư cho dự án {request.Project?.Name}.\n\nLý do: {dto.Reason}\n\nGhi chú: {dto.Comments ?? "Không có"}";

            await _notificationService.CreateForProjectParticipantsAsync(
                request.ProjectId,
                notificationTitle,
                notificationMessage,
                NotificationType.MaterialRequestRejected,
                requestId,
                $"/projects/{request.ProjectId}/materials",
                userId, // Exclude the person who rejected
                ct
            );

            // Send email to contractor
            if (request.Contractor != null)
            {
                if (!string.IsNullOrEmpty(request.Contractor.Email))
                {
                    var emailSubject = $"[OCSP] Yêu cầu vật tư bị từ chối - {request.Project?.Name}";
                    var emailBody = $@"
                        <h2>Yêu cầu vật tư bị từ chối</h2>
                        <p>Xin chào {request.Contractor.Username},</p>
                        <p>{approverName} đã từ chối yêu cầu vật tư của bạn cho dự án <strong>{request.Project?.Name}</strong>.</p>
                        <p><strong>Lý do từ chối:</strong> {dto.Reason}</p>
                        {(!string.IsNullOrEmpty(dto.Comments) ? $"<p><strong>Ghi chú:</strong> {dto.Comments}</p>" : "")}
                        <p>Vui lòng xem lại và chỉnh sửa yêu cầu của bạn.</p>
                    ";

                    try
                    {
                        await _emailService.SendEmailAsync(request.Contractor.Email, emailSubject, emailBody);
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't fail the rejection
                        Console.WriteLine($"Failed to send rejection email: {ex.Message}");
                    }
                }
            }

            return await MapToRequestDetailDto(request, ct);
        }

        public async Task DeleteRequestAsync(Guid requestId, Guid userId, CancellationToken ct = default)
        {
            var request = await _context.MaterialRequests
                .Include(r => r.Materials)
                .Include(r => r.ApprovalHistories)
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.Id == requestId, ct);

            if (request == null)
                throw new ArgumentException("Material request not found");

            // Only allow deletion of Pending or Rejected requests
            if (request.Status == MaterialRequestStatus.Approved ||
                request.Status == MaterialRequestStatus.PartiallyApproved)
                throw new InvalidOperationException("Cannot delete approved or partially approved requests");

            // Check permissions: only contractor who created it or homeowner/supervisor can delete
            var isContractor = request.ContractorId == userId;
            var isHomeowner = request.Project?.HomeownerId == userId;
            var isSupervisor = await _context.ProjectParticipants
                .AnyAsync(p => p.ProjectId == request.ProjectId &&
                              p.UserId == userId &&
                              p.DetailedRole == ProjectParticipantRole.MainSupervisor, ct);

            if (!isContractor && !isHomeowner && !isSupervisor)
                throw new UnauthorizedAccessException("You do not have permission to delete this request");

            // Delete will cascade to Materials and ApprovalHistories due to EF relationships
            _context.MaterialRequests.Remove(request);
            await _context.SaveChangesAsync(ct);
        }

        public async Task ClearImportedMaterialsAsync(Guid requestId, Guid userId, CancellationToken ct = default)
        {
            var request = await _context.MaterialRequests
                .Include(r => r.Materials)
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.Id == requestId, ct);

            if (request == null)
                throw new ArgumentException("Material request not found");

            // Check permissions: only contractor who created it or homeowner/supervisor can clear
            var isContractor = request.ContractorId == userId;
            var isHomeowner = request.Project?.HomeownerId == userId;
            var isSupervisor = await _context.ProjectParticipants
                .AnyAsync(p => p.ProjectId == request.ProjectId &&
                              p.UserId == userId &&
                              p.DetailedRole == ProjectParticipantRole.MainSupervisor, ct);

            if (!isContractor && !isHomeowner && !isSupervisor)
                throw new UnauthorizedAccessException("You do not have permission to clear materials from this request");

            // Clear all materials (allowed for all statuses)
            if (request.Materials.Any())
            {
                _context.Materials.RemoveRange(request.Materials);
                await _context.SaveChangesAsync(ct);
            }
        }

        #endregion

        #region Material Operations

        public async Task<List<MaterialDto>> GetMaterialsByProjectAsync(Guid projectId, CancellationToken ct = default)
        {
            var materials = await _context.Materials
                .Include(m => m.MaterialRequest)
                .Include(m => m.Payments)
                .Where(m => m.ProjectId == projectId && m.MaterialRequest.Status == MaterialRequestStatus.Approved)
                .OrderBy(m => m.SortOrder)
                .ToListAsync(ct);

            return materials.Select(m => MapToMaterialDto(m)).ToList();
        }

        public async Task<MaterialDetailDto?> GetMaterialByIdAsync(Guid materialId, CancellationToken ct = default)
        {
            var material = await _context.Materials
                .Include(m => m.Payments)
                .Include(m => m.WorkItem)
                .FirstOrDefaultAsync(m => m.Id == materialId, ct);

            if (material == null) return null;

            return MapToMaterialDetailDto(material);
        }

        public async Task<MaterialDto> UpdateMaterialAsync(Guid materialId, UpdateMaterialDto dto, CancellationToken ct = default)
        {
            var material = await _context.Materials.FindAsync(new object[] { materialId }, ct);
            if (material == null)
                throw new ArgumentException("Material not found");

            if (dto.ContractQuantity.HasValue)
            {
                material.ContractQuantity = dto.ContractQuantity.Value;
                material.ContractAmount = material.ContractQuantity.Value * material.UnitPrice;
            }

            if (dto.ActualQuantity.HasValue)
            {
                material.ActualQuantity = dto.ActualQuantity.Value;
                material.ActualAmount = material.ActualQuantity.Value * material.UnitPrice;
            }

            if (!string.IsNullOrEmpty(dto.Notes))
            {
                material.Notes = dto.Notes;
            }

            material.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return MapToMaterialDto(material);
        }

        public async Task<MaterialDto> UpdateActualQuantityAsync(Guid materialId, UpdateActualQuantityDto dto, Guid supervisorId, CancellationToken ct = default)
        {
            var material = await _context.Materials
                .Include(m => m.MaterialRequest)
                .ThenInclude(mr => mr.Project)
                .FirstOrDefaultAsync(m => m.Id == materialId, ct);

            if (material == null)
                throw new ArgumentException("Material not found");

            // Verify supervisor permission
            var participant = await _context.ProjectParticipants
                .FirstOrDefaultAsync(p => p.ProjectId == material.ProjectId &&
                                        p.UserId == supervisorId &&
                                        p.Role == ProjectRole.Supervisor, ct);

            if (participant == null)
                throw new UnauthorizedAccessException("Only supervisors can update actual quantity");

            material.ActualQuantity = dto.ActualQuantity;
            material.ActualAmount = dto.ActualQuantity * material.UnitPrice;
            material.Notes = dto.Notes;
            material.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return MapToMaterialDto(material);
        }

        #endregion

        #region Payment Operations

        public async Task<MaterialPaymentDto> CreatePaymentAsync(CreateMaterialPaymentDto dto, Guid currentUserId, CancellationToken ct = default)
        {
            var material = await _context.Materials
                .Include(m => m.Payments)
                .FirstOrDefaultAsync(m => m.Id == dto.MaterialId, ct);

            if (material == null)
                throw new ArgumentException("Material not found");

            // Calculate remaining after this payment
            var totalPaid = material.Payments.Sum(p => p.PaidQuantity);
            var totalAmount = material.Payments.Sum(p => p.PaidAmount);

            var remainingQty = (material.ActualQuantity ?? material.EstimatedQuantity) - totalPaid - dto.PaidQuantity;
            var remainingAmt = ((material.ActualAmount ?? material.EstimatedAmount) - totalAmount) - (dto.PaidQuantity * material.UnitPrice);

            var payment = new MaterialPayment
            {
                Id = Guid.NewGuid(),
                MaterialId = dto.MaterialId,
                ProjectId = material.ProjectId,
                PaymentDate = dto.PaymentDate,
                PaidQuantity = dto.PaidQuantity,
                PaidAmount = dto.PaidQuantity * material.UnitPrice,
                RemainingQuantity = remainingQty,
                RemainingAmount = remainingAmt,
                PaymentMethod = dto.PaymentMethod,
                InvoiceNumber = dto.InvoiceNumber,
                TransactionReference = dto.TransactionReference,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId.ToString()
            };

            _context.MaterialPayments.Add(payment);
            await _context.SaveChangesAsync(ct);

            return MapToPaymentDto(payment);
        }

        public async Task<List<MaterialPaymentDto>> GetPaymentsByMaterialAsync(Guid materialId, CancellationToken ct = default)
        {
            var payments = await _context.MaterialPayments
                .Where(p => p.MaterialId == materialId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync(ct);

            return payments.Select(p => MapToPaymentDto(p)).ToList();
        }

        public async Task<List<MaterialPaymentDto>> GetPaymentsByProjectAsync(Guid projectId, CancellationToken ct = default)
        {
            var payments = await _context.MaterialPayments
                .Where(p => p.ProjectId == projectId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync(ct);

            return payments.Select(p => MapToPaymentDto(p)).ToList();
        }

        #endregion

        #region Private Helper Methods

        private async Task SendApprovalRequestEmailsAsync(MaterialRequest request, CancellationToken ct)
        {
            var project = await _context.Projects
                .Include(p => p.Homeowner)
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct);

            if (project == null) return;

            // Get chief supervisor
            var supervisor = await _context.ProjectParticipants
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId &&
                                        p.DetailedRole == ProjectParticipantRole.MainSupervisor, ct);

            // TODO: Send emails (will implement with actual email service)
            // await _emailService.SendMaterialApprovalRequestAsync(project.Homeowner.Email, request);
            // if (supervisor != null)
            // {
            //     await _emailService.SendMaterialApprovalRequestAsync(supervisor.User.Email, request);
            // }
        }

        private MaterialRequestDto MapToRequestDto(MaterialRequest request)
        {
            return new MaterialRequestDto
            {
                Id = request.Id,
                ProjectId = request.ProjectId,
                ContractorId = request.ContractorId,
                RequestDate = request.RequestDate,
                Status = request.Status.ToString(),
                StatusLabel = GetStatusLabel(request.Status),
                ApprovedByHomeowner = request.ApprovedByHomeowner,
                ApprovedByHomeownerAt = request.ApprovedByHomeownerAt,
                ApprovedBySupervisor = request.ApprovedBySupervisor,
                ApprovedBySupervisorAt = request.ApprovedBySupervisorAt,
                RejectedAt = request.RejectedAt,
                ProjectDelegatesApprovalToSupervisor = request.Project?.DelegateApprovalToSupervisor ?? false,
                Notes = request.Notes,
                RejectionReason = request.RejectionReason,
                FileName = request.FileName,
                MaterialCount = request.Materials?.Count ?? 0,
                TotalEstimatedAmount = request.Materials?.Sum(m => m.EstimatedAmount) ?? 0,
                CreatedAt = request.CreatedAt
            };
        }

        private async Task<MaterialRequestDetailDto> MapToRequestDetailDto(MaterialRequest request, CancellationToken ct)
        {
            await _context.Entry(request).Collection(r => r.Materials).LoadAsync(ct);
            await _context.Entry(request).Collection(r => r.ApprovalHistories).LoadAsync(ct);

            var dto = new MaterialRequestDetailDto
            {
                Id = request.Id,
                ProjectId = request.ProjectId,
                ContractorId = request.ContractorId,
                RequestDate = request.RequestDate,
                Status = request.Status.ToString(),
                StatusLabel = GetStatusLabel(request.Status),
                ApprovedByHomeowner = request.ApprovedByHomeowner,
                ApprovedByHomeownerAt = request.ApprovedByHomeownerAt,
                ApprovedBySupervisor = request.ApprovedBySupervisor,
                ApprovedBySupervisorAt = request.ApprovedBySupervisorAt,
                RejectedAt = request.RejectedAt,
                Notes = request.Notes,
                RejectionReason = request.RejectionReason,
                FileName = request.FileName,
                MaterialCount = request.Materials?.Count ?? 0,
                TotalEstimatedAmount = request.Materials?.Sum(m => m.EstimatedAmount) ?? 0,
                CreatedAt = request.CreatedAt,
                Materials = request.Materials?.Select(m => MapToMaterialDto(m)).ToList() ?? new List<MaterialDto>(),
                ApprovalHistories = request.ApprovalHistories?.Select(h => MapToApprovalHistoryDto(h)).ToList() ?? new List<MaterialApprovalHistoryDto>()
            };

            return dto;
        }

        private MaterialDto MapToMaterialDto(Material material)
        {
            var totalPaid = material.Payments?.Sum(p => p.PaidQuantity) ?? 0;
            var totalPaidAmount = material.Payments?.Sum(p => p.PaidAmount) ?? 0;
            var remainingQty = (material.ActualQuantity ?? material.EstimatedQuantity) - totalPaid;
            var remainingAmt = (material.ActualAmount ?? material.EstimatedAmount) - totalPaidAmount;

            return new MaterialDto
            {
                Id = material.Id,
                MaterialRequestId = material.MaterialRequestId,
                ProjectId = material.ProjectId,
                WorkItemId = material.WorkItemId,
                Code = material.Code,
                Name = material.Name,
                Unit = material.Unit,
                UnitPrice = material.UnitPrice,
                ContractQuantity = material.ContractQuantity,
                EstimatedQuantity = material.EstimatedQuantity,
                ActualQuantity = material.ActualQuantity,
                ContractAmount = material.ContractAmount,
                EstimatedAmount = material.EstimatedAmount,
                ActualAmount = material.ActualAmount,
                TotalPaidQuantity = totalPaid,
                TotalPaidAmount = totalPaidAmount,
                RemainingQuantity = remainingQty,
                RemainingAmount = remainingAmt,
                Description = material.Description,
                Supplier = material.Supplier,
                Notes = material.Notes,
                SortOrder = material.SortOrder,
                CreatedAt = material.CreatedAt
            };
        }

        private MaterialDetailDto MapToMaterialDetailDto(Material material)
        {
            var dto = MapToMaterialDto(material);
            return new MaterialDetailDto
            {
                Id = dto.Id,
                MaterialRequestId = dto.MaterialRequestId,
                ProjectId = dto.ProjectId,
                WorkItemId = dto.WorkItemId,
                WorkItemName = material.WorkItem?.Name,
                Code = dto.Code,
                Name = dto.Name,
                Unit = dto.Unit,
                UnitPrice = dto.UnitPrice,
                ContractQuantity = dto.ContractQuantity,
                EstimatedQuantity = dto.EstimatedQuantity,
                ActualQuantity = dto.ActualQuantity,
                ContractAmount = dto.ContractAmount,
                EstimatedAmount = dto.EstimatedAmount,
                ActualAmount = dto.ActualAmount,
                TotalPaidQuantity = dto.TotalPaidQuantity,
                TotalPaidAmount = dto.TotalPaidAmount,
                RemainingQuantity = dto.RemainingQuantity,
                RemainingAmount = dto.RemainingAmount,
                Description = dto.Description,
                Supplier = dto.Supplier,
                Notes = dto.Notes,
                SortOrder = dto.SortOrder,
                CreatedAt = dto.CreatedAt,
                Payments = material.Payments?.Select(p => MapToPaymentDto(p)).ToList() ?? new List<MaterialPaymentDto>()
            };
        }

        private MaterialPaymentDto MapToPaymentDto(MaterialPayment payment)
        {
            return new MaterialPaymentDto
            {
                Id = payment.Id,
                MaterialId = payment.MaterialId,
                ProjectId = payment.ProjectId,
                PaymentDate = payment.PaymentDate,
                PaidQuantity = payment.PaidQuantity,
                PaidAmount = payment.PaidAmount,
                RemainingQuantity = payment.RemainingQuantity,
                RemainingAmount = payment.RemainingAmount,
                PaymentMethod = payment.PaymentMethod,
                InvoiceNumber = payment.InvoiceNumber,
                TransactionReference = payment.TransactionReference,
                Notes = payment.Notes,
                ApprovedBy = payment.ApprovedBy,
                ApprovedAt = payment.ApprovedAt,
                CreatedAt = payment.CreatedAt
            };
        }

        private MaterialApprovalHistoryDto MapToApprovalHistoryDto(MaterialApprovalHistory history)
        {
            return new MaterialApprovalHistoryDto
            {
                Id = history.Id,
                MaterialRequestId = history.MaterialRequestId,
                ApprovedById = history.ApprovedById,
                ApproverRole = history.ApproverRole.ToString(),
                Action = history.Action.ToString(),
                ActionDate = history.ActionDate,
                Comments = history.Comments
            };
        }

        private string GetStatusLabel(MaterialRequestStatus status)
        {
            return status switch
            {
                MaterialRequestStatus.Pending => "Chờ phê duyệt",
                MaterialRequestStatus.PartiallyApproved => "Đã duyệt một phần",
                MaterialRequestStatus.Approved => "Đã phê duyệt",
                MaterialRequestStatus.Rejected => "Đã từ chối",
                _ => "Không xác định"
            };
        }

        #endregion
    }
}
