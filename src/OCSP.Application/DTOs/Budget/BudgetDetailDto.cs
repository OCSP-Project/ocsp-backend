namespace OCSP.Application.DTOs.Budget
{
    public class BudgetDetailDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? WorkItemId { get; set; }

        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateBudgetDetailDto
    {
        public Guid ProjectId { get; set; }
        public Guid? WorkItemId { get; set; }

        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public string? Notes { get; set; }
    }

    public class UpdateBudgetDetailDto
    {
        public string? Name { get; set; }
        public string? Unit { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Notes { get; set; }
    }

    public class BudgetSummaryDto
    {
        public decimal TotalBudget { get; set; }
        public decimal ActualCost { get; set; }
        public decimal AmountDisbursed { get; set; }
        public decimal Remaining { get; set; }
        public decimal CostPercentage { get; set; }
        public decimal DisbursementPercentage { get; set; }
        public int WorkItemCount { get; set; }
        public int PaymentRequestCount { get; set; }
    }

    public class BudgetByCategoryDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public decimal ActualCost { get; set; }
        public decimal Percentage { get; set; }
        public int WorkItemCount { get; set; }
    }
}
