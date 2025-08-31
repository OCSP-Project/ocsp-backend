using System;

namespace OCSP.Application.DTOs.Contracts
{
    public class CreateContractItemDto
    {
        public string Name { get; set; } = string.Empty;    // Tên hạng mục
        public decimal Qty { get; set; }                   // Số lượng
        public string Unit { get; set; } = string.Empty;    // Đơn vị
        public decimal UnitPrice { get; set; }             // Giá 1 đơn vị
    }
}
