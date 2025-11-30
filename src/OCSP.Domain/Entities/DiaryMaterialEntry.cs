using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class DiaryMaterialEntry : AuditableEntity
    {
        // Foreign Keys
        public Guid ConstructionDiaryId { get; set; }
        public ConstructionDiary? ConstructionDiary { get; set; }

        public Guid MaterialId { get; set; }
        public Material? Material { get; set; }

        // Material Details (snapshot at time of diary entry)
        public string MaterialName { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string Unit { get; set; } = string.Empty;

        // Quantities
        public decimal ContractQuantity { get; set; }                   // KL hợp đồng
        public decimal ActualQuantity { get; set; }                     // KL thực tế
        public decimal? Variance { get; set; }                          // Chênh lệch %
    }
}
