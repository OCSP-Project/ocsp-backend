using Microsoft.EntityFrameworkCore;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _db;
    public ProjectRepository(ApplicationDbContext db) => _db = db;

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Projects
            .Include(p => p.Participants)
            .Include(p => p.Supervisor) // optional
            .Include(p => p.Homeowner)  // optional
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<Project>> GetByHomeownerIdAsync(Guid homeownerId, CancellationToken ct = default)
    {
        return await _db.Projects
            .AsNoTracking()
            .Include(p => p.Homeowner)
            .Include(p => p.Supervisor)
                .ThenInclude(s => s.User)
            .Where(p => p.HomeownerId == homeownerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public Task AddAsync(Project project, CancellationToken ct = default)
        => _db.Projects.AddAsync(project, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
