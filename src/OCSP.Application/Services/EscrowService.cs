// OCSP.Application/Services/EscrowService.cs
using Microsoft.EntityFrameworkCore;
using OCSP.Application.DTOs.Escrow;
using OCSP.Application.DTOs.Payments;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
// Alias to ensure we reference the enum with Planned/Submitted/Funded/Approved/Released
using MilestoneStatusEnum = OCSP.Domain.Enums.MilestoneStatus;
using OCSP.Infrastructure.Data;

namespace OCSP.Application.Services
{
    public class EscrowService : IEscrowService
    {
        private const decimal DEFAULT_COMMISSION = 0.10m;
        private readonly ApplicationDbContext _db;
        public EscrowService(ApplicationDbContext db) => _db = db;

        public async Task<EscrowAccountDto> SetupEscrowAsync(SetupEscrowDto dto, Guid homeownerId, CancellationToken ct = default)
        {
            var c = await _db.Contracts
                .Include(x => x.Escrow)
                .FirstOrDefaultAsync(x => x.Id == dto.ContractId, ct)
                ?? throw new ArgumentException("Contract not found");

            if (c.HomeownerUserId != homeownerId)
                throw new UnauthorizedAccessException("Only homeowner can setup escrow");

            if (c.Status != ContractStatus.Active && c.Status != ContractStatus.PendingSignatures)
                throw new InvalidOperationException("Contract must be Active or PendingSignatures");

            if (c.Escrow == null)
            {
                c.Escrow = new EscrowAccount
                {
                    ContractId = c.Id,
                    Provider = dto.Provider,
                    Status = EscrowStatus.Pending,
                    Balance = 0m,
                    ExternalAccountId = dto.ExternalAccountId
                };
                _db.EscrowAccounts.Add(c.Escrow);
            }
            else
            {
                // cập nhật provider/external id nếu muốn
                c.Escrow.Provider = dto.Provider;
                if (!string.IsNullOrWhiteSpace(dto.ExternalAccountId))
                    c.Escrow.ExternalAccountId = dto.ExternalAccountId!.Trim();
            }

            if (dto.Amount > 0)
            {
                c.Escrow.Balance += dto.Amount;
                c.Escrow.Status = EscrowStatus.Funded;

                _db.PaymentTransactions.Add(new PaymentTransaction
                {
                    ContractId = c.Id,
                    MilestoneId = null,
                    Provider = dto.Provider,
                    Type = PaymentType.Fund,
                    Status = PaymentStatus.Succeeded,
                    Amount = dto.Amount,
                    Description = "Initial escrow funding"
                });
            }

            await _db.SaveChangesAsync(ct);

            return new EscrowAccountDto
            {
                Id = c.Escrow.Id,
                ContractId = c.Id,
                Provider = c.Escrow.Provider,
                Status = c.Escrow.Status.ToString(),
                Balance = c.Escrow.Balance,
                ExternalAccountId = c.Escrow.ExternalAccountId,
                CreatedAt = c.Escrow.CreatedAt
            };
        }

        public async Task<PaymentTransactionDto> FundMilestoneAsync(FundMilestoneDto dto, Guid homeownerId, CancellationToken ct = default)
        {
            var ms = await _db.ContractMilestones
                .Include(m => m.Contract).ThenInclude(c => c.Escrow)
                .FirstOrDefaultAsync(m => m.Id == dto.MilestoneId, ct)
                ?? throw new ArgumentException("Milestone not found");

            if (ms.Contract.HomeownerUserId != homeownerId)
                throw new UnauthorizedAccessException("Only homeowner can fund milestone");

            if (ms.Contract.Escrow == null || ms.Contract.Escrow.Status == EscrowStatus.Closed)
                throw new InvalidOperationException("Escrow not found/closed");

            if (ms.Status != MilestoneStatusEnum.Planned && ms.Status != MilestoneStatusEnum.Submitted)
                throw new InvalidOperationException("Milestone must be Planned or Submitted to fund");

            if (dto.Amount <= 0 || dto.Amount > ms.Amount)
                throw new InvalidOperationException("Invalid funding amount");

            ms.Contract.Escrow.Balance += dto.Amount;
            ms.Status = MilestoneStatusEnum.Funded;

            var txn = new PaymentTransaction
            {
                ContractId = ms.ContractId,
                MilestoneId = ms.Id,
                Provider = ms.Contract.Escrow.Provider,
                Type = PaymentType.Fund,
                Status = PaymentStatus.Succeeded,
                Amount = dto.Amount,
                Description = "Fund milestone"
            };

            _db.PaymentTransactions.Add(txn);
            await _db.SaveChangesAsync(ct);

            return MapTxn(txn);
        }

        public async Task<MilestonePayoutResultDto> ApproveMilestoneAsync(Guid milestoneId, Guid homeownerId, CancellationToken ct = default)
        {
            var ms = await _db.ContractMilestones
                .Include(m => m.Contract).ThenInclude(c => c.Escrow)
                .FirstOrDefaultAsync(m => m.Id == milestoneId, ct)
                ?? throw new ArgumentException("Milestone not found");

            if (ms.Contract.HomeownerUserId != homeownerId)
                throw new UnauthorizedAccessException("Only homeowner can approve milestone");

            if (ms.Status != MilestoneStatusEnum.Submitted && ms.Status != MilestoneStatusEnum.Funded)
                throw new InvalidOperationException("Milestone is not submitted/funded");

            ms.Status = MilestoneStatusEnum.Approved;
            await _db.SaveChangesAsync(ct);

            return new MilestonePayoutResultDto
            {
                MilestoneId = ms.Id,
                Gross = ms.Amount,
                Commission = 0m,
                Net = 0m,
                EscrowBalanceAfter = ms.Contract.Escrow?.Balance ?? 0m,
                Status = ms.Status.ToString()
            };
        }

        public async Task<MilestonePayoutResultDto> ReleaseMilestoneAsync(ReleaseMilestoneDto dto, Guid homeownerId, CancellationToken ct = default)
        {
            var ms = await _db.ContractMilestones
                .Include(m => m.Contract).ThenInclude(c => c.Escrow)
                .FirstOrDefaultAsync(m => m.Id == dto.MilestoneId, ct)
                ?? throw new ArgumentException("Milestone not found");

            if (ms.Contract.HomeownerUserId != homeownerId)
                throw new UnauthorizedAccessException("Only homeowner can release milestone");

            if (ms.Contract.Escrow == null || ms.Contract.Escrow.Status == EscrowStatus.Closed)
                throw new InvalidOperationException("Escrow not found/closed");

            if (ms.Status != MilestoneStatusEnum.Approved && ms.Status != MilestoneStatusEnum.Funded)
                throw new InvalidOperationException("Milestone must be Approved or Funded to release");

            var rate = dto.CommissionRate ?? DEFAULT_COMMISSION;
            if (rate < 0 || rate >= 1m) throw new ArgumentException("CommissionRate must be in (0,1)");

            var gross = ms.Amount;
            var fee = Math.Round(gross * rate, 2);
            var net = gross - fee;

            if (ms.Contract.Escrow.Balance < gross)
                throw new InvalidOperationException("Insufficient escrow balance");

            // trừ tổng balance một lần, ghi 2 transaction: Commission & Release
            ms.Contract.Escrow.Balance -= gross;

            _db.PaymentTransactions.Add(new PaymentTransaction
            {
                ContractId = ms.ContractId,
                MilestoneId = ms.Id,
                Provider = ms.Contract.Escrow.Provider,
                Type = PaymentType.Commission,
                Status = PaymentStatus.Succeeded,
                Amount = fee,
                Description = "Platform commission"
            });

            _db.PaymentTransactions.Add(new PaymentTransaction
            {
                ContractId = ms.ContractId,
                MilestoneId = ms.Id,
                Provider = ms.Contract.Escrow.Provider,
                Type = PaymentType.Release,
                Status = PaymentStatus.Succeeded,
                Amount = net,
                Description = "Release to contractor"
            });

            ms.Status = MilestoneStatusEnum.Released;
            await _db.SaveChangesAsync(ct);

            return new MilestonePayoutResultDto
            {
                MilestoneId = ms.Id,
                Gross = gross,
                Commission = fee,
                Net = net,
                EscrowBalanceAfter = ms.Contract.Escrow.Balance,
                Status = ms.Status.ToString()
            };
        }

        public async Task<EscrowAccountDto> GetEscrowByContractAsync(Guid contractId, Guid userId, CancellationToken ct = default)
        {
            var c = await _db.Contracts.Include(x => x.Escrow)
                .FirstOrDefaultAsync(x => x.Id == contractId, ct)
                ?? throw new ArgumentException("Contract not found");

            if (c.HomeownerUserId != userId && c.ContractorUserId != userId)
                throw new UnauthorizedAccessException();

            if (c.Escrow == null) throw new ArgumentException("Escrow not created");

            return new EscrowAccountDto
            {
                Id = c.Escrow.Id,
                ContractId = c.Id,
                Provider = c.Escrow.Provider,
                Status = c.Escrow.Status.ToString(),
                Balance = c.Escrow.Balance,
                ExternalAccountId = c.Escrow.ExternalAccountId,
                CreatedAt = c.Escrow.CreatedAt
            };
        }

        public async Task<List<PaymentTransactionDto>> ListTransactionsByContractAsync(Guid contractId, Guid userId, CancellationToken ct = default)
        {
            var c = await _db.Contracts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == contractId, ct)
                ?? throw new ArgumentException("Contract not found");

            if (c.HomeownerUserId != userId && c.ContractorUserId != userId)
                throw new UnauthorizedAccessException();

            var txns = await _db.PaymentTransactions
                .AsNoTracking()
                .Where(t => t.ContractId == contractId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(ct);

            return txns.Select(MapTxn).ToList();
        }

        private static PaymentTransactionDto MapTxn(PaymentTransaction t) => new()
        {
            Id = t.Id,
            ContractId = t.ContractId,
            MilestoneId = t.MilestoneId,
            Provider = t.Provider,
            Type = t.Type,
            Status = t.Status,
            Amount = t.Amount,
            Description = t.Description,
            CreatedAt = t.CreatedAt
        };
    }
}
