namespace OCSP.Domain.Enums
{
    public enum ContractStatus
    {
        Draft = 0,             // Hợp đồng đang soạn thảo
        PendingSignatures = 1, // Đang chờ ký kết
        Active = 2,            // Đang hiệu lực
        Completed = 3,         // Đã hoàn thành
        Cancelled = 4          // Bị hủy
    }
}
