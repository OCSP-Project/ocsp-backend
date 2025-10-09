// OCSP.Application/Services/ContractMilestoneService.cs
using Microsoft.EntityFrameworkCore;
using OCSP.Application.DTOs.Milestones;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;

namespace OCSP.Application.Services
{
    public class ContractMilestoneService : IContractMilestoneService
    {
        private readonly ApplicationDbContext _db;
        public ContractMilestoneService(ApplicationDbContext db) => _db = db;

        // Tạo 1 milestone
        public async Task<MilestoneDto> CreateAsync(Guid contractId, CreateMilestoneDto dto, Guid currentUserId, CancellationToken ct = default)
{
    if (string.IsNullOrWhiteSpace(dto.Name))
        throw new ArgumentException("Name is required");
    if (dto.Amount <= 0)
        throw new ArgumentException("Amount must be > 0");

    var contract = await _db.Contracts
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == contractId, ct)
        ?? throw new ArgumentException("Contract not found");

    if (contract.HomeownerUserId != currentUserId)
        throw new UnauthorizedAccessException("Only homeowner can create milestones");

    if (contract.Status != ContractStatus.Completed)
    throw new InvalidOperationException("Contract must be Completed to create milestones");


    var totalExisting = await _db.ContractMilestones
        .Where(m => m.ContractId == contract.Id)
        .SumAsync(m => m.Amount, ct);

    if (totalExisting + dto.Amount > contract.TotalPrice)
        throw new InvalidOperationException("Total milestone amount cannot exceed contract total price");

    var ms = new ContractMilestone
    {
        ContractId = contract.Id,
        Name = dto.Name.Trim(),
        Amount = dto.Amount,
        DueDate = NormalizeUtc(dto.DueDate),
        Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim(),
        Status = MilestoneStatus.Planned,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    _db.ContractMilestones.Add(ms);
    await _db.SaveChangesAsync(ct);

    return Map(ms);
}


        // Tạo nhiều milestone 1 lần
        public async Task<IEnumerable<MilestoneDto>> BulkCreateAsync(BulkCreateMilestonesDto dto, Guid currentUserId, CancellationToken ct = default)
        {
            if (dto.Milestones == null || !dto.Milestones.Any())
                throw new ArgumentException("Milestones list cannot be empty");

            var contract = await _db.Contracts
                .FirstOrDefaultAsync(c => c.Id == dto.ContractId, ct)
                ?? throw new ArgumentException("Contract not found");

            if (contract.HomeownerUserId != currentUserId)
                throw new UnauthorizedAccessException("Only homeowner can create milestones");

            if (contract.Status != ContractStatus.Completed)
    throw new InvalidOperationException("Contract must be Completed to create milestones");


            // Validate từng item & tính tổng new
            decimal totalNew = 0;
            foreach (var m in dto.Milestones)
            {
                if (string.IsNullOrWhiteSpace(m.Name))
                    throw new ArgumentException("Milestone name is required");
                if (m.Amount <= 0)
                    throw new ArgumentException("Milestone amount must be > 0");

                totalNew += m.Amount;
            }

            // ✅ Check tổng tiền milestones (đã có + sắp tạo) không vượt quá contract.TotalPrice
            var totalExisting = await _db.ContractMilestones
                .Where(m => m.ContractId == contract.Id)
                .SumAsync(m => m.Amount, ct);

            if (totalExisting + totalNew > contract.TotalPrice)
                throw new InvalidOperationException("Total milestone amount cannot exceed contract total price");

            // Tạo danh sách
            var milestones = dto.Milestones.Select(m => new ContractMilestone
{
    ContractId = contract.Id,
    Name = m.Name.Trim(),
    Amount = m.Amount,
                DueDate = NormalizeUtc(m.DueDate),
    Note = string.IsNullOrWhiteSpace(m.Note) ? null : m.Note!.Trim(),
    Status = MilestoneStatus.Planned,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
}).ToList();


            _db.ContractMilestones.AddRange(milestones);
            await _db.SaveChangesAsync(ct);

            return milestones.Select(Map).ToList();
        }

        private static DateTime? NormalizeUtc(DateTime? input)
        {
            if (input == null) return null;
            var value = input.Value;
            if (value.Kind == DateTimeKind.Unspecified)
            {
                // Treat unspecified as UTC to satisfy timestamptz
                return DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
            return value.ToUniversalTime();
        }

        // Danh sách milestones theo hợp đồng
        public async Task<IEnumerable<MilestoneDto>> ListByContractAsync(Guid contractId, Guid currentUserId, CancellationToken ct = default)
        {
            var contract = await _db.Contracts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == contractId, ct)
                ?? throw new ArgumentException("Contract not found");

            if (contract.HomeownerUserId != currentUserId && contract.ContractorUserId != currentUserId)
                throw new UnauthorizedAccessException();

            var list = await _db.ContractMilestones
                .AsNoTracking()
                .Where(m => m.ContractId == contractId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(ct);

            return list.Select(Map).ToList();
        }

        // Contractor submit milestone đã hoàn thành
        public async Task<MilestoneDto> SubmitAsync(SubmitMilestoneDto dto, Guid contractorUserId, CancellationToken ct = default)
        {
            var ms = await _db.ContractMilestones
                .Include(m => m.Contract)
                .FirstOrDefaultAsync(m => m.Id == dto.MilestoneId, ct)
                ?? throw new ArgumentException("Milestone not found");

            if (ms.Contract.ContractorUserId != contractorUserId)
                throw new UnauthorizedAccessException("Only the contractor of this contract can submit");

            if (ms.Status != MilestoneStatus.Planned && ms.Status != MilestoneStatus.Funded)
                throw new InvalidOperationException("Milestone must be Planned or Funded to submit");

            ms.Status = MilestoneStatus.Submitted;
            if (!string.IsNullOrWhiteSpace(dto.Note))
                ms.Note = dto.Note!.Trim();

            await _db.SaveChangesAsync(ct);
            return Map(ms);
        }

        // Map entity -> DTO
        private static MilestoneDto Map(ContractMilestone m) => new()
        {
            Id = m.Id,
            ContractId = m.ContractId,
            Name = m.Name,
            Amount = m.Amount,
            DueDate = m.DueDate,
            Status = m.Status.ToString(),
            Note = m.Note,
            CreatedAt = m.CreatedAt
        };

        // Cập nhật milestone (homeowner)
        public async Task<MilestoneDto> UpdateAsync(Guid milestoneId, UpdateMilestoneDto dto, Guid currentUserId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required");
            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be > 0");

            var ms = await _db.ContractMilestones
                .Include(m => m.Contract)
                .FirstOrDefaultAsync(m => m.Id == milestoneId, ct)
                ?? throw new ArgumentException("Milestone not found");

            if (ms.Contract.HomeownerUserId != currentUserId)
                throw new UnauthorizedAccessException();

            if (ms.Contract.Status != ContractStatus.Completed)
                throw new InvalidOperationException("Contract must be Completed to update milestones");

            if (ms.Status != MilestoneStatus.Planned)
                throw new InvalidOperationException("Only Planned milestones can be updated");

            // Validate tổng không vượt quá tổng hợp đồng
            var totalOther = await _db.ContractMilestones
                .Where(x => x.ContractId == ms.ContractId && x.Id != ms.Id)
                .SumAsync(x => x.Amount, ct);

            if (totalOther + dto.Amount > ms.Contract.TotalPrice)
                throw new InvalidOperationException("Total milestone amount cannot exceed contract total price");

            ms.Name = dto.Name.Trim();
            ms.Amount = dto.Amount;
            ms.DueDate = NormalizeUtc(dto.DueDate);
            ms.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();
            ms.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return Map(ms);
        }

        // Xóa milestone (homeowner)
        public async Task DeleteAsync(Guid milestoneId, Guid currentUserId, CancellationToken ct = default)
        {
            var ms = await _db.ContractMilestones
                .Include(m => m.Contract)
                .FirstOrDefaultAsync(m => m.Id == milestoneId, ct)
                ?? throw new ArgumentException("Milestone not found");

            if (ms.Contract.HomeownerUserId != currentUserId)
                throw new UnauthorizedAccessException();

            if (ms.Contract.Status != ContractStatus.Completed)
                throw new InvalidOperationException("Contract must be Completed to delete milestones");

            if (ms.Status != MilestoneStatus.Planned)
                throw new InvalidOperationException("Only Planned milestones can be deleted");

            _db.ContractMilestones.Remove(ms);
            await _db.SaveChangesAsync(ct);
        }
    }
}
