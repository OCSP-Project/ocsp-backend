using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Repositories.Interfaces
{
    public interface IBuildingElementRepository
    {
        Task<BuildingElement?> GetByIdAsync(Guid id);
        Task<List<BuildingElement>> GetByModelIdAsync(Guid modelId);
        Task AddAsync(BuildingElement entity);
        Task UpdateAsync(BuildingElement entity);
        Task DeleteAsync(Guid id);
        Task<int> SaveChangesAsync();
    }

    public interface IElementTrackingHistoryRepository
    {
        Task<ElementTrackingHistory?> GetByIdAsync(Guid id);
        Task<List<ElementTrackingHistory>> GetByElementIdAsync(Guid elementId);
        Task AddAsync(ElementTrackingHistory entity);
        Task<int> SaveChangesAsync();
    }

    public interface ITrackingPhotoRepository
    {
        Task<List<TrackingPhoto>> GetByHistoryIdAsync(Guid historyId);
        Task AddAsync(TrackingPhoto photo);
        Task DeleteAsync(Guid id);
        Task<int> SaveChangesAsync();
    }
}
