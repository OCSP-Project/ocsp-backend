 using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Contractor
{
 public class ContractorDocumentDto
    {
        public Guid Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsVerified { get; set; }
    }}