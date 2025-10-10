using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class ProjectDailyResource : AuditableEntity
    {
        // Override Id property để sử dụng DailyResourceId
        public new Guid Id { get; set; } = Guid.NewGuid();
        
        // Alias cho Id để dễ hiểu
        public Guid DailyResourceId => Id;

        // Khóa ngoại tham chiếu đến Project
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

        // Ngày sử dụng tài nguyên
        public DateTime ResourceDate { get; set; }

        // Thiết bị (boolean: true/false)
        public bool TowerCrane { get; set; }           // cần trục tháp
        public bool ConcreteMixer { get; set; }        // máy trộn
        public bool MaterialHoist { get; set; }        // vận thăng tải
        public bool PassengerHoist { get; set; }       // vận thăng lồng
        public bool Vibrator { get; set; }             // đầm dùi

        // Vật liệu - Xi măng
        public decimal CementConsumed { get; set; }     // lượng tiêu thụ xi măng hằng ngày
        public decimal CementRemaining { get; set; }    // lượng xi măng còn lại trong kho hằng ngày

        // Vật liệu - Cát
        public decimal SandConsumed { get; set; }       // lượng tiêu thụ cát hằng ngày
        public decimal SandRemaining { get; set; }      // lượng cát còn lại trong kho hằng ngày

        // Vật liệu - Đá
        public decimal AggregateConsumed { get; set; }  // lượng tiêu thụ đá hằng ngày
        public decimal AggregateRemaining { get; set; } // lượng đá còn lại trong kho hằng ngày

        // Ghi chú thêm (tùy chọn)
        public string? Notes { get; set; }
    }
}
