using System.ComponentModel.DataAnnotations;

namespace OCSP.Application.DTOs.ProjectDailyResource
{
    public class UpdateProjectDailyResourceDto
    {
        [Required]
        public DateTime ResourceDate { get; set; }

        // Thiết bị (boolean)
        public bool TowerCrane { get; set; }
        public bool ConcreteMixer { get; set; }
        public bool MaterialHoist { get; set; }
        public bool PassengerHoist { get; set; }
        public bool Vibrator { get; set; }

        // Vật liệu - Xi măng
        [Range(0, double.MaxValue, ErrorMessage = "Lượng xi măng tiêu thụ phải >= 0")]
        public decimal CementConsumed { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Lượng xi măng còn lại phải >= 0")]
        public decimal CementRemaining { get; set; }

        // Vật liệu - Cát
        [Range(0, double.MaxValue, ErrorMessage = "Lượng cát tiêu thụ phải >= 0")]
        public decimal SandConsumed { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Lượng cát còn lại phải >= 0")]
        public decimal SandRemaining { get; set; }

        // Vật liệu - Đá
        [Range(0, double.MaxValue, ErrorMessage = "Lượng đá tiêu thụ phải >= 0")]
        public decimal AggregateConsumed { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Lượng đá còn lại phải >= 0")]
        public decimal AggregateRemaining { get; set; }

        // Ghi chú
        [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
        public string? Notes { get; set; }
    }
}
