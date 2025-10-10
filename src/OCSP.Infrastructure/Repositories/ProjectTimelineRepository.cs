using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OCSP.Infrastructure.Repositories
{
    public class ProjectTimelineRepository : IProjectTimelineRepository
    {
        private readonly ApplicationDbContext _db;

        public ProjectTimelineRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(ProjectTimeline timeline)
        {
            await _db.ProjectTimelines.AddAsync(timeline);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<ProjectTimeline?> GetByProjectIdWithDetailsAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _db.ProjectTimelines
                .Include(t => t.Milestones)
                    .ThenInclude(m => m.Deliverables)
                .FirstOrDefaultAsync(t => t.ProjectId == projectId, ct);
        }
    }
}