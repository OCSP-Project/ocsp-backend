using Microsoft.EntityFrameworkCore;
using OCSP.Application.DTOs.Proposals;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace OCSP.Application.Services
{
    public class ProposalService : IProposalService
    {
        private readonly ApplicationDbContext _db;
        public ProposalService(ApplicationDbContext db) => _db = db;

        public async Task<ProposalDto> CreateAsync(CreateProposalDto dto, Guid contractorUserId, CancellationToken ct = default)
        {
            // Quote phải tồn tại, đã Sent và contractor được mời
            var qr = await _db.QuoteRequests
                .Include(q => q.Invites)
                .Include(q => q.Project)
                .FirstOrDefaultAsync(q => q.Id == dto.QuoteRequestId, ct)
                ?? throw new ArgumentException("Quote request not found");

            if (qr.Status != Domain.Enums.QuoteStatus.Sent)
                throw new InvalidOperationException("QuoteRequest must be Sent");

            var invited = qr.Invites.Any(i => i.ContractorUserId == contractorUserId);
            if (!invited) throw new UnauthorizedAccessException("You are not invited to this quote");

            var exists = await _db.Proposals.AnyAsync(p =>
                p.QuoteRequestId == dto.QuoteRequestId &&
                p.ContractorUserId == contractorUserId, ct);
            if (exists) throw new InvalidOperationException("You already submitted a proposal for this quote");

            // Tính tổng từ items
            var total = dto.Items.Sum(i => i.Price);

            var p = new Proposal
            {
                QuoteRequestId = dto.QuoteRequestId,
                ContractorUserId = contractorUserId,
                Status = ProposalStatus.Draft,
                DurationDays = dto.DurationDays,
                TermsSummary = dto.TermsSummary,
                PriceTotal = total,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            foreach (var it in dto.Items)
            {
                p.Items.Add(new ProposalItem
                {
                    Name = it.Name,
                    Price = it.Price,
                    Notes = it.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            _db.Proposals.Add(p);
            await _db.SaveChangesAsync(ct);

            return ToDto(p);
        }

        public async Task<string> UploadExcelAsync(Guid quoteId, Guid contractorUserId, IFormFile excelFile, CancellationToken ct = default)
        {
            // Validate quote, status, and invite
            var qr = await _db.QuoteRequests
                .Include(q => q.Invites)
                .FirstOrDefaultAsync(q => q.Id == quoteId, ct)
                ?? throw new ArgumentException("Quote request not found");
            if (qr.Status != QuoteStatus.Sent)
                throw new InvalidOperationException("QuoteRequest must be Sent");
            if (!qr.Invites.Any(i => i.ContractorUserId == contractorUserId))
                throw new UnauthorizedAccessException("You are not invited to this quote");

            // Basic validation
            var ext = Path.GetExtension(excelFile.FileName).ToLowerInvariant();
            if (ext != ".xlsx") throw new InvalidOperationException("Only .xlsx files are accepted");
            if (excelFile.Length == 0) throw new InvalidOperationException("Empty file");

            // Check if proposal already exists
            var existingProposal = await _db.Proposals.FirstOrDefaultAsync(p =>
                p.QuoteRequestId == quoteId && p.ContractorUserId == contractorUserId, ct);
            
            if (existingProposal != null)
            {
                // Update existing proposal
                await UpdateProposalFromExcelAsync(existingProposal, excelFile, ct);
                return $"Updated existing proposal from {excelFile.FileName}";
            }
            else
            {
                // Create new proposal from Excel
                var proposal = await CreateProposalFromExcelAsync(quoteId, contractorUserId, excelFile, ct);
                return $"Created new proposal from {excelFile.FileName}";
            }
        }

        private async Task<Proposal> CreateProposalFromExcelAsync(Guid quoteId, Guid contractorUserId, IFormFile excelFile, CancellationToken ct)
        {
            // Parse Excel file
            var parser = new ExcelProposalParser();
            using var stream = excelFile.OpenReadStream();
            var parsedData = await parser.ParseExcelAsync(stream);

            // Create proposal with data from "Tổng hợp" tab
            var proposal = new Proposal
            {
                QuoteRequestId = quoteId,
                ContractorUserId = contractorUserId,
                Status = ProposalStatus.Draft,
                PriceTotal = parsedData.TotalCost,
                DurationDays = parsedData.TotalDurationDays,
                TermsSummary = BuildProjectInfoSummary(parsedData),
                IsFromExcel = true,
                ExcelFileName = excelFile.FileName,
                
                // Project Information from Excel
                ProjectTitle = parsedData.ProjectTitle,
                ConstructionArea = parsedData.GeneralInfo.TryGetValue("ConstructionArea", out var area) ? area?.ToString() : null,
                ConstructionTime = parsedData.GeneralInfo.TryGetValue("ConstructionTime", out var time) ? time?.ToString() : null,
                NumberOfWorkers = parsedData.GeneralInfo.TryGetValue("NumberOfWorkers", out var workers) ? workers?.ToString() : null,
                AverageSalary = parsedData.GeneralInfo.TryGetValue("AverageSalary", out var salary) ? salary?.ToString() : null
            };

            _db.Proposals.Add(proposal);
            await _db.SaveChangesAsync(ct);

            // Add cost items from "Tổng hợp" tab as proposal items
            foreach (var costItemData in parsedData.CostItems)
            {
                var proposalItem = new ProposalItem
                {
                    ProposalId = proposal.Id,
                    Name = costItemData.Name,
                    Price = costItemData.TotalAmount,
                    Notes = costItemData.Notes
                };
                _db.ProposalItems.Add(proposalItem);
            }

            await _db.SaveChangesAsync(ct);
            return proposal;
        }

        private async Task UpdateProposalFromExcelAsync(Proposal proposal, IFormFile excelFile, CancellationToken ct)
        {
            // Parse Excel file
            var parser = new ExcelProposalParser();
            using var stream = excelFile.OpenReadStream();
            var parsedData = await parser.ParseExcelAsync(stream);

            // Update proposal basic info
            proposal.PriceTotal = parsedData.TotalCost;
            proposal.DurationDays = parsedData.TotalDurationDays;
            proposal.ExcelFileName = excelFile.FileName;

            // Update terms summary
            proposal.TermsSummary = $"Dự án: {parsedData.ProjectTitle}";

            // Remove existing proposal items
            var existingItems = await _db.ProposalItems.Where(i => i.ProposalId == proposal.Id).ToListAsync(ct);
            _db.ProposalItems.RemoveRange(existingItems);

            // Add cost items from "Tổng hợp" tab as proposal items
            foreach (var costItemData in parsedData.CostItems)
            {
                var proposalItem = new ProposalItem
                {
                    ProposalId = proposal.Id,
                    Name = costItemData.Name,
                    Price = costItemData.TotalAmount,
                    Notes = costItemData.Notes
                };
                _db.ProposalItems.Add(proposalItem);
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task SubmitAsync(Guid proposalId, Guid contractorUserId, CancellationToken ct = default)
        {
            var p = await _db.Proposals
                .Include(x => x.QuoteRequest)
                .FirstOrDefaultAsync(x => x.Id == proposalId, ct)
                ?? throw new ArgumentException("Proposal not found");

            if (p.ContractorUserId != contractorUserId)
                throw new UnauthorizedAccessException("Not your proposal");

            if (p.Status != ProposalStatus.Draft)
                throw new InvalidOperationException("Only Draft proposal can be submitted");

            p.Status = ProposalStatus.Submitted;
            p.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<ProposalDto>> ListByQuoteAsync(Guid quoteId, Guid homeownerId, CancellationToken ct = default)
        {
            var qr = await _db.QuoteRequests
                .Include(q => q.Project)
                .FirstOrDefaultAsync(q => q.Id == quoteId, ct)
                ?? throw new ArgumentException("Quote request not found");

            if (qr.Project.HomeownerId != homeownerId)
                throw new UnauthorizedAccessException("Not project owner");

            var list = await _db.Proposals
                .AsNoTracking()
                .Include(p => p.Items)
                .Where(p => p.QuoteRequestId == quoteId)
                .OrderBy(p => p.PriceTotal)
                .ToListAsync(ct);

            // Load contractor information for each proposal
            var contractorUserIds = list.Select(p => p.ContractorUserId).Distinct().ToList();
            var contractors = await _db.Contractors
                .AsNoTracking()
                .Where(c => contractorUserIds.Contains(c.UserId))
                .Select(c => new { c.UserId, c.CompanyName, c.ContactPhone, c.ContactEmail })
                .ToListAsync(ct);

            // Load user profiles for contact person names
            var profiles = await _db.Profiles
                .AsNoTracking()
                .Where(p => contractorUserIds.Contains(p.UserId))
                .Select(p => new { p.UserId, p.FirstName, p.LastName })
                .ToListAsync(ct);

            var contractorByUserId = contractors.ToDictionary(c => c.UserId, c => c);
            var profileByUserId = profiles.ToDictionary(p => p.UserId, p => p);

            return list.Select(p => ToDtoWithContractor(p, contractorByUserId.GetValueOrDefault(p.ContractorUserId), profileByUserId.GetValueOrDefault(p.ContractorUserId)));
        }

        public async Task<ProposalDto> GetMyByIdAsync(Guid id, Guid contractorUserId, CancellationToken ct = default)
        {
            var p = await _db.Proposals
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new ArgumentException("Proposal not found");
            if (p.ContractorUserId != contractorUserId)
                throw new UnauthorizedAccessException("Not your proposal");
            return ToDto(p);
        }

        public async Task<ProposalDto?> GetMyByQuoteAsync(Guid quoteId, Guid contractorUserId, CancellationToken ct = default)
        {
            var p = await _db.Proposals
                .Include(x => x.Items)
                .Where(x => x.QuoteRequestId == quoteId && x.ContractorUserId == contractorUserId)
                .FirstOrDefaultAsync(ct);
            return p == null ? null : ToDto(p);
        }

        public async Task<ProposalDto> UpdateDraftAsync(Guid id, UpdateProposalDto dto, Guid contractorUserId, CancellationToken ct = default)
        {
            var p = await _db.Proposals
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new ArgumentException("Proposal not found");
            if (p.ContractorUserId != contractorUserId)
                throw new UnauthorizedAccessException("Not your proposal");
            if (p.Status != ProposalStatus.Draft)
                throw new InvalidOperationException("Only Draft can be updated");

            // Update scalar fields
            p.DurationDays = dto.DurationDays;
            p.TermsSummary = dto.TermsSummary;
            p.PriceTotal = dto.Items.Sum(i => i.Price);
            p.UpdatedAt = DateTime.UtcNow;

            // Replace items (simple approach)
            _db.ProposalItems.RemoveRange(p.Items);
            p.Items.Clear();
            foreach (var it in dto.Items)
            {
                p.Items.Add(new ProposalItem
                {
                    Name = it.Name,
                    Price = it.Price,
                    Notes = it.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync(ct);
            return ToDto(p);
        }

        public async Task AcceptAsync(Guid proposalId, Guid homeownerId, CancellationToken ct = default)
        {
            var selected = await _db.Proposals
                .Include(p => p.QuoteRequest)
                    .ThenInclude(q => q.Project)
                .FirstOrDefaultAsync(p => p.Id == proposalId, ct)
                ?? throw new ArgumentException("Proposal not found");

            if (selected.QuoteRequest.Project.HomeownerId != homeownerId)
                throw new UnauthorizedAccessException("Not project owner");

            if (selected.Status != ProposalStatus.Submitted)
                throw new InvalidOperationException("Only Submitted proposal can be accepted");

            using var tx = await _db.Database.BeginTransactionAsync(ct);

            // Set accepted
            selected.Status = ProposalStatus.Accepted;
            selected.UpdatedAt = DateTime.UtcNow;

            // Reject các proposal khác của cùng quote
            await _db.Proposals
                .Where(p => p.QuoteRequestId == selected.QuoteRequestId && p.Id != proposalId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, ProposalStatus.Rejected)
                                          .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

            // Đóng quote
            selected.QuoteRequest.Status = Domain.Enums.QuoteStatus.Closed;
            selected.QuoteRequest.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        private static ProposalDto ToDto(Proposal p) => new ProposalDto
        {
            Id = p.Id,
            QuoteRequestId = p.QuoteRequestId,
            ContractorUserId = p.ContractorUserId,
            Status = p.Status.ToString(),
            PriceTotal = p.PriceTotal,
            DurationDays = p.DurationDays,
            TermsSummary = p.TermsSummary,
            Items = p.Items.OrderBy(i => ExtractOrderFromName(i.Name)).Select(i => new ProposalItemDto
            {
                Name = i.Name,
                Price = i.Price,
                Notes = i.Notes
            }).ToList(),
            IsFromExcel = p.IsFromExcel,
            ExcelFileName = p.ExcelFileName,
            
            // Project Information from Excel
            ProjectTitle = p.ProjectTitle,
            ConstructionArea = p.ConstructionArea,
            ConstructionTime = p.ConstructionTime,
            NumberOfWorkers = p.NumberOfWorkers,
            AverageSalary = p.AverageSalary
        };

        private static ProposalDto ToDtoWithContractor(Proposal p, dynamic? contractorInfo, dynamic? profileInfo) => new ProposalDto
        {
            Id = p.Id,
            QuoteRequestId = p.QuoteRequestId,
            ContractorUserId = p.ContractorUserId,
            Status = p.Status.ToString(),
            PriceTotal = p.PriceTotal,
            DurationDays = p.DurationDays,
            TermsSummary = p.TermsSummary,
            Items = p.Items.OrderBy(i => ExtractOrderFromName(i.Name)).Select(i => new ProposalItemDto
            {
                Name = i.Name,
                Price = i.Price,
                Notes = i.Notes
            }).ToList(),
            IsFromExcel = p.IsFromExcel,
            ExcelFileName = p.ExcelFileName,
            
            // Project Information from Excel
            ProjectTitle = p.ProjectTitle,
            ConstructionArea = p.ConstructionArea,
            ConstructionTime = p.ConstructionTime,
            NumberOfWorkers = p.NumberOfWorkers,
            AverageSalary = p.AverageSalary,
            Contractor = contractorInfo != null ? new ProposalContractorSummaryDto 
            {
                CompanyName = contractorInfo.CompanyName ?? "",
                ContactPerson = profileInfo != null ? $"{profileInfo.FirstName ?? ""} {profileInfo.LastName ?? ""}".Trim() : "",
                Phone = contractorInfo.ContactPhone ?? "",
                Email = contractorInfo.ContactEmail ?? ""
            } : null
        };

        private static int ExtractOrderFromName(string name)
        {
            var match = System.Text.RegularExpressions.Regex.Match(name, @"^(\d+)\.\s*(.+)$");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int order))
            {
                return order;
            }
            return 999; // Put items without order at the end
        }

        private static string BuildProjectInfoSummary(ExcelProposalParser.ParsedProposalData parsedData)
        {
            var info = new List<string>();
            
            if (!string.IsNullOrEmpty(parsedData.ProjectTitle))
            {
                info.Add($"Dự án: {parsedData.ProjectTitle}");
            }
            
            if (parsedData.GeneralInfo.TryGetValue("ConstructionArea", out var area))
            {
                info.Add($"Diện tích xây dựng: {area}");
            }
            
            if (parsedData.GeneralInfo.TryGetValue("ConstructionTime", out var time))
            {
                info.Add($"Thời gian thi công: {time}");
            }
            
            if (parsedData.GeneralInfo.TryGetValue("NumberOfWorkers", out var workers))
            {
                info.Add($"Số công nhân: {workers}");
            }
            
            if (parsedData.GeneralInfo.TryGetValue("AverageSalary", out var salary))
            {
                info.Add($"Lương trung bình: {salary}");
            }
            
            return string.Join("\n", info);
        }
    }
}
