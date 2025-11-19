namespace OCSP.Application.DTOs.Material
{
    public class MaterialPaymentDto
    {
        public Guid Id { get; set; }
        public Guid MaterialId { get; set; }
        public Guid ProjectId { get; set; }

        public DateTime PaymentDate { get; set; }

        public decimal PaidQuantity { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal RemainingAmount { get; set; }

        public string? PaymentMethod { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }

        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }

    public class CreateMaterialPaymentDto
    {
        public Guid MaterialId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PaidQuantity { get; set; }
        public string? PaymentMethod { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }
    }
}
