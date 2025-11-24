using OCSP.Domain.Entities;

namespace OCSP.Application.DTOs.Notification
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime NotificationDate { get; set; }
        public NotificationType Type { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? ActionUrl { get; set; }
        public Guid? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? ActionUrl { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid UserId { get; set; }
    }

    public class MarkNotificationAsReadDto
    {
        public Guid NotificationId { get; set; }
    }
}
