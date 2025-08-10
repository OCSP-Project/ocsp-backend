using Microsoft.EntityFrameworkCore;
using OCSP.Application.DTOs.Supervisor;
using OCSP.Application.Services.Interfaces;
using OCSP.Infrastructure.Data;

namespace OCSP.Application.Services
{
    public class SupervisorService : ISupervisorService
    {
        private readonly ApplicationDbContext _db;
        public SupervisorService(ApplicationDbContext db) => _db = db;

        public async Task<List<SupervisorListDto>> GetAllAsync()
        {
            return await _db.Supervisors
                .Include(s => s.User)
                .AsNoTracking()
                .Select(s => new SupervisorListDto
                {
                    Id = s.Id,
                    Department = s.Department,
                    Position = s.Position,
                    Phone = s.Phone,
                    Username = s.User!.Username,
                    Email = s.User!.Email
                })
                .ToListAsync();
        }

        public async Task<SupervisorDetailDto?> GetByIdAsync(Guid id)
        {
            var s = await _db.Supervisors
                .Include(x => x.User)
                .Include(x => x.Projects)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (s == null) return null;

            return new SupervisorDetailDto
            {
                Id = s.Id,
                Department = s.Department,
                Position = s.Position,
                Phone = s.Phone,
                Username = s.User!.Username,
                Email = s.User!.Email,
                Projects = (s.Projects ?? new List<OCSP.Domain.Entities.Project>())
                    .Select(p => new ProjectItemDto { Id = p.Id, Name = p.Name, Status = p.Status })
                    .ToList()
            };
        }
    }
}