using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.Repositories.Interfaces;

namespace OCSP.Infrastructure.Repositories
{
    public class BuildingElementRepository : IBuildingElementRepository
    {
        private readonly ApplicationDbContext _db;
        public BuildingElementRepository(ApplicationDbContext db) { _db = db; }
        public async Task<BuildingElement?> GetByIdAsync(Guid id) =>
            await _db.BuildingElements.FindAsync(id);
        public async Task<List<BuildingElement>> GetByModelIdAsync(Guid modelId) =>
            await _db.BuildingElements.Where(e => e.ModelId == modelId)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        public async Task AddAsync(BuildingElement entity) { await _db.BuildingElements.AddAsync(entity); }
        public async Task UpdateAsync(BuildingElement entity) { _db.BuildingElements.Update(entity); await Task.CompletedTask; }
        public async Task DeleteAsync(Guid id)
        {
            var e = await _db.BuildingElements.FindAsync(id);
            if (e != null) _db.BuildingElements.Remove(e);
        }
        public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
    }

    public class ElementTrackingHistoryRepository : IElementTrackingHistoryRepository
    {
        private readonly ApplicationDbContext _db;
        public ElementTrackingHistoryRepository(ApplicationDbContext db) { _db = db; }
        public async Task<ElementTrackingHistory?> GetByIdAsync(Guid id) =>
            await _db.ElementTrackingHistory.FindAsync(id);
        public async Task<List<ElementTrackingHistory>> GetByElementIdAsync(Guid elementId) =>
            await _db.ElementTrackingHistory
                .Where(h => h.BuildingElementId == elementId)
                .OrderByDescending(h => h.TrackingDate)
                .ToListAsync();
        public async Task AddAsync(ElementTrackingHistory entity) { await _db.ElementTrackingHistory.AddAsync(entity); }
        public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
    }

    public class TrackingPhotoRepository : ITrackingPhotoRepository
    {
        private readonly ApplicationDbContext _db;
        public TrackingPhotoRepository(ApplicationDbContext db) { _db = db; }
        public async Task<List<TrackingPhoto>> GetByHistoryIdAsync(Guid historyId) =>
            await _db.TrackingPhotos.Where(p => p.TrackingHistoryId == historyId)
                .OrderBy(p => p.UploadedAt)
                .ToListAsync();
        public async Task AddAsync(TrackingPhoto photo) { await _db.TrackingPhotos.AddAsync(photo); }
        public async Task DeleteAsync(Guid id)
        {
            var p = await _db.TrackingPhotos.FindAsync(id);
            if (p != null) _db.TrackingPhotos.Remove(p);
        }
        public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
