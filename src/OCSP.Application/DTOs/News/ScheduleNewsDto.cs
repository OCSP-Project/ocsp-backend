using System.ComponentModel.DataAnnotations;

namespace OCSP.Application.DTOs.News
{
    public class ScheduleNewsDto
    {
        [Required]
        public DateTime ScheduledPublishAt { get; set; }
    }
}
