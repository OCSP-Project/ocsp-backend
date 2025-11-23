using Microsoft.EntityFrameworkCore;
using OCSP.Application.DTOs.Notification;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;

namespace OCSP.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(Guid userId, string title, string message, CancellationToken ct = default)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                NotificationDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(ct);
        }

        // Create notification for specific user
        public async Task<NotificationDto> CreateAsync(CreateNotificationDto dto, CancellationToken ct = default)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                ReferenceId = dto.ReferenceId,
                ActionUrl = dto.ActionUrl,
                ProjectId = dto.ProjectId,
                UserId = dto.UserId,
                NotificationDate = DateTime.UtcNow,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.UserId.ToString(),
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(ct);

            // Load project if exists
            if (notification.ProjectId.HasValue)
            {
                await _context.Entry(notification)
                    .Reference(n => n.Project)
                    .LoadAsync(ct);
            }

            return MapToDto(notification);
        }

        // Create notifications for all project participants
        public async Task CreateForProjectParticipantsAsync(
            Guid projectId,
            string title,
            string message,
            NotificationType type,
            Guid? referenceId = null,
            string? actionUrl = null,
            Guid? excludeUserId = null,
            CancellationToken ct = default)
        {
            // Get all participants in the project
            var participantIds = await _context.ProjectParticipants
                .Where(p => p.ProjectId == projectId)
                .Select(p => p.UserId)
                .ToListAsync(ct);

            // Add homeowner
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId, ct);

            if (project != null && project.HomeownerId != Guid.Empty)
            {
                participantIds.Add(project.HomeownerId);
            }

            // Remove duplicates and excluded user
            participantIds = participantIds
                .Distinct()
                .Where(id => !excludeUserId.HasValue || id != excludeUserId.Value)
                .ToList();

            // Create notifications for all participants
            var notifications = participantIds.Select(userId => new Notification
            {
                Id = Guid.NewGuid(),
                Title = title,
                Message = message,
                Type = type,
                ReferenceId = referenceId,
                ActionUrl = actionUrl,
                ProjectId = projectId,
                UserId = userId,
                NotificationDate = DateTime.UtcNow,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync(ct);
        }

        // Get notifications for user
        public async Task<List<NotificationDto>> GetByUserIdAsync(Guid userId, bool unreadOnly = false, int limit = 50, CancellationToken ct = default)
        {
            var query = _context.Notifications
                .Include(n => n.Project)
                .Where(n => n.UserId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            var notifications = await query
                .OrderByDescending(n => n.NotificationDate)
                .Take(limit)
                .ToListAsync(ct);

            return notifications.Select(n => MapToDto(n)).ToList();
        }

        // Get unread count
        public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync(ct);
        }

        // Mark as read
        public async Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);

            if (notification == null)
                throw new ArgumentException("Notification not found");

            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        // Mark all as read
        public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(ct);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);
        }

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                NotificationDate = notification.NotificationDate,
                Type = notification.Type,
                ReferenceId = notification.ReferenceId,
                ActionUrl = notification.ActionUrl,
                ProjectId = notification.ProjectId,
                ProjectName = notification.Project?.Name,
                UserId = notification.UserId,
                CreatedAt = notification.CreatedAt,
            };
        }
    }
}