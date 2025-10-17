using OCSP.Domain.Common;
using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{
    public class Proposal : AuditableEntity
    {
        public Guid QuoteRequestId { get; set; }
        public QuoteRequest QuoteRequest { get; set; } = default!;

        public Guid ContractorUserId { get; set; }   // User.Id của nhà thầu

        public ProposalStatus Status { get; set; } = ProposalStatus.Draft;

        public decimal PriceTotal { get; set; }      // tổng giá
        public int DurationDays { get; set; }        // thời gian thi công (ngày)
        public string? TermsSummary { get; set; }    // điều khoản tóm tắt

        public ICollection<ProposalItem> Items { get; set; } = new List<ProposalItem>();
        
        /// <summary>
        /// Indicates if this proposal was created from Excel upload
        /// </summary>
        public bool IsFromExcel { get; set; } = false;
        
        /// <summary>
        /// Original Excel filename
        /// </summary>
        public string? ExcelFileName { get; set; }
        
        // Project Information from Excel
        /// <summary>
        /// Project title from Excel
        /// </summary>
        public string? ProjectTitle { get; set; }
        
        /// <summary>
        /// Construction area (e.g., "196.53 m²")
        /// </summary>
        public string? ConstructionArea { get; set; }
        
        /// <summary>
        /// Construction time (e.g., "6 tháng")
        /// </summary>
        public string? ConstructionTime { get; set; }
        
        /// <summary>
        /// Number of workers (e.g., "7 người")
        /// </summary>
        public string? NumberOfWorkers { get; set; }
        
        /// <summary>
        /// Average salary (e.g., "12.000.000 đ/người/tháng")
        /// </summary>
        public string? AverageSalary { get; set; }
    }
}
