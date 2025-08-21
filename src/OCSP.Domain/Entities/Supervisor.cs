// OCSP.Domain/Entities/Supervisor.cs
using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class Supervisor : AuditableEntity   // AuditableEntity đã có Id, CreatedAt, UpdatedAt
    {
        // Thông tin cơ bản
        public string Department { get; set; } = string.Empty;
        public string Position   { get; set; } = string.Empty;
        public string Phone      { get; set; } = string.Empty;

        // Địa điểm (quận ở Đà Nẵng)
        public string? District  { get; set; }   // ví dụ "Hai Chau", "Thanh Khe", ...

        // Hiển thị & lọc
        public double?  Rating        { get; set; }   // 0..5
        public int?     ReviewsCount  { get; set; }
        public decimal? MinRate       { get; set; }   // đơn giá tối thiểu
        public decimal? MaxRate       { get; set; }   // đơn giá tối đa
        public bool     AvailableNow  { get; set; }   // có thể nhận ngay

        // Quan hệ
        public Guid  UserId { get; set; }
        public User? User   { get; set; }
    }
}
