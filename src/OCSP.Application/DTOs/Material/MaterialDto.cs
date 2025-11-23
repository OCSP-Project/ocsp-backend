namespace OCSP.Application.DTOs.Material
{
    public class MaterialDto
    {
        public Guid Id { get; set; }
        public Guid MaterialRequestId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? WorkItemId { get; set; }
        public string? WorkItemName { get; set; }

        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }

        // Quantities
        public decimal? ContractQuantity { get; set; }
        public decimal EstimatedQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }

        // Amounts
        public decimal? ContractAmount { get; set; }
        public decimal EstimatedAmount { get; set; }
        public decimal? ActualAmount { get; set; }

        // Payment summary
        public decimal TotalPaidQuantity { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal RemainingAmount { get; set; }

        public string? Description { get; set; }
        public string? Supplier { get; set; }
        public string? Notes { get; set; }

        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MaterialDetailDto : MaterialDto
    {
        public List<MaterialPaymentDto> Payments { get; set; } = new();
    }

    public class UpdateMaterialDto
    {
        public decimal? ContractQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateActualQuantityDto
    {
        public decimal ActualQuantity { get; set; }
        public string? Notes { get; set; }
    }
}
