using Microsoft.EntityFrameworkCore;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.Repositories.Interfaces;

namespace OCSP.Infrastructure.Repositories
{
    public class ProgressMediaRepository : IProgressMediaRepository
    {
        private readonly ApplicationDbContext _db;

        public ProgressMediaRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ProgressMedia?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.ProgressMedias
                .Include(pm => pm.Creator)
                .FirstOrDefaultAsync(pm => pm.Id == id, ct);
        }

        public async Task<List<ProgressMedia>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _db.ProgressMedias
                .Include(pm => pm.Creator)
                .Where(pm => pm.ProjectId == projectId)
                .OrderByDescending(pm => pm.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<List<ProgressMedia>> GetByProjectIdAsync(Guid projectId, Guid? taskId, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.ProgressMedias
                .Include(pm => pm.Creator)
                .Where(pm => pm.ProjectId == projectId);

            if (taskId.HasValue)
                query = query.Where(pm => pm.TaskId == taskId);

            if (fromDate.HasValue)
                query = query.Where(pm => pm.CreatedAt >= fromDate);

            if (toDate.HasValue)
                query = query.Where(pm => pm.CreatedAt <= toDate);

            return await query
                .OrderByDescending(pm => pm.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<int> GetCountByProjectIdAsync(Guid projectId, Guid? taskId, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
        {
            var query = _db.ProgressMedias
                .Where(pm => pm.ProjectId == projectId);

            if (taskId.HasValue)
                query = query.Where(pm => pm.TaskId == taskId);

            if (fromDate.HasValue)
                query = query.Where(pm => pm.CreatedAt >= fromDate);

            if (toDate.HasValue)
                query = query.Where(pm => pm.CreatedAt <= toDate);

            return await query.CountAsync(ct);
        }

        public async Task<ProgressMedia> AddAsync(ProgressMedia progressMedia, CancellationToken ct = default)
        {
            await _db.ProgressMedias.AddAsync(progressMedia, ct);
            return progressMedia;
        }

        public Task DeleteAsync(ProgressMedia progressMedia, CancellationToken ct = default)
        {
            _db.ProgressMedias.Remove(progressMedia);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
