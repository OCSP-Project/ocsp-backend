using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using OCSP.Application.DTOs.Contracts;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.ExternalServices.Interfaces;

namespace OCSP.Application.Services
{
    public class ContractService : IContractService
    {
        private readonly ApplicationDbContext _db;
        private readonly IPdfService _pdfService;
        private readonly IFileService _fileService;
        
        public ContractService(
            ApplicationDbContext db, 
            IPdfService pdfService,
            IFileService fileService)
        {
            _db = db;
            _pdfService = pdfService;
            _fileService = fileService;
        }

        public async Task<ContractDetailDto> CreateFromProposalAsync(
            CreateContractDto dto, Guid homeownerId, CancellationToken ct = default)
        {
            var proposal = await _db.Proposals
                .Include(p => p.Items)
                .Include(p => p.QuoteRequest)
                    .ThenInclude(q => q.Project)
                .FirstOrDefaultAsync(p => p.Id == dto.ProposalId, ct)
                ?? throw new ArgumentException("Proposal not found");

            var project = proposal.QuoteRequest.Project;
            if (project.HomeownerId != homeownerId)
                throw new UnauthorizedAccessException("Only project owner can create a contract");

            var items = (dto.Items?.Any() == true)
                ? dto.Items.Select(i => new ContractItem
                {
                    Name = i.Name.Trim(),
                    Qty = i.Qty,
                    Unit = i.Unit.Trim(),
                    UnitPrice = i.UnitPrice
                }).ToList()
                : proposal.Items.Select(i => new ContractItem
                {
                    Name = i.Name,
                    Qty = 1, // Default quantity since Excel doesn't have this
                    Unit = "lần", // Default unit
                    UnitPrice = i.Price
                }).ToList();

            var total = items.Sum(x => x.Qty * x.UnitPrice);

            var contract = new Contract
            {
                ProjectId        = project.Id,
                QuoteRequestId   = proposal.QuoteRequestId,
                ProposalId       = proposal.Id,
                HomeownerUserId  = project.HomeownerId,
                ContractorUserId = proposal.ContractorUserId,
                Terms            = (dto.Terms ?? string.Empty).Trim(),
                Status           = ContractStatus.Draft,  // Bắt đầu ở bản nháp
                TotalPrice       = total,
                DurationDays     = proposal.DurationDays
            };
            foreach (var it in items)
                contract.Items.Add(it);

            _db.Contracts.Add(contract);
            await _db.SaveChangesAsync(ct);

            // Generate PDF template immediately after creating contract
            try
            {
                var homeownerProfile = await _db.Profiles
                    .FirstOrDefaultAsync(p => p.UserId == homeownerId, ct);
                var contractorProfile = await _db.Profiles
                    .FirstOrDefaultAsync(p => p.UserId == contract.ContractorUserId, ct);
                var contractorCompany = await _db.Contractors
                    .FirstOrDefaultAsync(c => c.UserId == contract.ContractorUserId, ct);

                if (homeownerProfile != null && contractorProfile != null)
                {
                    var pdfBytes = await _pdfService.GenerateContractPdfAsync(
                        contract, homeownerProfile, contractorProfile, contractorCompany, proposal);

                    var pdfUrl = await _fileService.UploadFileAsync(
                        new System.IO.MemoryStream(pdfBytes),
                        $"contracts/{contract.Id}/template.pdf",
                        "contracts");

                    contract.TemplatePdfUrl = pdfUrl;
                    await _db.SaveChangesAsync(ct);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail contract creation
                Console.WriteLine($"Error generating PDF template: {ex.Message}");
            }

            return await BuildDetailDtoAsync(contract.Id, homeownerId, ct);
        }

        public async Task<ContractDetailDto> GetByIdAsync(Guid contractId, Guid currentUserId, CancellationToken ct = default)
            => await BuildDetailDtoAsync(contractId, currentUserId, ct);

        public async Task<IEnumerable<ContractListItemDto>> ListByProjectAsync(
            Guid projectId, Guid currentUserId, CancellationToken ct = default)
        {
            var project = await _db.Projects
                .Include(p => p.Participants)
                .FirstOrDefaultAsync(p => p.Id == projectId, ct)
                ?? throw new ArgumentException("Project not found");

            var canView = project.HomeownerId == currentUserId ||
                          project.Participants.Any(x => x.UserId == currentUserId);
            if (!canView) throw new UnauthorizedAccessException("No access to this project");

            var list = await _db.Contracts
                .AsNoTracking()
                .Where(c => c.ProjectId == projectId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(ct);

            var contractorIds = list.Select(l => l.ContractorUserId).Distinct().ToList();
            var contractors = await _db.Users
                .Where(u => contractorIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Username })
                .ToListAsync(ct);

            return list.Select(c => new ContractListItemDto
            {
                Id             = c.Id,
                ProjectId      = c.ProjectId,
                ProjectName    = project.Name,
                ContractorName = contractors.FirstOrDefault(x => x.Id == c.ContractorUserId)?.Username ?? "",
                TotalPrice     = c.TotalPrice,
                Status         = c.Status.ToString(),
                CreatedAt      = c.CreatedAt
            }).ToList();
        }

        public async Task<IEnumerable<ContractListItemDto>> ListMyContractsAsync(Guid currentUserId, CancellationToken ct = default)
        {
            var list = await _db.Contracts
                .AsNoTracking()
                .Include(c => c.Project)
                .Where(c => c.HomeownerUserId == currentUserId || c.ContractorUserId == currentUserId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(ct);

            var contractorIds = list.Select(l => l.ContractorUserId).Distinct().ToList();
            var contractors = await _db.Users
                .Where(u => contractorIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Username })
                .ToListAsync(ct);

            return list.Select(c => new ContractListItemDto
            {
                Id             = c.Id,
                ProjectId      = c.ProjectId,
                ProjectName    = c.Project?.Name ?? "",
                ContractorName = contractors.FirstOrDefault(x => x.Id == c.ContractorUserId)?.Username ?? "",
                TotalPrice     = c.TotalPrice,
                Status         = c.Status.ToString(),
                CreatedAt      = c.CreatedAt
            }).ToList();
        }

        public async Task<ContractDetailDto> GeneratePdfForContractAsync(
            Guid contractId, Guid currentUserId, CancellationToken ct = default)
        {
            var contract = await _db.Contracts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == contractId, ct)
                ?? throw new ArgumentException("Contract not found");

            // Check access
            var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == contract.ProjectId, ct);
            if (project == null) throw new ArgumentException("Project not found");
            
            var canAccess = project.HomeownerId == currentUserId || contract.ContractorUserId == currentUserId;
            if (!canAccess) throw new UnauthorizedAccessException("No access to this contract");

            // Validate homeowner profile before generating PDF
            var homeownerProfile = await _db.Profiles
                .FirstOrDefaultAsync(p => p.UserId == contract.HomeownerUserId, ct);
            
            // Check if homeowner profile is complete
            bool homeownerProfileComplete = homeownerProfile != null && 
                !string.IsNullOrWhiteSpace(homeownerProfile.FirstName) && 
                !string.IsNullOrWhiteSpace(homeownerProfile.LastName) &&
                !string.IsNullOrWhiteSpace(homeownerProfile.PhoneNumber) &&
                !string.IsNullOrWhiteSpace(homeownerProfile.Address);
            
            // If profile is incomplete, throw error (don't generate PDF)
            if (!homeownerProfileComplete)
            {
                throw new InvalidOperationException("HOMEOWNER_PROFILE_MISSING: Chủ nhà chưa cập nhật đầy đủ thông tin cá nhân. Vui lòng yêu cầu chủ nhà cập nhật đầy đủ thông tin (Họ tên, SĐT, Địa chỉ) trong mục Hồ sơ trước khi xem hợp đồng.");
            }
            
            // Generate PDF if not exists
            if (string.IsNullOrEmpty(contract.TemplatePdfUrl))
            {
                var contractorProfile = await _db.Profiles
                    .FirstOrDefaultAsync(p => p.UserId == contract.ContractorUserId, ct);
                var contractorCompany = await _db.Contractors
                    .FirstOrDefaultAsync(c => c.UserId == contract.ContractorUserId, ct);
                
                // Get proposal with items
                var proposal = await _db.Proposals
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.Id == contract.ProposalId, ct);

                if (contractorCompany != null && proposal != null)
                {
                    var pdfBytes = await _pdfService.GenerateContractPdfAsync(
                        contract, homeownerProfile!, contractorProfile, contractorCompany, proposal);

                    var pdfUrl = await _fileService.UploadFileAsync(
                        new System.IO.MemoryStream(pdfBytes),
                        $"contracts/{contract.Id}/template.pdf",
                        "contracts");

                    contract.TemplatePdfUrl = pdfUrl;
                    await _db.SaveChangesAsync(ct);
                }
            }

            return await BuildDetailDtoAsync(contract.Id, currentUserId, ct);
        }

        public async Task<ContractDto> UpdateStatusAsync(
            UpdateContractStatusDto dto, Guid currentUserId, CancellationToken ct = default)
        {
            var c = await _db.Contracts
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == dto.ContractId, ct)
                ?? throw new ArgumentException("Contract not found");

            var isHomeowner  = c.HomeownerUserId  == currentUserId;
            var isContractor = c.ContractorUserId == currentUserId;
            if (!isHomeowner && !isContractor)
                throw new UnauthorizedAccessException("Not your contract");

            // ====== Quy tắc chuyển trạng thái theo enum mới ======
            switch (dto.Status)
            {
                case ContractStatus.PendingSignatures:
                    // Draft -> PendingSignatures (gửi hợp đồng để ký) — homeowner làm
                    if (!isHomeowner)
                        throw new UnauthorizedAccessException("Only homeowner can send for signatures");
                    if (c.Status != ContractStatus.Draft)
                        throw new InvalidOperationException("Only Draft contracts can be sent for signatures");
                    c.Status = ContractStatus.PendingSignatures;
                    break;

                case ContractStatus.Active:
                    // Bước 2: contractor xác nhận để Active
                    if (!isContractor)
                        throw new UnauthorizedAccessException("Only contractor can activate the contract");
                    if (c.Status != ContractStatus.PendingSignatures)
                        throw new InvalidOperationException("Only PendingSignatures can be activated");

                    c.Status = ContractStatus.Active;

                    // Ensure contractor is assigned to project when contract becomes Active
                    await AssignContractorToProjectAsync(c.ProjectId, c.ContractorUserId, ct);
                    break;

                case ContractStatus.Completed:
                    // Active -> Completed — homeowner làm
                    if (!isHomeowner)
                        throw new UnauthorizedAccessException("Only homeowner can complete the contract");
                    if (c.Status != ContractStatus.Active)
                        throw new InvalidOperationException("Only Active contracts can be completed");

                    c.Status = ContractStatus.Completed;

                    // Double‑check contractor assignment when contract is marked Completed
                    await AssignContractorToProjectAsync(c.ProjectId, c.ContractorUserId, ct);
                    break;

                case ContractStatus.Cancelled:
                    // Hủy mọi trạng thái trừ Completed — homeowner làm
                    if (!isHomeowner)
                        throw new UnauthorizedAccessException("Only homeowner can cancel the contract");
                    if (c.Status == ContractStatus.Completed)
                        throw new InvalidOperationException("Cannot cancel a completed contract");
                    c.Status = ContractStatus.Cancelled;
                    break;

                case ContractStatus.Draft:
                    throw new InvalidOperationException("Cannot move contract back to Draft");

                default:
                    throw new InvalidOperationException("Unsupported status transition");
            }
            // =====================================================

            await _db.SaveChangesAsync(ct);

            return new ContractDto
            {
                Id               = c.Id,
                ProposalId       = c.ProposalId,
                ProjectId        = c.ProjectId,
                HomeownerUserId  = c.HomeownerUserId,
                ContractorUserId = c.ContractorUserId,
                Terms            = c.Terms,
                TotalPrice       = c.TotalPrice,
                Status           = c.Status.ToString(),
                CreatedAt        = c.CreatedAt,
                UpdatedAt        = c.UpdatedAt
            };
        }

        private async Task<ContractDetailDto> BuildDetailDtoAsync(Guid id, Guid currentUserId, CancellationToken ct)
        {
            var c = await _db.Contracts
                .Include(x => x.Items)
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new ArgumentException("Contract not found");

            if (c.HomeownerUserId != currentUserId && c.ContractorUserId != currentUserId)
                throw new UnauthorizedAccessException("No access to this contract");

            // Fetch homeowner information
            var homeowner = await _db.Users
                .Where(u => u.Id == c.HomeownerUserId)
                .Select(u => new { u.Username, u.Email })
                .FirstOrDefaultAsync(ct);

            // Fetch homeowner profile information
            var homeownerProfile = await _db.Profiles
                .Where(p => p.UserId == c.HomeownerUserId)
                .Select(p => new { p.FirstName, p.LastName })
                .FirstOrDefaultAsync(ct);

            // Fetch contractor information
            var contractor = await _db.Users
                .Where(u => u.Id == c.ContractorUserId)
                .Select(u => new { u.Username, u.Email })
                .FirstOrDefaultAsync(ct);

            // Fetch contractor profile information
            var contractorProfile = await _db.Profiles
                .Where(p => p.UserId == c.ContractorUserId)
                .Select(p => new { p.FirstName, p.LastName })
                .FirstOrDefaultAsync(ct);

            // Fetch contractor business information
            var contractorBusiness = await _db.Contractors
                .Where(ctr => ctr.UserId == c.ContractorUserId)
                .Select(ctr => new
                {
                    ctr.CompanyName,
                    ctr.ContactPhone,
                    ctr.ContactEmail,
                    ctr.Address,
                    ctr.City,
                    ctr.Province,
                    ctr.YearsOfExperience,
                    ctr.TeamSize,
                    ctr.AverageRating,
                    ctr.TotalReviews,
                    ctr.CompletedProjects,
                    ctr.IsVerified,
                    ctr.IsPremium
                })
                .FirstOrDefaultAsync(ct);

            return new ContractDetailDto
            {
                Id               = c.Id,
                ProposalId       = c.ProposalId,
                ProjectId        = c.ProjectId,
                HomeownerUserId  = c.HomeownerUserId,
                ContractorUserId = c.ContractorUserId,
                Terms            = c.Terms,
                TotalPrice       = c.TotalPrice,
                DurationDays     = c.DurationDays,
                Status           = c.Status.ToString(),
                CreatedAt        = c.CreatedAt,
                UpdatedAt        = c.UpdatedAt,
                HomeownerSignatureBase64 = c.HomeownerSignatureBase64,
                ContractorSignatureBase64 = c.ContractorSignatureBase64,
                SignedByHomeownerAt = c.SignedByHomeownerAt,
                SignedByContractorAt = c.SignedByContractorAt,
                TemplatePdfUrl = c.TemplatePdfUrl,
                SignedPdfUrl = c.SignedPdfUrl,
                Items = c.Items.Select(i => new ContractItemDto
                {
                    Id        = i.Id,
                    Name      = i.Name,
                    Qty       = i.Qty,
                    Unit      = i.Unit,
                    UnitPrice = i.UnitPrice
                }).ToList(),
                Homeowner = homeowner != null ? new HomeownerInfoDto
                {
                    Username  = homeowner.Username,
                    Email     = homeowner.Email,
                    FirstName = homeownerProfile?.FirstName,
                    LastName  = homeownerProfile?.LastName
                } : null,
                Contractor = contractor != null && contractorBusiness != null ? new ContractorInfoDto
                {
                    Username          = contractor.Username,
                    Email             = contractor.Email,
                    FirstName         = contractorProfile?.FirstName,
                    LastName          = contractorProfile?.LastName,
                    CompanyName       = contractorBusiness.CompanyName,
                    ContactPhone      = contractorBusiness.ContactPhone,
                    ContactEmail      = contractorBusiness.ContactEmail,
                    Address           = contractorBusiness.Address,
                    City              = contractorBusiness.City,
                    Province          = contractorBusiness.Province,
                    YearsOfExperience = contractorBusiness.YearsOfExperience,
                    TeamSize          = contractorBusiness.TeamSize,
                    AverageRating     = contractorBusiness.AverageRating,
                    TotalReviews      = contractorBusiness.TotalReviews,
                    CompletedProjects = contractorBusiness.CompletedProjects,
                    IsVerified        = contractorBusiness.IsVerified,
                    IsPremium         = contractorBusiness.IsPremium
                } : null
            };
        }

        public async Task<ContractDetailDto> SignByHomeownerAsync(
            Guid contractId, SignContractDto dto, Guid homeownerId, CancellationToken ct = default)
        {
            var contract = await _db.Contracts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == contractId, ct)
                ?? throw new ArgumentException("Contract not found");

            if (contract.HomeownerUserId != homeownerId)
                throw new UnauthorizedAccessException("You are not the homeowner of this contract");

            if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.PendingSignatures)
                throw new InvalidOperationException("Contract cannot be signed at this status");

            // Check if homeowner profile is complete
            var homeownerProfile = await _db.Profiles
                .FirstOrDefaultAsync(p => p.UserId == homeownerId, ct);
            
            if (homeownerProfile == null || 
                string.IsNullOrEmpty(homeownerProfile.FirstName) || 
                string.IsNullOrEmpty(homeownerProfile.LastName) ||
                string.IsNullOrEmpty(homeownerProfile.PhoneNumber) ||
                string.IsNullOrEmpty(homeownerProfile.Address))
            {
                throw new InvalidOperationException("Vui lòng cập nhật đầy đủ thông tin cá nhân (Họ tên, SĐT, Địa chỉ) trước khi ký hợp đồng");
            }

            // Save signature
            contract.HomeownerSignatureBase64 = dto.SignatureBase64;
            contract.SignedByHomeownerAt = DateTime.UtcNow;

            // PDF should already exist from contract creation
            if (string.IsNullOrEmpty(contract.TemplatePdfUrl))
            {
                throw new InvalidOperationException("Contract PDF template not found. Please contact support.");
            }

            // Change status to PendingSignatures if both haven't signed yet
            if (contract.Status == ContractStatus.Draft)
            {
                contract.Status = ContractStatus.PendingSignatures;
            }

            // If contractor already signed, generate final PDF and mark as completed
            if (!string.IsNullOrEmpty(contract.ContractorSignatureBase64))
            {
                await GenerateFinalSignedPdfAsync(contract, ct);
                contract.Status = ContractStatus.Completed; // Changed from Active to Completed
            }

            await _db.SaveChangesAsync(ct);
            return await BuildDetailDtoAsync(contract.Id, homeownerId, ct);
        }

        public async Task<ContractDetailDto> SignByContractorAsync(
            Guid contractId, SignContractDto dto, Guid contractorId, CancellationToken ct = default)
        {
            var contract = await _db.Contracts
                .Include(c => c.Items)
                .Include(c => c.Project)
                .FirstOrDefaultAsync(c => c.Id == contractId, ct)
                ?? throw new ArgumentException("Contract not found");

            if (contract.ContractorUserId != contractorId)
                throw new UnauthorizedAccessException("You are not the contractor of this contract");

            if (contract.Status != ContractStatus.PendingSignatures)
                throw new InvalidOperationException("Contract must be signed by homeowner first");

            // Note: Contractor info comes from ContractorBusiness table (set during registration)
            // No need to validate contractor Profile - contractor info is always available

            // Save signature
            contract.ContractorSignatureBase64 = dto.SignatureBase64;
            contract.SignedByContractorAt = DateTime.UtcNow;

            // Generate final signed PDF
            await GenerateFinalSignedPdfAsync(contract, ct);

            // Mark contract as completed (both parties have signed)
            contract.Status = ContractStatus.Completed;

            // When contract is completed, assign contractor to project
            // so that Project.ContractorId and Participants are updated
            await AssignContractorToProjectAsync(contract.ProjectId, contract.ContractorUserId, ct);

            await _db.SaveChangesAsync(ct);
            return await BuildDetailDtoAsync(contract.Id, contractorId, ct);
        }

        private async Task AssignContractorToProjectAsync(Guid projectId, Guid contractorUserId, CancellationToken ct)
        {
            var project = await _db.Projects
                .Include(p => p.Participants)
                .FirstOrDefaultAsync(p => p.Id == projectId, ct)
                ?? throw new ArgumentException("Project not found");

            var contractor = await _db.Contractors
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == contractorUserId, ct)
                ?? throw new ArgumentException("Contractor not found");

            // Only assign if not already assigned
            if (project.ContractorId.HasValue && project.ContractorId.Value == contractor.Id)
                return;

            project.ContractorId = contractor.Id;

            // Add Contractor as project participant if not exists
            var hasContractorParticipant = project.Participants
                .Any(pp => pp.Role == ProjectRole.Contractor && pp.UserId == contractor.UserId);

            if (!hasContractorParticipant)
            {
                project.Participants.Add(new ProjectParticipant
                {
                    ProjectId = project.Id,
                    UserId = contractor.UserId,
                    Role = ProjectRole.Contractor,
                    Status = ParticipantStatus.Active,
                    JoinedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task<byte[]> GetContractPdfAsync(Guid contractId, Guid currentUserId, CancellationToken ct = default)
        {
            var contract = await _db.Contracts
                .FirstOrDefaultAsync(c => c.Id == contractId, ct)
                ?? throw new ArgumentException("Contract not found");

            if (contract.HomeownerUserId != currentUserId && contract.ContractorUserId != currentUserId)
                throw new UnauthorizedAccessException("No access to this contract");

            // Check if both parties have signed
            bool bothSigned = !string.IsNullOrEmpty(contract.HomeownerSignatureBase64) 
                           && !string.IsNullOrEmpty(contract.ContractorSignatureBase64);

            // Return signed PDF if available and both have signed
            // Signed PDF is FROZEN - never regenerate after both parties have signed
            if (bothSigned)
            {
                if (!string.IsNullOrEmpty(contract.SignedPdfUrl))
                {
                    try
                    {
                        return await _fileService.GetFileAsync(contract.SignedPdfUrl);
                    }
                    catch (Exception ex)
                    {
                        // Signed PDF file is missing - this should not happen
                        // Throw error instead of regenerating to preserve contract integrity
                        throw new InvalidOperationException($"Signed PDF file not found for contract {contract.Id}. Please contact support.");
                    }
                }
                else
                {
                    // Signed PDF URL is missing - this should not happen if contract is completed
                    // Throw error instead of regenerating to preserve contract integrity
                    throw new InvalidOperationException($"Signed PDF URL not found for completed contract {contract.Id}. Please contact support.");
                }
            }

            // If not both signed yet, always regenerate template PDF to ensure it uses latest profile information
            // This ensures that if user updates their profile, the template PDF reflects the changes
            // Template PDF is regenerated each time to keep it up-to-date until contract is signed
            return await GenerateTemplatePdfBytesAsync(contract, ct);
        }

        private async Task<byte[]> GenerateTemplatePdfBytesAsync(Contract contract, CancellationToken ct)
        {
            var homeownerProfile = await _db.Profiles
                .FirstOrDefaultAsync(p => p.UserId == contract.HomeownerUserId, ct);
            var contractorProfile = await _db.Profiles
                .FirstOrDefaultAsync(p => p.UserId == contract.ContractorUserId, ct);
            var contractorCompany = await _db.Contractors
                .FirstOrDefaultAsync(c => c.UserId == contract.ContractorUserId, ct);
            
            // Get proposal with items
            var proposal = await _db.Proposals
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == contract.ProposalId, ct);

            // Check for missing profiles and provide clear error messages
            if (homeownerProfile == null)
                throw new InvalidOperationException("HOMEOWNER_PROFILE_MISSING: Chủ nhà chưa cập nhật thông tin cá nhân. Vui lòng yêu cầu chủ nhà cập nhật đầy đủ thông tin (Họ tên, SĐT, Địa chỉ) trong mục Hồ sơ trước khi xem hợp đồng.");

            // Validate homeowner profile fields
            if (string.IsNullOrWhiteSpace(homeownerProfile.FirstName) || 
                string.IsNullOrWhiteSpace(homeownerProfile.LastName) ||
                string.IsNullOrWhiteSpace(homeownerProfile.PhoneNumber) ||
                string.IsNullOrWhiteSpace(homeownerProfile.Address))
            {
                throw new InvalidOperationException("HOMEOWNER_PROFILE_MISSING: Chủ nhà chưa cập nhật đầy đủ thông tin cá nhân. Vui lòng yêu cầu chủ nhà cập nhật đầy đủ thông tin (Họ tên, SĐT, Địa chỉ) trong mục Hồ sơ trước khi xem hợp đồng.");
            }

            // For contractor: we can use either contractorCompany (preferred) or contractorProfile
            // PDF generation logic already handles this by preferring contractorCompany over contractorProfile
            // So we only need to check if at least one exists
            if (contractorCompany == null && contractorProfile == null)
                throw new InvalidOperationException("CONTRACTOR_INFO_MISSING: Không tìm thấy thông tin nhà thầu. Vui lòng liên hệ hỗ trợ.");

            if (proposal == null)
                throw new InvalidOperationException("Proposal not found for contract PDF.");

            // Pass contractorProfile even if null - PdfService will use contractorCompany if available
            return await _pdfService.GenerateContractPdfAsync(
                contract, homeownerProfile, contractorProfile, contractorCompany, proposal);
        }

        private async Task GenerateFinalSignedPdfAsync(Contract contract, CancellationToken ct)
        {
            // Generate new PDF with signatures embedded directly in the table
            var homeownerProfile = await _db.Profiles
                .FirstOrDefaultAsync(p => p.UserId == contract.HomeownerUserId, ct);
            var contractorProfile = await _db.Profiles
                .FirstOrDefaultAsync(p => p.UserId == contract.ContractorUserId, ct);
            var contractorCompany = await _db.Contractors
                .FirstOrDefaultAsync(c => c.UserId == contract.ContractorUserId, ct);
            
            // Get proposal with items
            var proposal = await _db.Proposals
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == contract.ProposalId, ct);

            // Check for missing profiles and provide clear error messages
            if (homeownerProfile == null)
                throw new InvalidOperationException("HOMEOWNER_PROFILE_MISSING: Chủ nhà chưa cập nhật thông tin cá nhân. Vui lòng yêu cầu chủ nhà cập nhật đầy đủ thông tin (Họ tên, SĐT, Địa chỉ) trong mục Hồ sơ trước khi xem hợp đồng.");

            // Validate homeowner profile fields
            if (string.IsNullOrWhiteSpace(homeownerProfile.FirstName) || 
                string.IsNullOrWhiteSpace(homeownerProfile.LastName) ||
                string.IsNullOrWhiteSpace(homeownerProfile.PhoneNumber) ||
                string.IsNullOrWhiteSpace(homeownerProfile.Address))
            {
                throw new InvalidOperationException("HOMEOWNER_PROFILE_MISSING: Chủ nhà chưa cập nhật đầy đủ thông tin cá nhân. Vui lòng yêu cầu chủ nhà cập nhật đầy đủ thông tin (Họ tên, SĐT, Địa chỉ) trong mục Hồ sơ trước khi xem hợp đồng.");
            }

            // For contractor: we can use either contractorCompany (preferred) or contractorProfile
            // PDF generation logic already handles this by preferring contractorCompany over contractorProfile
            // So we only need to check if at least one exists
            if (contractorCompany == null && contractorProfile == null)
                throw new InvalidOperationException("CONTRACTOR_INFO_MISSING: Không tìm thấy thông tin nhà thầu. Vui lòng liên hệ hỗ trợ.");

            if (proposal == null)
                throw new InvalidOperationException("Proposal not found for contract PDF.");

            // Generate PDF with signatures embedded in the signature table
            var signedPdfBytes = await _pdfService.GenerateContractPdfAsync(
                contract, 
                homeownerProfile!, 
                contractorProfile!, 
                contractorCompany, 
                proposal!,
                contract.HomeownerSignatureBase64,   // Pass signatures
                contract.ContractorSignatureBase64);

            // Upload signed PDF
            var signedPdfUrl = await _fileService.UploadFileAsync(
                new System.IO.MemoryStream(signedPdfBytes),
                $"contracts/{contract.Id}/signed_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf",
                "contracts");

            contract.SignedPdfUrl = signedPdfUrl;
        }
    }
}
