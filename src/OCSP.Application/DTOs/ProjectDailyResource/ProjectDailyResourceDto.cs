namespace OCSP.Application.DTOs.ProjectDailyResource
{
    public class ProjectDailyResourceDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public DateTime ResourceDate { get; set; }

        // Thiết bị (boolean)
        public bool TowerCrane { get; set; }
        public bool ConcreteMixer { get; set; }
        public bool MaterialHoist { get; set; }
        public bool PassengerHoist { get; set; }
        public bool Vibrator { get; set; }

        // Vật liệu - Xi măng
        public decimal CementConsumed { get; set; }
        public decimal CementRemaining { get; set; }

        // Vật liệu - Cát
        public decimal SandConsumed { get; set; }
        public decimal SandRemaining { get; set; }

        // Vật liệu - Đá
        public decimal AggregateConsumed { get; set; }
        public decimal AggregateRemaining { get; set; }

        // Ghi chú
        public string? Notes { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
