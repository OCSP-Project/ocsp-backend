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
    }
}