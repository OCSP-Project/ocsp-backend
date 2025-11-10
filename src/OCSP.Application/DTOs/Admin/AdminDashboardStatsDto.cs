namespace OCSP.Application.DTOs.Admin
{
    public class AdminDashboardStatsDto
    {
        // Basic Counts
        public int TotalUsers { get; set; }
        public int TotalProjects { get; set; }
        public int TotalProposals { get; set; }
        public int TotalQuoteRequests { get; set; }
        public int TotalContracts { get; set; }

        // Financial
        public decimal TotalTransactionValue { get; set; } // Tổng giá trị giao dịch (từ PaymentTransaction hoặc Contract)
        public decimal TotalCommission { get; set; } // Tổng hoa hồng (có thể tính từ Contract TotalPrice * commission rate)

        // Breakdowns
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int PendingProposals { get; set; }
        public int ActiveContracts { get; set; }
        public int CompletedContracts { get; set; }
    }
}

