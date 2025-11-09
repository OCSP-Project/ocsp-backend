using Microsoft.EntityFrameworkCore;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.Repositories.Interfaces;

namespace OCSP.Infrastructure.Repositories
{
    public class Project3DModelRepository : IProject3DModelRepository
    {
        private readonly ApplicationDbContext _db;

        public Project3DModelRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Project3DModel?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _db.Set<Project3DModel>()
                .Include(m => m.Project)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(x => x.ProjectId == projectId, cancellationToken);
        }

        public async Task<List<Project3DModel>> ListByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _db.Set<Project3DModel>()
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Project3DModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Set<Project3DModel>()
                .Include(m => m.Project)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<Project3DModel?> GetLatestByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _db.Set<Project3DModel>()
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task CreateAsync(Project3DModel model, CancellationToken cancellationToken = default)
        {
            await _db.Set<Project3DModel>().AddAsync(model, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var model = await _db.Set<Project3DModel>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (model != null)
            {
                _db.Remove(model);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
