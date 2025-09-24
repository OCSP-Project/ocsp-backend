namespace OCSP.Application.DTOs.ProjectDailyResource
{
    public class ProjectDailyResourceListDto
    {
        public Guid Id { get; set; }
        public DateTime ResourceDate { get; set; }

        // Trạng thái từng thiết bị trong ngày
        public bool TowerCrane { get; set; }
        public bool ConcreteMixer { get; set; }
        public bool MaterialHoist { get; set; }
        public bool PassengerHoist { get; set; }
        public bool Vibrator { get; set; }

        // Tổng thiết bị được sử dụng
        public int EquipmentCount { get; set; }

        // Tổng vật liệu tiêu thụ
        public decimal TotalCementConsumed { get; set; }
        public decimal TotalSandConsumed { get; set; }
        public decimal TotalAggregateConsumed { get; set; }

        // Tổng vật liệu còn lại
        public decimal TotalCementRemaining { get; set; }
        public decimal TotalSandRemaining { get; set; }
        public decimal TotalAggregateRemaining { get; set; }

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
