using OCSP.Application.DTOs.News;

namespace OCSP.Application.Services.Interfaces
{
    public interface INewsService
    {
        // Webhook from n8n
        Task<NewsDto> ReceiveFromN8nAsync(N8nNewsWebhookDto dto);

        // Public endpoints
        Task<List<NewsDto>> GetPublishedNewsAsync(int page = 1, int pageSize = 10, string? category = null);
        Task<NewsDto?> GetNewsByIdAsync(Guid id);
        Task IncrementViewCountAsync(Guid id);

        // Admin endpoints
        Task<List<NewsDto>> GetAllNewsAsync(bool? isPublished = null, int page = 1, int pageSize = 20);
        Task<NewsDto> CreateNewsAsync(CreateNewsDto dto);
        Task<NewsDto> UpdateNewsAsync(Guid id, UpdateNewsDto dto);
        Task DeleteNewsAsync(Guid id);
        Task<NewsDto> PublishNewsAsync(Guid id);
        Task<NewsDto> UnpublishNewsAsync(Guid id);
        Task<NewsDto> ScheduleNewsAsync(Guid id, ScheduleNewsDto dto);
        Task ProcessScheduledNewsAsync(); // Background job
    }
}
