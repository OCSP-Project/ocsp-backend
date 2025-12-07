using Microsoft.EntityFrameworkCore;
using OCSP.Application.Common.Exceptions;
using OCSP.Application.DTOs.News;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;

namespace OCSP.Application.Services
{
    public class NewsService : INewsService
    {
        private readonly ApplicationDbContext _context;

        public NewsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<NewsDto> ReceiveFromN8nAsync(N8nNewsWebhookDto dto)
        {
            // Parse date string from n8n
            DateTime? dateStart = null;
            if (!string.IsNullOrEmpty(dto.DateStart))
            {
                if (DateTime.TryParse(dto.DateStart, out var parsedDate))
                {
                    dateStart = parsedDate;
                }
            }

            var news = new News
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Author = dto.TácGiả ?? "VietnamNet",
                DateStart = dateStart,
                ImageLinks = dto.ImageLink,
                ContentNews = dto.ContentNews,
                OriginalLink = dto.OriginalLink,
                N8nWorkflowId = dto.N8nWorkflowId,
                CrawledAt = DateTime.UtcNow,
                IsPublished = false, // Admin phải duyệt trước khi publish
                CreatedAt = DateTime.UtcNow
            };

            _context.News.Add(news);
            await _context.SaveChangesAsync();

            return MapToDto(news);
        }

        public async Task<List<NewsDto>> GetPublishedNewsAsync(int page = 1, int pageSize = 10, string? category = null)
        {
            var query = _context.News
                .Where(n => n.IsPublished)
                .OrderByDescending(n => n.IsFeatured)
                .ThenByDescending(n => n.PublishedAt ?? n.CreatedAt);

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = (IOrderedQueryable<News>)query.Where(n => n.Category == category);
            }

            var news = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return news.Select(MapToDto).ToList();
        }

        public async Task<NewsDto?> GetNewsByIdAsync(Guid id)
        {
            var news = await _context.News.FindAsync(id);
            return news == null ? null : MapToDto(news);
        }

        public async Task IncrementViewCountAsync(Guid id)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                news.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<NewsDto>> GetAllNewsAsync(bool? isPublished = null, int page = 1, int pageSize = 20)
        {
            var query = _context.News.AsQueryable();

            if (isPublished.HasValue)
            {
                query = query.Where(n => n.IsPublished == isPublished.Value);
            }

            var news = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return news.Select(MapToDto).ToList();
        }

        public async Task<NewsDto> CreateNewsAsync(CreateNewsDto dto)
        {
            var news = new News
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Author = dto.Author,
                DateStart = dto.DateStart,
                ImageLinks = dto.ImageLinks,
                ContentNews = dto.ContentNews,
                OriginalLink = dto.OriginalLink,
                ScheduledPublishAt = dto.ScheduledPublishAt,
                IsPublished = dto.PublishImmediately,
                PublishedAt = dto.PublishImmediately ? DateTime.UtcNow : null,
                IsFeatured = dto.IsFeatured,
                Category = dto.Category,
                Tags = dto.Tags,
                CreatedAt = DateTime.UtcNow
            };

            _context.News.Add(news);
            await _context.SaveChangesAsync();

            return MapToDto(news);
        }

        public async Task<NewsDto> UpdateNewsAsync(Guid id, UpdateNewsDto dto)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
                throw new NotFoundException("News not found");

            if (dto.Title != null) news.Title = dto.Title;
            if (dto.Author != null) news.Author = dto.Author;
            if (dto.DateStart != null) news.DateStart = dto.DateStart;
            if (dto.ImageLinks != null) news.ImageLinks = dto.ImageLinks;
            if (dto.ContentNews != null) news.ContentNews = dto.ContentNews;
            if (dto.OriginalLink != null) news.OriginalLink = dto.OriginalLink;
            if (dto.IsFeatured.HasValue) news.IsFeatured = dto.IsFeatured.Value;
            if (dto.Category != null) news.Category = dto.Category;
            if (dto.Tags != null) news.Tags = dto.Tags;

            news.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(news);
        }

        public async Task DeleteNewsAsync(Guid id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
                throw new NotFoundException("News not found");

            _context.News.Remove(news);
            await _context.SaveChangesAsync();
        }

        public async Task<NewsDto> PublishNewsAsync(Guid id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
                throw new NotFoundException("News not found");

            news.IsPublished = true;
            news.PublishedAt = DateTime.UtcNow;
            news.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(news);
        }

        public async Task<NewsDto> UnpublishNewsAsync(Guid id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
                throw new NotFoundException("News not found");

            news.IsPublished = false;
            news.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(news);
        }

        public async Task<NewsDto> ScheduleNewsAsync(Guid id, ScheduleNewsDto dto)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
                throw new NotFoundException("News not found");

            news.ScheduledPublishAt = dto.ScheduledPublishAt;
            news.IsPublished = false; // Unset published until scheduled time
            news.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(news);
        }

        public async Task ProcessScheduledNewsAsync()
        {
            var now = DateTime.UtcNow;
            var scheduledNews = await _context.News
                .Where(n => !n.IsPublished && n.ScheduledPublishAt <= now)
                .ToListAsync();

            foreach (var news in scheduledNews)
            {
                news.IsPublished = true;
                news.PublishedAt = DateTime.UtcNow;
                news.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private NewsDto MapToDto(News news)
        {
            return new NewsDto
            {
                Id = news.Id,
                Title = news.Title,
                Author = news.Author,
                DateStart = news.DateStart,
                ImageLinks = news.ImageLinks,
                ContentNews = news.ContentNews,
                OriginalLink = news.OriginalLink,
                ScheduledPublishAt = news.ScheduledPublishAt,
                IsPublished = news.IsPublished,
                PublishedAt = news.PublishedAt,
                IsFeatured = news.IsFeatured,
                ViewCount = news.ViewCount,
                Category = news.Category,
                Tags = news.Tags,
                N8nWorkflowId = news.N8nWorkflowId,
                CrawledAt = news.CrawledAt,
                CreatedAt = news.CreatedAt,
                UpdatedAt = news.UpdatedAt
            };
        }
    }
}
