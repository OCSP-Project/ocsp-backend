using System.ComponentModel.DataAnnotations;

namespace OCSP.Application.DTOs.ProjectDailyResource
{
    public class CreateProjectDailyResourceDto
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public DateTime ResourceDate { get; set; }

        // Thiết bị (boolean)
        public bool TowerCrane { get; set; } = false;
        public bool ConcreteMixer { get; set; } = false;
        public bool MaterialHoist { get; set; } = false;
        public bool PassengerHoist { get; set; } = false;
        public bool Vibrator { get; set; } = false;

        // Vật liệu - Xi măng
        [Range(0, double.MaxValue, ErrorMessage = "Lượng xi măng tiêu thụ phải >= 0")]
        public decimal CementConsumed { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Lượng xi măng còn lại phải >= 0")]
        public decimal CementRemaining { get; set; } = 0;

        // Vật liệu - Cát
        [Range(0, double.MaxValue, ErrorMessage = "Lượng cát tiêu thụ phải >= 0")]
        public decimal SandConsumed { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Lượng cát còn lại phải >= 0")]
        public decimal SandRemaining { get; set; } = 0;

        // Vật liệu - Đá
        [Range(0, double.MaxValue, ErrorMessage = "Lượng đá tiêu thụ phải >= 0")]
        public decimal AggregateConsumed { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Lượng đá còn lại phải >= 0")]
        public decimal AggregateRemaining { get; set; } = 0;

        // Ghi chú
        [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
        public string? Notes { get; set; }
    }
}
