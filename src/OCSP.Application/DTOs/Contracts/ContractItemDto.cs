using System;

namespace OCSP.Application.DTOs.Contracts
{
    public class ContractItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal Total => Qty * UnitPrice;
    }
}
