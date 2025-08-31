using System;
using System.Collections.Generic;

namespace OCSP.Application.DTOs.Contracts
{
    public class CreateContractDto
    {
        public Guid ProposalId { get; set; }          // Hợp đồng được tạo từ Proposal nào
        public string Terms { get; set; } = string.Empty; // Điều khoản hợp đồng
        public List<CreateContractItemDto> Items { get; set; } = new();
    }
}
