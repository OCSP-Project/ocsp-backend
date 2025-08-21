// OCSP.Application/Services/SupervisorService.cs
using Microsoft.EntityFrameworkCore;
using OCSP.Application.DTOs.Common;
using OCSP.Application.DTOs.Supervisor;
using OCSP.Application.Services.Interfaces;
using OCSP.Infrastructure.Data;

namespace OCSP.Application.Services
{
    public class SupervisorService : ISupervisorService
    {
        private readonly ApplicationDbContext _db;
        public SupervisorService(ApplicationDbContext db) => _db = db;

        public async Task<PagedResult<SupervisorListItemDto>> FilterAsync(
            FilterSupervisorsRequest req, CancellationToken ct)
        {
            var q = _db.Supervisors
                .Include(s => s.User)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(req.District))
                q = q.Where(s => s.District != null && s.District.ToLower() == req.District.ToLower());

            if (req.MinRating.HasValue)
                q = q.Where(s => s.Rating.HasValue && s.Rating >= req.MinRating);

            if (req.PriceMin.HasValue)
                q = q.Where(s => s.MinRate.HasValue && s.MinRate >= req.PriceMin);

            if (req.PriceMax.HasValue)
                q = q.Where(s => s.MaxRate.HasValue && s.MaxRate <= req.PriceMax);

            if (req.AvailableNow == true)
                q = q.Where(s => s.AvailableNow);

            var total = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(s => s.Rating ?? 0)   // default sort: rating cao trước
                .ThenBy(s => s.MinRate ?? 0)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .Select(s => new SupervisorListItemDto
                {
                    Id = s.Id,
                    Username = s.User!.Username,
                    Email = s.User!.Email,
                    Department = s.Department,
                    Position = s.Position,
                    District = s.District,
                    Rating = s.Rating,
                    ReviewsCount = s.ReviewsCount,
                    MinRate = s.MinRate,
                    MaxRate = s.MaxRate,
                    AvailableNow = s.AvailableNow
                })
                .ToListAsync(ct);

            return new PagedResult<SupervisorListItemDto>
            {
                Page = req.Page,
                PageSize = req.PageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)req.PageSize),
                Items = items
            };
        }

        public async Task<SupervisorDetailsDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _db.Supervisors
                .Include(s => s.User)
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new SupervisorDetailsDto
                {
                    Id = s.Id,
                    Username = s.User!.Username,
                    Email = s.User!.Email,
                    Phone = s.Phone,
                    Department = s.Department,
                    Position = s.Position,
                    District = s.District,
                    Rating = s.Rating,
                    ReviewsCount = s.ReviewsCount,
                    MinRate = s.MinRate,
                    MaxRate = s.MaxRate,
                    AvailableNow = s.AvailableNow,
                    Bio = null,
                    YearsExperience = null
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<SupervisorListItemDto>> GetAllAsync(CancellationToken ct)
{
    return await _db.Supervisors
        .Include(s => s.User)
        .AsNoTracking()
        .OrderByDescending(s => s.Rating ?? 0)
        .Select(s => new SupervisorListItemDto
        {
            Id = s.Id,
            Username = s.User!.Username,
            Email = s.User!.Email,
            Department = s.Department,
            Position = s.Position,
            District = s.District,
            Rating = s.Rating,
            ReviewsCount = s.ReviewsCount,
            MinRate = s.MinRate,
            MaxRate = s.MaxRate,
            AvailableNow = s.AvailableNow
        })
        .ToListAsync(ct);
}

    }
}
