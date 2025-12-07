using System;
using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Contracts
{
    public class SupervisorContractDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public Guid SupervisorId { get; set; }
        public Guid SupervisorUserId { get; set; }
        public string SupervisorName { get; set; } = string.Empty;
        public Guid HomeownerUserId { get; set; }
        public string HomeownerName { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public string Terms { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Digital Signatures
        public string? HomeownerSignatureBase64 { get; set; }
        public string? SupervisorSignatureBase64 { get; set; }
        public DateTime? SignedByHomeownerAt { get; set; }
        public DateTime? SignedBySupervisorAt { get; set; }
        
        // PDF URLs
        public string? TemplatePdfUrl { get; set; }
        public string? SignedPdfUrl { get; set; }
    }

    public class SupervisorContractListItemDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string SupervisorName { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class SignSupervisorContractDto
    {
        public string SignatureBase64 { get; set; } = string.Empty;
    }

    public class CreateSupervisorContractDto
    {
        public Guid ProjectId { get; set; }
        public decimal MonthlyPrice { get; set; }
    }

    public class CreateSupervisorContractWithSupervisorDto
    {
        public Guid ProjectId { get; set; }
        public Guid SupervisorId { get; set; }
        public decimal MonthlyPrice { get; set; }
    }
}
