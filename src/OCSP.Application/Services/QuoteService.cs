// OCSP.Application/Services/QuoteService.cs
using Microsoft.EntityFrameworkCore;
using OCSP.Application.DTOs.Quotes;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Enums;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;

namespace OCSP.Application.Services
{
    public class QuoteService : IQuoteService
    {
        private readonly ApplicationDbContext _db;

        public QuoteService(ApplicationDbContext db) => _db = db;

        public async Task<QuoteRequestDto> CreateAsync(CreateQuoteRequestDto dto, Guid homeownerId, CancellationToken ct = default)
        {
            // 1) Validate project & quyền
            var project = await _db.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId, ct)
                ?? throw new ArgumentException("Project not found");

            if (project.HomeownerId != homeownerId)
                throw new UnauthorizedAccessException("Not project owner");

            if (string.IsNullOrWhiteSpace(dto.Scope))
                throw new ArgumentException("Scope is required");

            // 2) Tạo QuoteRequest (Draft)
            var qr = new QuoteRequest
            {
                ProjectId = project.Id,
                Scope = dto.Scope.Trim(),
                DueDate = dto.DueDate,
                Status = QuoteStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 3) Tạo danh sách invites (lọc trùng & bỏ self)
            var invitees = (dto.InviteeUserIds ?? new List<Guid>())
                .Where(id => id != Guid.Empty && id != homeownerId)
                .Distinct()
                .ToList();

            foreach (var uid in invitees)
            {
                qr.Invites.Add(new QuoteInvite
                {
                    ContractorUserId = uid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            _db.QuoteRequests.Add(qr);
            await _db.SaveChangesAsync(ct);

            return new QuoteRequestDto
            {
                Id = qr.Id,
                ProjectId = qr.ProjectId,
                Scope = qr.Scope,
                DueDate = qr.DueDate,
                Status = qr.Status.ToString(),
                InviteeUserIds = qr.Invites.Select(i => i.ContractorUserId).ToList()
            };
        }

        public async Task SendAsync(Guid quoteId, Guid homeownerId, CancellationToken ct = default)
        {
            var qr = await _db.QuoteRequests
                .Include(q => q.Project)
                .FirstOrDefaultAsync(q => q.Id == quoteId, ct)
                ?? throw new ArgumentException("Quote request not found");

            if (qr.Project.HomeownerId != homeownerId)
                throw new UnauthorizedAccessException("Not project owner");

            if (qr.Status != QuoteStatus.Draft)
                throw new InvalidOperationException("Only Draft quote can be sent");

            // business: phải có ít nhất 1 invite
            if (!await _db.QuoteInvites.AnyAsync(i => i.QuoteRequestId == qr.Id, ct))
                throw new InvalidOperationException("No invitees to send");

            qr.Status = QuoteStatus.Sent;
            qr.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            // TODO: gửi notification/email cho contractor (tuỳ bạn)
        }
        public async Task<QuoteRequestDto> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var q = await _db.QuoteRequests
            .AsNoTracking()
            .Include(x => x.Invites)
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ArgumentException("Quote request not found");

        // Quyền xem: homeowner của project hoặc contractor được mời
        var isHomeowner = q.Project.HomeownerId == userId;
        var isInvitedContractor = q.Invites.Any(i => i.ContractorUserId == userId);

        if (!isHomeowner && !isInvitedContractor)
            throw new UnauthorizedAccessException("You don't have access to this quote");

        return new QuoteRequestDto
        {
            Id = q.Id,
            ProjectId = q.ProjectId,
            Scope = q.Scope,
            DueDate = q.DueDate,
            Status = q.Status.ToString(),
            InviteeUserIds = q.Invites.Select(i => i.ContractorUserId).ToList()
        };
    }

        public async Task<IEnumerable<QuoteRequestDto>> ListByProjectAsync(Guid projectId, Guid userId, CancellationToken ct = default)
        {
            // Cho phép: homeowner của project hoặc participant của project xem.
            var project = await _db.Projects
                .Include(p => p.Participants)
                .FirstOrDefaultAsync(p => p.Id == projectId, ct)
                ?? throw new ArgumentException("Project not found");

            var canView = project.HomeownerId == userId
                          || project.Participants.Any(x => x.UserId == userId);
            if (!canView) throw new UnauthorizedAccessException("No access to this project");

            var list = await _db.QuoteRequests
                .AsNoTracking()
                .Include(q => q.Invites)
                .Where(q => q.ProjectId == projectId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync(ct);

            return list.Select(q => new QuoteRequestDto
            {
                Id = q.Id,
                ProjectId = q.ProjectId,
                Scope = q.Scope,
                DueDate = q.DueDate,
                Status = q.Status.ToString(),
                InviteeUserIds = q.Invites.Select(i => i.ContractorUserId).ToList()
            });
        }
        public async Task<IEnumerable<QuoteRequestDto>> ListMyInvitesAsync(Guid contractorUserId, CancellationToken ct = default)
{
    var invites = await _db.QuoteInvites
        .Include(i => i.QuoteRequest)
        .Where(i => i.ContractorUserId == contractorUserId)
        .ToListAsync(ct);

    return invites
        .Select(i => i.QuoteRequest)
        .Distinct()
        .Select(q => new QuoteRequestDto
        {
            Id = q.Id,
            ProjectId = q.ProjectId,
            Scope = q.Scope,
            DueDate = q.DueDate,
            Status = q.Status.ToString(),
            InviteeUserIds = q.Invites.Select(iv => iv.ContractorUserId).ToList()
        })
        .ToList();
}

public async Task<IEnumerable<QuoteRequestDetailDto>> ListMyInvitesDetailedAsync(
    Guid contractorUserId, CancellationToken ct = default)
{
    // 1) Lấy toàn bộ quote đã SENT có mời mình
    var quotes = await _db.QuoteRequests
        .AsNoTracking()
        .Include(q => q.Invites)
        .Include(q => q.Project)
            .ThenInclude(p => p.Homeowner) // navigation User
        .Where(q => q.Status == QuoteStatus.Sent &&
                    q.Invites.Any(i => i.ContractorUserId == contractorUserId))
        .OrderByDescending(q => q.CreatedAt)
        .ToListAsync(ct);

    if (quotes.Count == 0) return Enumerable.Empty<QuoteRequestDetailDto>();

    // 2) Gom tất cả userIds
    var userIds = quotes.SelectMany(q => q.Invites.Select(i => i.ContractorUserId))
                        .Concat(quotes.Select(q => q.Project.HomeownerId))
                        .Distinct()
                        .ToList();

    // 3) Load User & Company
    var users = await _db.Users
        .AsNoTracking()
        .Where(u => userIds.Contains(u.Id))
        .Select(u => new { u.Id, u.Username, u.Email })
        .ToListAsync(ct);
    var usersById = users.ToDictionary(x => x.Id, x => x);

    var companies = await _db.Contractors
        .AsNoTracking()
        .Where(c => userIds.Contains(c.UserId))
        .Select(c => new { c.UserId, c.CompanyName })
        .ToListAsync(ct);
    var companyByUserId = companies.ToDictionary(x => x.UserId, x => x.CompanyName);

    // 4) Lấy proposals của contractor này
    var quoteIds = quotes.Select(q => q.Id).ToList();
    var myProposals = await _db.Proposals
        .AsNoTracking()
        .Where(p => p.ContractorUserId == contractorUserId && quoteIds.Contains(p.QuoteRequestId))
        .Select(p => new { p.Id, p.QuoteRequestId, p.Status, p.PriceTotal, p.DurationDays })
        .ToListAsync(ct);
    var myProposalByQuoteId = myProposals.ToDictionary(x => x.QuoteRequestId, x => x);

    // 5) Map DTO
    return quotes.Select(q =>
    {
        usersById.TryGetValue(q.Project.HomeownerId, out var homeownerUser);
        myProposalByQuoteId.TryGetValue(q.Id, out var mp);

        return new QuoteRequestDetailDto
        {
            Id        = q.Id,
            Scope     = q.Scope,
            DueDate   = q.DueDate,
            Status    = q.Status.ToString(),
            CreatedAt = q.CreatedAt,
            UpdatedAt = q.UpdatedAt,

            Project = new ProjectSummaryDto
            {
                Id                      = q.Project.Id,
                Name                    = q.Project.Name ?? string.Empty,
                Address                 = q.Project.Address ?? string.Empty,
                Budget                  = q.Project.Budget,
                ActualBudget            = q.Project.ActualBudget,
                NumberOfFloors          = q.Project.NumberOfFloors,
                FloorArea               = q.Project.FloorArea,
                StartDate               = q.Project.StartDate,
                EstimatedCompletionDate = q.Project.EstimatedCompletionDate
            },

            Homeowner = new HomeownerSummaryDto
            {
                UserId   = q.Project.HomeownerId,
                Username = homeownerUser?.Username ?? "",
                Email    = homeownerUser?.Email ?? ""
            },

            Invitees = q.Invites.Select(i =>
            {
                usersById.TryGetValue(i.ContractorUserId, out var u);
                companyByUserId.TryGetValue(i.ContractorUserId, out var compName);

                return new InviteeSummaryDto
                {
                    UserId      = i.ContractorUserId,
                    Username    = u?.Username ?? "",
                    CompanyName = compName
                };
            }).ToList(),

            MyProposal = mp == null
                ? new MyProposalSummaryDto()
                : new MyProposalSummaryDto
                {
                    Id           = mp.Id,
                    Status       = mp.Status.ToString(),
                    PriceTotal   = mp.PriceTotal,
                    DurationDays = mp.DurationDays
                }
        };
    }).ToList();
}

    

    public async Task<QuoteRequestDetailDto> GetDetailForUserAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var q = await _db.QuoteRequests
            .AsNoTracking()
            .Include(x => x.Invites)
            .Include(x => x.Project).ThenInclude(p => p.Homeowner)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ArgumentException("Quote request not found");

        var isHomeowner = q.Project.HomeownerId == userId;
        var isInvited   = q.Invites.Any(i => i.ContractorUserId == userId);
        if (!isHomeowner && !isInvited)
            throw new UnauthorizedAccessException("You don't have access to this quote");

        // Lấy bảng phụ
        var inviteUserIds = q.Invites.Select(i => i.ContractorUserId).ToList();
        var users = await _db.Users
            .Where(u => inviteUserIds.Contains(u.Id) || u.Id == q.Project.HomeownerId)
            .Select(u => new { u.Id, u.Username, u.Email })
            .ToListAsync(ct);
        var companyByUser = await _db.Contractors
            .Where(c => inviteUserIds.Contains(c.UserId))
            .Select(c => new { c.UserId, c.CompanyName })
            .ToListAsync(ct);

        var myProp = await _db.Proposals
            .Where(p => p.QuoteRequestId == q.Id && p.ContractorUserId == userId)
            .Select(p => new { p.Id, p.Status, p.PriceTotal, p.DurationDays })
            .FirstOrDefaultAsync(ct);

        var hoUser = users.FirstOrDefault(u => u.Id == q.Project.HomeownerId);

        return new QuoteRequestDetailDto
        {
            Id = q.Id,
            Scope = q.Scope,
            DueDate = q.DueDate,
            Status = q.Status.ToString(),
            CreatedAt = q.CreatedAt,
            UpdatedAt = q.UpdatedAt,
            Project = new ProjectSummaryDto
            {
                Id = q.ProjectId,
                Name = q.Project.Name,
                Address = q.Project.Address,
                Budget = q.Project.Budget,
                ActualBudget = q.Project.ActualBudget,
                NumberOfFloors = q.Project.NumberOfFloors,
                FloorArea = q.Project.FloorArea,
                StartDate = q.Project.StartDate,
                EstimatedCompletionDate = q.Project.EstimatedCompletionDate
            },
            Homeowner = new HomeownerSummaryDto
            {
                UserId = q.Project.HomeownerId,
                Username = hoUser?.Username ?? "",
                Email = hoUser?.Email ?? ""
            },
            Invitees = q.Invites.Select(i =>
            {
                var u = users.FirstOrDefault(x => x.Id == i.ContractorUserId);
                var comp = companyByUser.FirstOrDefault(x => x.UserId == i.ContractorUserId)?.CompanyName;
                return new InviteeSummaryDto
                {
                    UserId = i.ContractorUserId,
                    Username = u?.Username ?? "",
                    CompanyName = comp
                };
            }).ToList(),
            MyProposal = myProp == null ? new MyProposalSummaryDto() : new MyProposalSummaryDto
            {
                Id = myProp.Id,
                Status = myProp.Status.ToString(),
                PriceTotal = myProp.PriceTotal,
                DurationDays = myProp.DurationDays
            }
        };
    }

    }
}
