using System;
using System.Collections.Generic;
using System.Linq;
using OCSP.Application.DTOs.Contracts;

namespace OCSP.Application.DTOs.Contracts
{
    public class ContractDetailDto
    {
        public Guid Id { get; set; }
        public Guid ProposalId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid ContractorUserId { get; set; }
        public Guid HomeownerUserId { get; set; }

        public string Terms { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // List chi tiết item
        public List<ContractItemDto> Items { get; set; } = new();
    }
}
