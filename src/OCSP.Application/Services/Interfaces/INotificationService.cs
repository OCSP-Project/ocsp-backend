using OCSP.Application.DTOs.Notification;
using OCSP.Domain.Entities;

namespace OCSP.Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(Guid userId, string title, string message, CancellationToken ct = default);
        Task<NotificationDto> CreateAsync(CreateNotificationDto dto, CancellationToken ct = default);
        Task CreateForProjectParticipantsAsync(
            Guid projectId,
            string title,
            string message,
            NotificationType type,
            Guid? referenceId = null,
            string? actionUrl = null,
            Guid? excludeUserId = null,
            CancellationToken ct = default);
        Task<List<NotificationDto>> GetByUserIdAsync(Guid userId, bool unreadOnly = false, int limit = 50, CancellationToken ct = default);
        Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
        Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default);
        Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
    }
}