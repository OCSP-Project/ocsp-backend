using Microsoft.EntityFrameworkCore;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OCSP.Infrastructure.Repositories
{
    public class ProjectDailyResourceRepository : IProjectDailyResourceRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectDailyResourceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectDailyResource?> GetByIdAsync(Guid dailyResourceId, CancellationToken cancellationToken = default)
        {
            return await _context.ProjectDailyResources
                .Include(pdr => pdr.Project)
                .FirstOrDefaultAsync(pdr => pdr.Id == dailyResourceId, cancellationToken);
        }

        public async Task<ProjectDailyResource?> GetByProjectAndDateAsync(Guid projectId, DateTime resourceDate, CancellationToken cancellationToken = default)
        {
            return await _context.ProjectDailyResources
                .FirstOrDefaultAsync(pdr => pdr.ProjectId == projectId && pdr.ResourceDate.Date == resourceDate.Date, cancellationToken);
        }

        public async Task<List<ProjectDailyResource>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.ProjectDailyResources
                .Where(pdr => pdr.ProjectId == projectId)
                .OrderByDescending(pdr => pdr.ResourceDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ProjectDailyResource>> GetByProjectIdAndDateRangeAsync(Guid projectId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _context.ProjectDailyResources
                .Where(pdr => pdr.ProjectId == projectId && 
                             pdr.ResourceDate.Date >= startDate.Date && 
                             pdr.ResourceDate.Date <= endDate.Date)
                .OrderBy(pdr => pdr.ResourceDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<ProjectDailyResource> CreateAsync(ProjectDailyResource entity, CancellationToken cancellationToken = default)
        {
            _context.ProjectDailyResources.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<ProjectDailyResource> UpdateAsync(ProjectDailyResource entity, CancellationToken cancellationToken = default)
        {
            _context.ProjectDailyResources.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(Guid dailyResourceId, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ProjectDailyResources.FindAsync(dailyResourceId);
            if (entity == null) return false;

            _context.ProjectDailyResources.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ExistsAsync(Guid projectId, DateTime resourceDate, CancellationToken cancellationToken = default)
        {
            return await _context.ProjectDailyResources
                .AnyAsync(pdr => pdr.ProjectId == projectId && pdr.ResourceDate.Date == resourceDate.Date, cancellationToken);
        }
    }
}
