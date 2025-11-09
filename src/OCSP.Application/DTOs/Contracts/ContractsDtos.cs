using System;
using System.Collections.Generic;
using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Contracts
{
    public class CreateContractItemDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
    }

    public class UpdateContractStatusDto
    {
        public Guid ContractId { get; set; }
        public ContractStatus Status { get; set; }
    }

    public class CreateContractDto
    {
        public Guid ProposalId { get; set; }
        public string Terms { get; set; } = string.Empty;
        public List<CreateContractItemDto> Items { get; set; } = new();
    }

    public class ContractListItemDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ContractorName { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ContractItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal Total => Qty * UnitPrice;
    }


    public class ContractDto
    {
        public Guid Id { get; set; }
        public Guid ProposalId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid ContractorUserId { get; set; }
        public Guid HomeownerUserId { get; set; }
        public string Terms { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public int DurationDays { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Digital Signatures
        public string? HomeownerSignatureBase64 { get; set; }
        public string? ContractorSignatureBase64 { get; set; }
        public DateTime? SignedByHomeownerAt { get; set; }
        public DateTime? SignedByContractorAt { get; set; }
        
        // PDF URLs
        public string? TemplatePdfUrl { get; set; }
        public string? SignedPdfUrl { get; set; }
    }
    
    public class SignContractDto
    {
        public string SignatureBase64 { get; set; } = string.Empty;
    }
}
