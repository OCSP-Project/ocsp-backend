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

        public MaterialService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
            // STT | Mã số | Hạng mục | Đơn vị | Đơn giá | Khối lượng theo HĐ | Khối lượng theo NKTC | ...
            for (int row = 2; row <= rowCount; row++)
            {
                var code = worksheet.Cells[row, 2].Text?.Trim(); // Mã số
                var name = worksheet.Cells[row, 3].Text?.Trim(); // Hạng mục

                if (string.IsNullOrEmpty(name)) continue;

                var material = new Material
                {
                    Id = Guid.NewGuid(),
                    MaterialRequestId = requestId,
                    ProjectId = request.ProjectId,
                    Code = code ?? $"MAT{sortOrder:D5}",
                    Name = name,
                    Unit = worksheet.Cells[row, 4].Text?.Trim() ?? "m³", // Đơn vị
                    SortOrder = sortOrder++,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Parse unit price (Đơn giá - column 5)
                if (decimal.TryParse(worksheet.Cells[row, 5].Text?.Replace(",", ""), out decimal unitPrice))
                {
                    material.UnitPrice = unitPrice;
                }

                // Parse contract quantity (Khối lượng theo HĐ - column 6)
                if (decimal.TryParse(worksheet.Cells[row, 6].Text?.Replace(",", ""), out decimal contractQty))
                {
                    material.ContractQuantity = contractQty;
                    material.ContractAmount = contractQty * material.UnitPrice;
                }

                // Parse estimated quantity (Khối lượng theo NKTC - column 7)
                if (decimal.TryParse(worksheet.Cells[row, 7].Text?.Replace(",", ""), out decimal estimatedQty))
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

            return await MapToRequestDetailDto(request, ct);
        }

        public async Task<MaterialRequestDetailDto> RejectRequestAsync(Guid requestId, Guid userId, RejectMaterialRequestDto dto, CancellationToken ct = default)
        {
            var request = await _context.MaterialRequests
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.Id == requestId, ct);

            if (request == null)
                throw new ArgumentException("Material request not found");

            // Determine approver role
            ApproverRole role;
            if (request.Project.HomeownerId == userId)
            {
                role = ApproverRole.Homeowner;
            }
            else
            {
                var participant = await _context.ProjectParticipants
                    .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId &&
                                            p.UserId == userId &&
                                            p.DetailedRole == ProjectParticipantRole.MainSupervisor, ct);
                if (participant == null)
                    throw new UnauthorizedAccessException("Only homeowner or chief supervisor can reject");

                role = ApproverRole.Supervisor;
            }

            request.Status = MaterialRequestStatus.Rejected;
            request.RejectionReason = dto.Reason;

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

            return await MapToRequestDetailDto(request, ct);
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
