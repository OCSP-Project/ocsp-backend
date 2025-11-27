using OCSP.Application.DTOs.Budget;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace OCSP.Application.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly ApplicationDbContext _context;

        public BudgetService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BudgetDetailDto>> GetAllByProjectAsync(Guid projectId, CancellationToken ct = default)
        {
            var budgetDetails = await _context.Set<BudgetDetail>()
                .Where(b => b.ProjectId == projectId && !b.IsDeleted)
                .OrderBy(b => b.Code)
                .ToListAsync(ct);

            return budgetDetails.Select(b => MapToDto(b)).ToList();
        }

        public async Task<BudgetDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var budgetDetail = await _context.Set<BudgetDetail>()
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

            return budgetDetail == null ? null : MapToDto(budgetDetail);
        }

        public async Task<BudgetDetailDto> CreateAsync(CreateBudgetDetailDto dto, Guid currentUserId, CancellationToken ct = default)
        {
            // Validate project exists
            var projectExists = await _context.Set<Project>().AnyAsync(p => p.Id == dto.ProjectId, ct);
            if (!projectExists)
                throw new ArgumentException("Project not found");

            // Validate work item if specified
            if (dto.WorkItemId.HasValue)
            {
                var workItemExists = await _context.Set<WorkItem>().AnyAsync(w => w.Id == dto.WorkItemId.Value && !w.IsDeleted, ct);
                if (!workItemExists)
                    throw new ArgumentException("Work item not found");
            }

            // Calculate total amount
            var totalAmount = dto.Quantity * dto.UnitPrice;

            var budgetDetail = new BudgetDetail
            {
                Id = Guid.NewGuid(),
                ProjectId = dto.ProjectId,
                WorkItemId = dto.WorkItemId,
                Code = dto.Code,
                Name = dto.Name,
                Unit = dto.Unit,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                TotalAmount = totalAmount,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId.ToString(),
                UpdatedBy = currentUserId.ToString()
            };

            _context.Set<BudgetDetail>().Add(budgetDetail);
            await _context.SaveChangesAsync(ct);

            // Recalculate project budget
            await RecalculateProjectBudgetAsync(dto.ProjectId, ct);

            return MapToDto(budgetDetail);
        }

        public async Task<BudgetDetailDto> UpdateAsync(Guid id, UpdateBudgetDetailDto dto, Guid currentUserId, CancellationToken ct = default)
        {
            var budgetDetail = await _context.Set<BudgetDetail>()
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

            if (budgetDetail == null)
                throw new ArgumentException("Budget detail not found");

            if (dto.Name != null) budgetDetail.Name = dto.Name;
            if (dto.Unit != null) budgetDetail.Unit = dto.Unit;
            if (dto.Quantity.HasValue) budgetDetail.Quantity = dto.Quantity.Value;
            if (dto.UnitPrice.HasValue) budgetDetail.UnitPrice = dto.UnitPrice.Value;
            if (dto.Notes != null) budgetDetail.Notes = dto.Notes;

            // Recalculate total amount
            budgetDetail.TotalAmount = budgetDetail.Quantity * budgetDetail.UnitPrice;
            budgetDetail.UpdatedAt = DateTime.UtcNow;
            budgetDetail.UpdatedBy = currentUserId.ToString();

            await _context.SaveChangesAsync(ct);

            // Recalculate project budget
            await RecalculateProjectBudgetAsync(budgetDetail.ProjectId, ct);

            return MapToDto(budgetDetail);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var budgetDetail = await _context.Set<BudgetDetail>()
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

            if (budgetDetail == null)
                throw new ArgumentException("Budget detail not found");

            var projectId = budgetDetail.ProjectId;

            budgetDetail.IsDeleted = true;
            budgetDetail.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            // Recalculate project budget
            await RecalculateProjectBudgetAsync(projectId, ct);
        }

        public async Task<BudgetSummaryDto> GetSummaryAsync(Guid projectId, CancellationToken ct = default)
        {
            var project = await _context.Set<Project>().FindAsync(projectId);
            if (project == null)
                throw new ArgumentException("Project not found");

            // Calculate total budget from budget details
            var totalBudget = await _context.Set<BudgetDetail>()
                .Where(b => b.ProjectId == projectId && !b.IsDeleted)
                .SumAsync(b => b.TotalAmount, ct);

            // Calculate actual cost from work items
            var actualCost = await _context.Set<WorkItem>()
                .Where(w => w.ProjectId == projectId && !w.IsDeleted && w.ActualQuantity.HasValue && w.UnitPrice.HasValue)
                .SumAsync(w => w.ActualQuantity!.Value * w.UnitPrice!.Value, ct);

            // Calculate amount disbursed from paid payment requests
            var amountDisbursed = await _context.Set<PaymentRequest>()
                .Where(p => p.ProjectId == projectId && !p.IsDeleted && p.Status == Domain.Enums.PaymentRequestStatus.Paid)
                .SumAsync(p => p.Amount, ct);

            var remaining = totalBudget - actualCost;
            var costPercentage = totalBudget > 0 ? (actualCost / totalBudget) * 100 : 0;
            var disbursementPercentage = totalBudget > 0 ? (amountDisbursed / totalBudget) * 100 : 0;

            var workItemCount = await _context.Set<WorkItem>()
                .CountAsync(w => w.ProjectId == projectId && !w.IsDeleted, ct);

            var paymentRequestCount = await _context.Set<PaymentRequest>()
                .CountAsync(p => p.ProjectId == projectId && !p.IsDeleted, ct);

            return new BudgetSummaryDto
            {
                TotalBudget = totalBudget,
                ActualCost = actualCost,
                AmountDisbursed = amountDisbursed,
                Remaining = remaining,
                CostPercentage = costPercentage,
                DisbursementPercentage = disbursementPercentage,
                WorkItemCount = workItemCount,
                PaymentRequestCount = paymentRequestCount
            };
        }

        public async Task<List<BudgetByCategoryDto>> GetByCategoryAsync(Guid projectId, CancellationToken ct = default)
        {
            var workItems = await _context.Set<WorkItem>()
                .Where(w => w.ProjectId == projectId && !w.IsDeleted && w.Level == 1) // Phase level
                .ToListAsync(ct);

            var result = new List<BudgetByCategoryDto>();

            foreach (var phase in workItems)
            {
                // Get all work items under this phase
                var phaseWorkItems = await GetAllChildWorkItems(phase.Id, ct);
                phaseWorkItems.Add(phase);

                var totalBudget = phaseWorkItems
                    .Where(w => w.TotalAmount.HasValue)
                    .Sum(w => w.TotalAmount!.Value);

                var actualCost = phaseWorkItems
                    .Where(w => w.ActualQuantity.HasValue && w.UnitPrice.HasValue)
                    .Sum(w => w.ActualQuantity!.Value * w.UnitPrice!.Value);

                var projectTotalBudget = await _context.Set<BudgetDetail>()
                    .Where(b => b.ProjectId == projectId && !b.IsDeleted)
                    .SumAsync(b => b.TotalAmount, ct);

                var percentage = projectTotalBudget > 0 ? (totalBudget / projectTotalBudget) * 100 : 0;

                result.Add(new BudgetByCategoryDto
                {
                    Category = phase.Name,
                    TotalBudget = totalBudget,
                    ActualCost = actualCost,
                    Percentage = percentage,
                    WorkItemCount = phaseWorkItems.Count
                });
            }

            return result.OrderByDescending(c => c.TotalBudget).ToList();
        }

        public async Task RecalculateProjectBudgetAsync(Guid projectId, CancellationToken ct = default)
        {
            var project = await _context.Set<Project>().FindAsync(projectId);
            if (project == null) return;

            // Calculate total budget from budget details
            var totalBudget = await _context.Set<BudgetDetail>()
                .Where(b => b.ProjectId == projectId && !b.IsDeleted)
                .SumAsync(b => b.TotalAmount, ct);

            // Calculate actual budget from work items
            var actualBudget = await _context.Set<WorkItem>()
                .Where(w => w.ProjectId == projectId && !w.IsDeleted && w.ActualQuantity.HasValue && w.UnitPrice.HasValue)
                .SumAsync(w => w.ActualQuantity!.Value * w.UnitPrice!.Value, ct);

            project.Budget = totalBudget;
            project.ActualBudget = actualBudget;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
        }

        // Private helper methods
        private BudgetDetailDto MapToDto(BudgetDetail budgetDetail)
        {
            return new BudgetDetailDto
            {
                Id = budgetDetail.Id,
                ProjectId = budgetDetail.ProjectId,
                WorkItemId = budgetDetail.WorkItemId,
                Code = budgetDetail.Code,
                Name = budgetDetail.Name,
                Unit = budgetDetail.Unit,
                Quantity = budgetDetail.Quantity,
                UnitPrice = budgetDetail.UnitPrice,
                TotalAmount = budgetDetail.TotalAmount,
                Notes = budgetDetail.Notes,
                CreatedAt = budgetDetail.CreatedAt,
                UpdatedAt = budgetDetail.UpdatedAt
            };
        }

        private async Task<List<WorkItem>> GetAllChildWorkItems(Guid parentId, CancellationToken ct)
        {
            var result = new List<WorkItem>();
            var children = await _context.Set<WorkItem>()
                .Where(w => w.ParentId == parentId && !w.IsDeleted)
                .ToListAsync(ct);

            result.AddRange(children);

            foreach (var child in children)
            {
                var grandChildren = await GetAllChildWorkItems(child.Id, ct);
                result.AddRange(grandChildren);
            }

            return result;
        }
    }
}
