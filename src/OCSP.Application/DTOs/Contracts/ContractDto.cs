using System;

namespace OCSP.Application.DTOs.Contracts
{
    public class ContractDto
    {
        public Guid Id { get; set; }
        public Guid ProposalId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid ContractorUserId { get; set; }
        public Guid HomeownerUserId { get; set; }

        public string Terms { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty; // Enum dưới dạng string

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
