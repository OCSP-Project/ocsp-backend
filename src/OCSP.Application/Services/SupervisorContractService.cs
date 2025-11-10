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
    public class SupervisorContractService : ISupervisorContractService
    {
        private readonly ApplicationDbContext _db;
        private readonly IPdfService _pdfService;
        private readonly IFileService _fileService;
        
        public SupervisorContractService(
            ApplicationDbContext db, 
            IPdfService pdfService,
            IFileService fileService)
        {
            _db = db;
            _pdfService = pdfService;
            _fileService = fileService;
        }

        public async Task<SupervisorContractDto> CreateAsync(Guid projectId, Guid supervisorId, decimal monthlyPrice, CancellationToken ct = default)
        {
            var project = await _db.Projects
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == projectId, ct)
                ?? throw new ArgumentException("Project not found");

            if (project.SupervisorId != supervisorId)
                throw new ArgumentException("Supervisor not assigned to this project");

            var supervisor = project.Supervisor 
                ?? throw new ArgumentException("Supervisor not found");

            // Check if contract already exists
            var existingContract = await _db.SupervisorContracts
                .FirstOrDefaultAsync(sc => sc.ProjectId == projectId && sc.SupervisorId == supervisorId, ct);

            if (existingContract != null)
                return await BuildDtoAsync(existingContract.Id, project.HomeownerId, ct);

            // Load terms from template (will be filled with actual contract template later)
            var terms = LoadSupervisorContractTerms();

            var contract = new SupervisorContract
            {
                ProjectId = projectId,
                SupervisorId = supervisorId,
                SupervisorUserId = supervisor.UserId,
                HomeownerUserId = project.HomeownerId,
                MonthlyPrice = monthlyPrice,
                Terms = terms,
                Status = ContractStatus.Draft
            };

            _db.SupervisorContracts.Add(contract);
            await _db.SaveChangesAsync(ct);

            return await BuildDtoAsync(contract.Id, project.HomeownerId, ct);
        }

        public async Task<SupervisorContractDto> CreateForProjectAsync(Guid projectId, Guid homeownerId, decimal monthlyPrice, CancellationToken ct = default)
        {
            var project = await _db.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId, ct)
                ?? throw new ArgumentException("Project not found");

            if (project.HomeownerId != homeownerId)
                throw new UnauthorizedAccessException("Only project owner can create supervisor contract");

            // Check if contract already exists for this project
            var existingContract = await _db.SupervisorContracts
                .FirstOrDefaultAsync(sc => sc.ProjectId == projectId, ct);

            if (existingContract != null)
                return await BuildDtoAsync(existingContract.Id, homeownerId, ct);

            // Find an available supervisor (but don't assign to project yet - only assign when contract is completed)
            var supervisor = await _db.Supervisors
                .Include(s => s.User)
                .Where(s => s.AvailableNow)
                .OrderBy(_ => Guid.NewGuid())
                .FirstOrDefaultAsync(ct);

            if (supervisor == null)
                throw new InvalidOperationException("Không có giám sát viên sẵn sàng");

            var supervisorId = supervisor.Id;
            var supervisorUser = supervisor.User ?? throw new InvalidOperationException("Supervisor user not found");

            // Load terms from template
            var terms = LoadSupervisorContractTerms();

            var contract = new SupervisorContract
            {
                ProjectId = projectId,
                SupervisorId = supervisorId,
                SupervisorUserId = supervisorUser.Id,
                HomeownerUserId = homeownerId,
                MonthlyPrice = monthlyPrice,
                Terms = terms,
                Status = ContractStatus.Draft
            };

            _db.SupervisorContracts.Add(contract);
            await _db.SaveChangesAsync(ct);

            return await BuildDtoAsync(contract.Id, homeownerId, ct);
        }

        public async Task<SupervisorContractDto> GetByIdAsync(Guid contractId, Guid currentUserId, CancellationToken ct = default)
        {
            return await BuildDtoAsync(contractId, currentUserId, ct);
        }

        public async Task<SupervisorContractDto?> GetByProjectIdAsync(Guid projectId, Guid currentUserId, CancellationToken ct = default)
        {
            var contract = await _db.SupervisorContracts
                .FirstOrDefaultAsync(sc => sc.ProjectId == projectId, ct);

            if (contract == null)
                return null;

            // Check access
            if (contract.HomeownerUserId != currentUserId && contract.SupervisorUserId != currentUserId)
                throw new UnauthorizedAccessException("No access to this contract");

            return await BuildDtoAsync(contract.Id, currentUserId, ct);
        }

        public async Task<IEnumerable<SupervisorContractListItemDto>> ListMyContractsAsync(Guid userId, CancellationToken ct = default)
        {
            var contracts = await _db.SupervisorContracts
                .Include(sc => sc.Project)
                .Include(sc => sc.Supervisor)
                    .ThenInclude(s => s.User)
                .Where(sc => sc.HomeownerUserId == userId || sc.SupervisorUserId == userId)
                .OrderByDescending(sc => sc.CreatedAt) // Mới nhất lên đầu
                .ToListAsync(ct);

            return contracts.Select(sc => new SupervisorContractListItemDto
            {
                Id = sc.Id,
                ProjectId = sc.ProjectId,
                ProjectName = sc.Project.Name,
                SupervisorName = sc.Supervisor.User?.Username ?? "Unknown",
                MonthlyPrice = sc.MonthlyPrice,
                Status = sc.Status.ToString(),
                CreatedAt = sc.CreatedAt
            });
        }

        public async Task<SupervisorContractDto> SignByHomeownerAsync(
            Guid contractId, SignSupervisorContractDto dto, Guid homeownerId, CancellationToken ct = default)
        {
            var contract = await _db.SupervisorContracts
                .Include(sc => sc.Project)
                .FirstOrDefaultAsync(sc => sc.Id == contractId, ct)
                ?? throw new ArgumentException("Supervisor contract not found");

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

            // Change status
            if (contract.Status == ContractStatus.Draft)
            {
                contract.Status = ContractStatus.PendingSignatures;
            }

            // If supervisor already signed, mark as completed
            if (!string.IsNullOrEmpty(contract.SupervisorSignatureBase64))
            {
                contract.Status = ContractStatus.Completed;
                // Assign supervisor to project only when contract is completed
                await AssignSupervisorToProjectAsync(contract.ProjectId, contract.SupervisorId, ct);
            }

            await _db.SaveChangesAsync(ct);
            return await BuildDtoAsync(contract.Id, homeownerId, ct);
        }

        public async Task<SupervisorContractDto> SignBySupervisorAsync(
            Guid contractId, SignSupervisorContractDto dto, Guid supervisorId, CancellationToken ct = default)
        {
            var contract = await _db.SupervisorContracts
                .Include(sc => sc.Project)
                .FirstOrDefaultAsync(sc => sc.Id == contractId, ct)
                ?? throw new ArgumentException("Supervisor contract not found");

            if (contract.SupervisorUserId != supervisorId)
                throw new UnauthorizedAccessException("You are not the supervisor of this contract");

            if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.PendingSignatures)
                throw new InvalidOperationException("Contract cannot be signed at this status");

            // Check if supervisor profile is complete
            var supervisorProfile = await _db.Profiles
                .FirstOrDefaultAsync(p => p.UserId == supervisorId, ct);
            
            if (supervisorProfile == null || 
                string.IsNullOrEmpty(supervisorProfile.FirstName) || 
                string.IsNullOrEmpty(supervisorProfile.LastName) ||
                string.IsNullOrEmpty(supervisorProfile.PhoneNumber) ||
                string.IsNullOrEmpty(supervisorProfile.Address))
            {
                throw new InvalidOperationException("Vui lòng cập nhật đầy đủ thông tin cá nhân (Họ tên, SĐT, Địa chỉ) trước khi ký hợp đồng");
            }

            // Save signature
            contract.SupervisorSignatureBase64 = dto.SignatureBase64;
            contract.SignedBySupervisorAt = DateTime.UtcNow;

            // Change status
            if (contract.Status == ContractStatus.Draft)
            {
                contract.Status = ContractStatus.PendingSignatures;
            }

            // If homeowner already signed, mark as completed
            if (!string.IsNullOrEmpty(contract.HomeownerSignatureBase64))
            {
                contract.Status = ContractStatus.Completed;
                // Assign supervisor to project only when contract is completed
                await AssignSupervisorToProjectAsync(contract.ProjectId, contract.SupervisorId, ct);
            }

            await _db.SaveChangesAsync(ct);
            return await BuildDtoAsync(contract.Id, supervisorId, ct);
        }

        public async Task<byte[]> GeneratePdfAsync(Guid contractId, Guid userId, CancellationToken ct = default)
        {
            var contract = await _db.SupervisorContracts
                .Include(sc => sc.Project)
                .Include(sc => sc.Supervisor)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(sc => sc.Id == contractId, ct)
                ?? throw new ArgumentException("Supervisor contract not found");

            // Check access
            if (contract.HomeownerUserId != userId && contract.SupervisorUserId != userId)
                throw new UnauthorizedAccessException("No access to this contract");

            // Load profiles
            var homeownerProfile = await _db.Profiles
                .FirstOrDefaultAsync(p => p.UserId == contract.HomeownerUserId, ct);
            var supervisorProfile = await _db.Profiles
                .FirstOrDefaultAsync(p => p.UserId == contract.SupervisorUserId, ct);

            if (homeownerProfile == null || supervisorProfile == null || contract.Project == null)
                throw new InvalidOperationException("Missing required data for PDF generation");

            // Generate PDF with signatures if available
            var pdfBytes = await _pdfService.GenerateSupervisorContractPdfAsync(
                contract,
                homeownerProfile,
                supervisorProfile,
                contract.Project,
                contract.HomeownerSignatureBase64,
                contract.SupervisorSignatureBase64);

            return pdfBytes;
        }

        public async Task<SupervisorContractDto> GeneratePdfForContractAsync(Guid contractId, Guid userId, CancellationToken ct = default)
        {
            var contract = await _db.SupervisorContracts
                .Include(sc => sc.Project)
                .Include(sc => sc.Supervisor)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(sc => sc.Id == contractId, ct)
                ?? throw new ArgumentException("Supervisor contract not found");

            // Check access
            if (contract.HomeownerUserId != userId && contract.SupervisorUserId != userId)
                throw new UnauthorizedAccessException("No access to this contract");

            // Generate PDF if not exists or needs update
            if (string.IsNullOrEmpty(contract.TemplatePdfUrl) || 
                (!string.IsNullOrEmpty(contract.HomeownerSignatureBase64) && !string.IsNullOrEmpty(contract.SupervisorSignatureBase64)))
            {
                var homeownerProfile = await _db.Profiles
                    .FirstOrDefaultAsync(p => p.UserId == contract.HomeownerUserId, ct);
                var supervisorProfile = await _db.Profiles
                    .FirstOrDefaultAsync(p => p.UserId == contract.SupervisorUserId, ct);

                if (homeownerProfile != null && supervisorProfile != null && contract.Project != null)
                {
                    var pdfBytes = await _pdfService.GenerateSupervisorContractPdfAsync(
                        contract,
                        homeownerProfile,
                        supervisorProfile,
                        contract.Project,
                        contract.HomeownerSignatureBase64,
                        contract.SupervisorSignatureBase64);

                    // Upload PDF
                    var pdfUrl = await _fileService.UploadFileAsync(
                        new System.IO.MemoryStream(pdfBytes),
                        $"supervisor-contracts/{contract.Id}/{(string.IsNullOrEmpty(contract.HomeownerSignatureBase64) ? "template" : "signed")}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf",
                        "supervisor-contracts");

                    if (string.IsNullOrEmpty(contract.TemplatePdfUrl))
                    {
                        contract.TemplatePdfUrl = pdfUrl;
                    }
                    
                    // Update signed PDF URL if both signatures exist
                    if (!string.IsNullOrEmpty(contract.HomeownerSignatureBase64) && 
                        !string.IsNullOrEmpty(contract.SupervisorSignatureBase64))
                    {
                        contract.SignedPdfUrl = pdfUrl;
                    }

                    await _db.SaveChangesAsync(ct);
                }
            }

            return await BuildDtoAsync(contract.Id, userId, ct);
        }

        private async Task<SupervisorContractDto> BuildDtoAsync(Guid contractId, Guid currentUserId, CancellationToken ct)
        {
            var contract = await _db.SupervisorContracts
                .Include(sc => sc.Project)
                .Include(sc => sc.Supervisor)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(sc => sc.Id == contractId, ct)
                ?? throw new ArgumentException("Supervisor contract not found");

            // Check access
            if (contract.HomeownerUserId != currentUserId && contract.SupervisorUserId != currentUserId)
                throw new UnauthorizedAccessException("No access to this contract");

            var homeownerProfile = await _db.Profiles
                .FirstOrDefaultAsync(p => p.UserId == contract.HomeownerUserId, ct);

            var supervisorProfile = await _db.Profiles
                .FirstOrDefaultAsync(p => p.UserId == contract.SupervisorUserId, ct);

            return new SupervisorContractDto
            {
                Id = contract.Id,
                ProjectId = contract.ProjectId,
                ProjectName = contract.Project.Name,
                SupervisorId = contract.SupervisorId,
                SupervisorUserId = contract.SupervisorUserId,
                SupervisorName = contract.Supervisor.User?.Username ?? "Unknown",
                HomeownerUserId = contract.HomeownerUserId,
                HomeownerName = homeownerProfile != null 
                    ? $"{homeownerProfile.FirstName} {homeownerProfile.LastName}".Trim() 
                    : "Unknown",
                MonthlyPrice = contract.MonthlyPrice,
                Terms = contract.Terms,
                Status = contract.Status.ToString(),
                CreatedAt = contract.CreatedAt,
                UpdatedAt = contract.UpdatedAt,
                HomeownerSignatureBase64 = contract.HomeownerSignatureBase64,
                SupervisorSignatureBase64 = contract.SupervisorSignatureBase64,
                SignedByHomeownerAt = contract.SignedByHomeownerAt,
                SignedBySupervisorAt = contract.SignedBySupervisorAt,
                TemplatePdfUrl = contract.TemplatePdfUrl,
                SignedPdfUrl = contract.SignedPdfUrl
            };
        }


        private async Task AssignSupervisorToProjectAsync(Guid projectId, Guid supervisorId, CancellationToken ct)
        {
            var project = await _db.Projects
                .Include(p => p.Participants)
                .FirstOrDefaultAsync(p => p.Id == projectId, ct)
                ?? throw new ArgumentException("Project not found");

            // Only assign if not already assigned
            if (project.SupervisorId.HasValue && project.SupervisorId.Value == supervisorId)
                return; // Already assigned

            var supervisor = await _db.Supervisors
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == supervisorId, ct)
                ?? throw new ArgumentException("Supervisor not found");

            // Assign supervisor to project
            project.SupervisorId = supervisor.Id;
            supervisor.AvailableNow = false;

            // Add participant if not exists
            var hasSupervisorParticipant = project.Participants.Any(pp => pp.Role == ProjectRole.Supervisor);
            if (!hasSupervisorParticipant)
            {
                project.Participants.Add(new ProjectParticipant
                {
                    ProjectId = project.Id,
                    UserId = supervisor.UserId,
                    Role = ProjectRole.Supervisor,
                    Status = ParticipantStatus.Active,
                    JoinedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync(ct);
        }

        private string LoadSupervisorContractTerms()
        {
            // Return the contract template terms - full template will be filled when generating PDF
            return @"HỢP ĐỒNG TƯ VẤN GIÁM SÁT THI CÔNG XÂY DỰNG CÔNG TRÌNH

Các điều khoản hợp đồng sẽ được hiển thị đầy đủ trong PDF hợp đồng.";
        }
    }
}
