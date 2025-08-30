namespace OCSP.Domain.Common 
{     
    public abstract class AuditableEntity : BaseEntity // ← Inherit từ BaseEntity
    {         
        // Remove: public Guid Id { get; set; }  ← Xóa dòng này
        public DateTime CreatedAt { get; set; }         
        public DateTime UpdatedAt { get; set; }         
        public string? CreatedBy { get; set; }         
        public string? UpdatedBy { get; set; }     
    } 
}