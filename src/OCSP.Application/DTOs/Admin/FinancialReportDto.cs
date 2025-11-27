namespace OCSP.Application.DTOs.Admin
{
    public class FinancialReportDto
    {
        // Tổng quan
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal TotalCommission { get; set; }

        // Theo tháng (12 tháng gần nhất)
        public List<MonthlyFinancialDto> MonthlyData { get; set; } = new();

        // Theo loại giao dịch
        public decimal CompletedContractValue { get; set; }
        public decimal ActiveContractValue { get; set; }
        public decimal PendingPaymentValue { get; set; }

        // Thống kê giao dịch
        public int TotalTransactions { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public int PendingTransactions { get; set; }

        // Chi tiết để giải thích
        public int TotalProjects { get; set; }
        public int TotalContracts { get; set; }
        public int TotalSuccessfulPaymentTransactions { get; set; }
        public decimal AverageTransactionAmount { get; set; }
        public decimal LargestTransactionAmount { get; set; }
        public decimal SmallestTransactionAmount { get; set; }
    }

    public class MonthlyFinancialDto
    {
        public string Month { get; set; } = string.Empty; // Format: "YYYY-MM"
        public string MonthName { get; set; } = string.Empty; // Format: "Tháng MM/YYYY"
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal Commission { get; set; }
        public int TransactionCount { get; set; }
    }
}

