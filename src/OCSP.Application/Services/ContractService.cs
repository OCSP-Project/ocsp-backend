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

namespace OCSP.Application.Services
{
    public class ContractService : IContractService
    {
        private readonly ApplicationDbContext _db;
        public ContractService(ApplicationDbContext db) => _db = db;

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
                    Qty = i.Qty,
                    Unit = i.Unit,
                    UnitPrice = i.UnitPrice
                }).ToList();

            var total = items.Sum(x => x.Qty * x.UnitPrice);

            var contract = new Contract
            {
                ProjectId        = project.Id,
                ProposalId       = proposal.Id,
                HomeownerUserId  = project.HomeownerId,
                ContractorUserId = proposal.ContractorUserId,
                Terms            = (dto.Terms ?? string.Empty).Trim(),
                Status           = ContractStatus.Draft,  // Bắt đầu ở bản nháp
                TotalPrice       = total
            };
            foreach (var it in items)
                contract.Items.Add(it);

            _db.Contracts.Add(contract);
            await _db.SaveChangesAsync(ct);

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
    break;

                case ContractStatus.Completed:
                    // Active -> Completed — homeowner làm
                    if (!isHomeowner)
                        throw new UnauthorizedAccessException("Only homeowner can complete the contract");
                    if (c.Status != ContractStatus.Active)
                        throw new InvalidOperationException("Only Active contracts can be completed");
                    c.Status = ContractStatus.Completed;
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

            return new ContractDetailDto
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
                UpdatedAt        = c.UpdatedAt,
                Items = c.Items.Select(i => new ContractItemDto
                {
                    Id        = i.Id,
                    Name      = i.Name,
                    Qty       = i.Qty,
                    Unit      = i.Unit,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }
    }
}
