// OCSP.Domain/Entities/SupervisorContract.cs
using OCSP.Domain.Common;
using OCSP.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCSP.Domain.Entities
{
    public class SupervisorContract : AuditableEntity
    {
        [ForeignKey("ProjectId")]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;

        [ForeignKey("SupervisorId")]
        public Guid SupervisorId { get; set; }
        public Supervisor Supervisor { get; set; } = default!;

        public Guid HomeownerUserId { get; set; }
        public Guid SupervisorUserId { get; set; }

        public decimal MonthlyPrice { get; set; }

        public string Terms { get; set; } = string.Empty;

        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        public DateTime? SignedByHomeownerAt { get; set; }
        public DateTime? SignedBySupervisorAt { get; set; }

        // Digital Signatures (Base64)
        public string? HomeownerSignatureBase64 { get; set; }
        public string? SupervisorSignatureBase64 { get; set; }

        // PDF Files
        public string? TemplatePdfUrl { get; set; }
        public string? SignedPdfUrl { get; set; }
    }
}



