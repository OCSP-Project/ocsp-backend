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
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;

        public ProposalService(ApplicationDbContext db, IFileService fileService, INotificationService notificationService)
        {
            _db = db;
            _fileService = fileService;
            _notificationService = notificationService;
        }

        public async Task<ProposalDto> CreateAsync(CreateProposalDto dto, Guid contractorUserId, CancellationToken ct = default)
{
    var qr = await _db.QuoteRequests
        .Include(q => q.Invites)
        .Include(q => q.Project)
        .FirstOrDefaultAsync(q => q.Id == dto.QuoteRequestId, ct)
        ?? throw new ArgumentException("Quote request not found");

    if (qr.Status != QuoteStatus.Sent)
        throw new InvalidOperationException("QuoteRequest must be Sent");

    if (!qr.Invites.Any(i => i.ContractorUserId == contractorUserId))
        throw new UnauthorizedAccessException("You are not invited to this quote");

    var exists = await _db.Proposals.AnyAsync(p =>
        p.QuoteRequestId == dto.QuoteRequestId &&
        p.ContractorUserId == contractorUserId, ct);
    if (exists)
        throw new InvalidOperationException("You already submitted a proposal for this quote");

    var total = dto.Items.Sum(i => i.Price);

    var p = new Proposal
    {
        QuoteRequestId = dto.QuoteRequestId,
        ProjectId = qr.ProjectId,            // ✅ thêm dòng này
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

            var quote = await _db.QuoteRequests
        .Include(q => q.Project)
        .FirstOrDefaultAsync(q => q.Id == quoteId, ct)
        ?? throw new ArgumentException("Quote request not found");

            // Save Excel file to storage
            var excelFileUrl = await _fileService.UploadFileAsync(
                excelFile.OpenReadStream(), 
                excelFile.FileName, 
                $"proposals/{quoteId}"
            );

            // Create proposal with data from "Tổng hợp" tab
            var proposal = new Proposal
            {
                QuoteRequestId = quoteId,
ProjectId = quote.ProjectId, 
                ContractorUserId = contractorUserId,
                Status = ProposalStatus.Draft,
                PriceTotal = parsedData.TotalCost,
                DurationDays = parsedData.TotalDurationDays,
                TermsSummary = BuildProjectInfoSummary(parsedData),
                IsFromExcel = true,
                ExcelFileName = excelFile.FileName,
                ExcelFileUrl = excelFileUrl,

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

            // Save new Excel file to storage
            var excelFileUrl = await _fileService.UploadFileAsync(
                excelFile.OpenReadStream(), 
                excelFile.FileName, 
                $"proposals/{proposal.QuoteRequestId}"
            );

            // Update proposal basic info
            proposal.PriceTotal = parsedData.TotalCost;
            proposal.DurationDays = parsedData.TotalDurationDays;
            proposal.ExcelFileName = excelFile.FileName;
            proposal.ExcelFileUrl = excelFileUrl;
            proposal.UpdatedAt = DateTime.UtcNow;

            // If proposal was RevisionRequested, change it back to Draft after update
            if (proposal.Status == ProposalStatus.RevisionRequested)
            {
                proposal.Status = ProposalStatus.Draft;
            }

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
                    .ThenInclude(q => q.Project)
                .FirstOrDefaultAsync(x => x.Id == proposalId, ct)
                ?? throw new ArgumentException("Proposal not found");

            if (p.ContractorUserId != contractorUserId)
                throw new UnauthorizedAccessException("Not your proposal");

            if (p.Status != ProposalStatus.Draft)
                throw new InvalidOperationException("chỉnh sửa file và upload lại");

            // Check if this proposal was revised by homeowner request
            var isResubmission = p.WasRevised;

            // Set status based on whether this is resubmission after revision
            p.Status = isResubmission ? ProposalStatus.Resubmitted : ProposalStatus.Submitted;
            p.HasBeenSubmitted = true; // Mark as submitted
            p.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            // Send notification to homeowner
            var homeownerId = p.QuoteRequest.Project.HomeownerId;
            await _notificationService.CreateAsync(new DTOs.Notification.CreateNotificationDto
            {
                UserId = homeownerId,
                Title = isResubmission ? "Đề xuất đã được chỉnh sửa và gửi lại" : "Đề xuất báo giá mới",
                Message = $"Bạn đã nhận được {(isResubmission ? "đề xuất chỉnh sửa" : "đề xuất báo giá mới")} cho dự án '{p.QuoteRequest.Project.Name}'",
                Type = NotificationType.ProposalSubmitted,
                ReferenceId = p.Id,
                ActionUrl = "/projects?tab=quotes",
                ProjectId = p.ProjectId
            }, ct);
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

        public async Task<ProposalDto> GetMyByIdAsync(Guid id, Guid currentUserId, CancellationToken ct = default)
        {
            var p = await _db.Proposals
                .Include(x => x.Items)
                .Include(x => x.QuoteRequest)
                    .ThenInclude(q => q.Project)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new ArgumentException("Proposal not found");
            
            // Allow both contractor (owner of proposal) and homeowner (owner of project) to view
            var isContractor = p.ContractorUserId == currentUserId;
            var isHomeowner = p.QuoteRequest?.Project?.HomeownerId == currentUserId;
            
            if (!isContractor && !isHomeowner)
                throw new UnauthorizedAccessException("No access to this proposal");
            
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
            if (p.Status != ProposalStatus.Draft && p.Status != ProposalStatus.RevisionRequested)
                throw new InvalidOperationException("Only Draft or RevisionRequested proposal can be updated");

            // If proposal was RevisionRequested, change it back to Draft after update
            if (p.Status == ProposalStatus.RevisionRequested)
            {
                p.Status = ProposalStatus.Draft;
            }

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

            if (selected.Status != ProposalStatus.Submitted && selected.Status != ProposalStatus.Resubmitted)
                throw new InvalidOperationException("Only Submitted or Resubmitted proposal can be accepted");

            using var tx = await _db.Database.BeginTransactionAsync(ct);

            // Set accepted
            selected.Status = ProposalStatus.Accepted;
            selected.UpdatedAt = DateTime.UtcNow;

            // Get all other proposals for this quote to send rejection notifications
            var otherProposals = await _db.Proposals
                .Where(p => p.QuoteRequestId == selected.QuoteRequestId && p.Id != proposalId)
                .ToListAsync(ct);

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

            // Send notification to accepted contractor
            await _notificationService.CreateAsync(new DTOs.Notification.CreateNotificationDto
            {
                UserId = selected.ContractorUserId,
                Title = "Đề xuất của bạn đã được chấp nhận",
                Message = $"Chúc mừng! Đề xuất của bạn cho dự án '{selected.QuoteRequest.Project.Name}' đã được chủ nhà chấp nhận",
                Type = NotificationType.ProposalAccepted,
                ReferenceId = selected.Id,
                ActionUrl = "/projects?tab=invites",
                ProjectId = selected.ProjectId
            }, ct);

            // Send rejection notifications to other contractors
            foreach (var otherProposal in otherProposals)
            {
                await _notificationService.CreateAsync(new DTOs.Notification.CreateNotificationDto
                {
                    UserId = otherProposal.ContractorUserId,
                    Title = "Đề xuất không được chọn",
                    Message = $"Đề xuất của bạn cho dự án '{selected.QuoteRequest.Project.Name}' không được chọn. Cảm ơn bạn đã tham gia!",
                    Type = NotificationType.ProposalRejected,
                    ReferenceId = otherProposal.Id,
                    ActionUrl = "/projects?tab=invites",
                    ProjectId = selected.ProjectId
                }, ct);
            }
        }

        public async Task RequestRevisionAsync(Guid proposalId, Guid homeownerId, CancellationToken ct = default)
        {
            var proposal = await _db.Proposals
                .Include(p => p.QuoteRequest)
                    .ThenInclude(q => q.Project)
                .FirstOrDefaultAsync(p => p.Id == proposalId, ct)
                ?? throw new ArgumentException("Proposal not found");

            if (proposal.QuoteRequest.Project.HomeownerId != homeownerId)
                throw new UnauthorizedAccessException("Not project owner");

            if (proposal.Status != ProposalStatus.Submitted && proposal.Status != ProposalStatus.Resubmitted)
                throw new InvalidOperationException("Only Submitted or Resubmitted proposal can be requested for revision");

            // Set proposal status to RevisionRequested and mark as revised
            proposal.Status = ProposalStatus.RevisionRequested;
            proposal.WasRevised = true; // Mark that this proposal was revised
            proposal.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            // Send notification to contractor
            await _notificationService.CreateAsync(new DTOs.Notification.CreateNotificationDto
            {
                UserId = proposal.ContractorUserId,
                Title = "Yêu cầu chỉnh sửa đề xuất",
                Message = $"Chủ nhà yêu cầu chỉnh sửa đề xuất của bạn cho dự án '{proposal.QuoteRequest.Project.Name}'. Vui lòng liên hệ với chủ nhà để thảo luận chi tiết.",
                Type = NotificationType.ProposalRevisionRequested,
                ReferenceId = proposal.Id,
                ActionUrl = "/projects?tab=invites",
                ProjectId = proposal.ProjectId
            }, ct);
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
            ExcelFileUrl = p.ExcelFileUrl,
            
            // Project Information from Excel
            ProjectTitle = p.ProjectTitle,
            ConstructionArea = p.ConstructionArea,
            ConstructionTime = p.ConstructionTime,
            NumberOfWorkers = p.NumberOfWorkers,
            AverageSalary = p.AverageSalary,
            
            // Resubmission tracking
            HasBeenSubmitted = p.HasBeenSubmitted
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
            ExcelFileUrl = p.ExcelFileUrl,
            
            // Project Information from Excel
            ProjectTitle = p.ProjectTitle,
            ConstructionArea = p.ConstructionArea,
            ConstructionTime = p.ConstructionTime,
            NumberOfWorkers = p.NumberOfWorkers,
            AverageSalary = p.AverageSalary,
            
            // Resubmission tracking
            HasBeenSubmitted = p.HasBeenSubmitted,
            
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

        public async Task<(Stream fileStream, string fileName, string contentType)> DownloadExcelAsync(Guid proposalId, Guid homeownerId, CancellationToken ct = default)
        {
            var proposal = await _db.Proposals
                .Include(p => p.QuoteRequest)
                    .ThenInclude(q => q.Project)
                .FirstOrDefaultAsync(p => p.Id == proposalId, ct)
                ?? throw new ArgumentException("Proposal not found");

            if (proposal.QuoteRequest.Project.HomeownerId != homeownerId)
                throw new UnauthorizedAccessException("Not project owner");

            if (string.IsNullOrEmpty(proposal.ExcelFileUrl))
                throw new InvalidOperationException("No Excel file available for this proposal");

            // Get file from storage
            var fileBytes = await _fileService.GetFileAsync(proposal.ExcelFileUrl);
            var fileStream = new MemoryStream(fileBytes);
            var fileName = proposal.ExcelFileName ?? "proposal.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return (fileStream, fileName, contentType);
        }
    }
}