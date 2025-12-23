using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OCSP.Application.DTOs.BuildingElements;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using OCSP.Infrastructure.Services;

namespace OCSP.Application.Services
{
    public interface IBuildingElementService
    {
        Task<List<BuildingElementDto>> GetByModelAsync(Guid modelId);
        Task<BuildingElementDto?> GetAsync(Guid id);
        Task<BuildingElementDetailDto?> GetDetailAsync(Guid id);
        Task<BuildingElementDto> CreateAsync(CreateBuildingElementRequest req, Guid userId);
        Task<BuildingElementDto> UpdateAsync(Guid id, UpdateBuildingElementRequest req, Guid userId);
        Task<TrackingHistoryDto> AddTrackingAsync(Guid elementId, AddTrackingRecordRequest req, Guid userId);
        Task<List<TrackingHistoryDto>> GetHistoryAsync(Guid elementId);
        Task<List<TrackingPhotoDto>> GetPhotosAsync(Guid historyId);
        Task<TrackingPhotoDto> AddPhotoAsync(Guid historyId, AddTrackingPhotoRequest req, Guid userId);
        Task<TrackingPhotoDto> AddPhotoAsync(Guid historyId, IFormFile file, string? caption, Guid userId);
        Task DeletePhotoAsync(Guid photoId);
        Task<ModelSummaryDto> GetModelSummaryAsync(Guid modelId);
    }

    public class BuildingElementService : IBuildingElementService
    {
        private readonly IBuildingElementRepository _elements;
        private readonly IElementTrackingHistoryRepository _histories;
        private readonly ITrackingPhotoRepository _photos;
        private readonly IFileStorageService _fileStorage;

        public BuildingElementService(IBuildingElementRepository elements,
                                      IElementTrackingHistoryRepository histories,
                                      ITrackingPhotoRepository photos,
                                      IFileStorageService fileStorage)
        {
            _elements = elements; _histories = histories; _photos = photos; _fileStorage = fileStorage;
        }

        public async Task<List<BuildingElementDto>> GetByModelAsync(Guid modelId)
        {
            var list = await _elements.GetByModelIdAsync(modelId);
            return list.Select(MapElement).ToList();
        }

        public async Task<BuildingElementDto?> GetAsync(Guid id)
        {
            var e = await _elements.GetByIdAsync(id);
            return e == null ? null : MapElement(e);
        }

        public async Task<BuildingElementDetailDto?> GetDetailAsync(Guid id)
        {
            var e = await _elements.GetByIdAsync(id);
            if (e == null) return null;

            // Get latest tracking history
            var histories = await _histories.GetByElementIdAsync(id);
            var latestHistory = histories.OrderByDescending(h => h.TrackingDate).FirstOrDefault();

            // Get photos of latest history
            var photos = new List<TrackingPhotoDto>();
            if (latestHistory != null)
            {
                var photoEntities = await _photos.GetByHistoryIdAsync(latestHistory.Id);
                photos = photoEntities.Select(MapPhoto).ToList();
            }

            return new BuildingElementDetailDto
            {
                Id = e.Id,
                ModelId = e.ModelId,
                Name = e.Name,
                ElementType = (int)e.ElementType,
                FloorLevel = e.FloorLevel,
                MeshIndicesJson = e.MeshIndices,
                TrackingStatus = (int)e.TrackingStatus,
                CompletionPercentage = e.CompletionPercentage,
                Color = e.Color,
                CreatedAt = e.CreatedAt,
                LatestHistory = latestHistory == null ? null : MapHistory(latestHistory),
                LatestPhotos = photos
            };
        }

        public async Task<BuildingElementDto> CreateAsync(CreateBuildingElementRequest req, Guid userId)
        {
            var entity = new BuildingElement
            {
                Id = Guid.NewGuid(),
                ModelId = req.ModelId,
                Name = req.Name,
                ElementType = (ComponentType)req.ElementType,
                FloorLevel = req.FloorLevel,
                MeshIndices = JsonSerializer.Serialize(req.MeshIndices),
                TrackingStatus = TrackingStatus.NotStarted,
                CompletionPercentage = 0,
                CanTrack = true,
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };
            await _elements.AddAsync(entity);
            await _elements.SaveChangesAsync();
            return MapElement(entity);
        }

        public async Task<BuildingElementDto> UpdateAsync(Guid id, UpdateBuildingElementRequest req, Guid userId)
        {
            var e = await _elements.GetByIdAsync(id) ?? throw new KeyNotFoundException("Element not found");
            if (!string.IsNullOrWhiteSpace(req.Name)) e.Name = req.Name;
            if (req.ElementType.HasValue) e.ElementType = (ComponentType)req.ElementType.Value;
            if (req.FloorLevel.HasValue) e.FloorLevel = req.FloorLevel.Value;
            if (req.MeshIndices != null) e.MeshIndices = JsonSerializer.Serialize(req.MeshIndices);
            if (!string.IsNullOrWhiteSpace(req.Color)) e.Color = req.Color;
            e.UpdatedBy = userId.ToString();
            e.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            await _elements.UpdateAsync(e);
            await _elements.SaveChangesAsync();
            return MapElement(e);
        }

        public async Task<TrackingHistoryDto> AddTrackingAsync(Guid elementId, AddTrackingRecordRequest req, Guid userId)
        {
            var e = await _elements.GetByIdAsync(elementId) ?? throw new KeyNotFoundException("Element not found");
            var history = new ElementTrackingHistory
            {
                Id = Guid.NewGuid(),
                BuildingElementId = elementId,
                TrackingDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                PreviousPercentage = e.CompletionPercentage,
                NewPercentage = req.NewPercentage,
                PreviousStatus = e.TrackingStatus,
                NewStatus = (TrackingStatus)req.NewStatus,
                PlannedQuantity = req.PlannedQuantity,
                ActualQuantity = req.ActualQuantity,
                CementUsed = req.CementUsed,
                SandUsed = req.SandUsed,
                AggregateUsed = req.AggregateUsed,
                Notes = req.Notes,
                RecordedById = userId
            };
            e.CompletionPercentage = req.NewPercentage;
            e.TrackingStatus = (TrackingStatus)req.NewStatus;
            e.UpdatedBy = userId.ToString();
            e.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            await _histories.AddAsync(history);
            await _elements.UpdateAsync(e);
            await _elements.SaveChangesAsync();
            await _histories.SaveChangesAsync();
            return MapHistory(history);
        }

        public async Task<List<TrackingHistoryDto>> GetHistoryAsync(Guid elementId)
        {
            var list = await _histories.GetByElementIdAsync(elementId);
            return list.Select(MapHistory).ToList();
        }

        public async Task<List<TrackingPhotoDto>> GetPhotosAsync(Guid historyId)
        {
            var list = await _photos.GetByHistoryIdAsync(historyId);
            return list.Select(p => new TrackingPhotoDto
            {
                Id = p.Id,
                TrackingHistoryId = p.TrackingHistoryId,
                PhotoUrl = p.PhotoUrl,
                Caption = p.Caption,
                FileSizeMB = p.FileSizeMB,
                FileType = p.FileType,
                Width = p.Width,
                Height = p.Height,
                UploadedAt = p.UploadedAt
            }).ToList();
        }

        public async Task<TrackingPhotoDto> AddPhotoAsync(Guid historyId, AddTrackingPhotoRequest req, Guid userId)
        {
            var photo = new TrackingPhoto
            {
                Id = Guid.NewGuid(),
                TrackingHistoryId = historyId,
                PhotoUrl = req.PhotoUrl,
                Caption = req.Caption,
                FileSizeMB = req.FileSizeMB,
                FileType = req.FileType,
                Width = req.Width,
                Height = req.Height,
                UploadedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };
            await _photos.AddAsync(photo);
            await _photos.SaveChangesAsync();
            return new TrackingPhotoDto
            {
                Id = photo.Id,
                TrackingHistoryId = photo.TrackingHistoryId,
                PhotoUrl = photo.PhotoUrl,
                Caption = photo.Caption,
                FileSizeMB = photo.FileSizeMB,
                FileType = photo.FileType,
                Width = photo.Width,
                Height = photo.Height,
                UploadedAt = photo.UploadedAt
            };
        }

        public async Task<TrackingPhotoDto> AddPhotoAsync(Guid historyId, IFormFile file, string? caption, Guid userId)
        {
            // Upload actual file and create photo record
            var url = await _fileStorage.UploadFileAsync(file, "tracking-photos");

            var photo = new TrackingPhoto
            {
                Id = Guid.NewGuid(),
                TrackingHistoryId = historyId,
                PhotoUrl = url,
                Caption = caption,
                FileSizeMB = Math.Round((decimal)file.Length / (1024 * 1024), 2),
                FileType = file.ContentType,
                UploadedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };
            await _photos.AddAsync(photo);
            await _photos.SaveChangesAsync();
            return new TrackingPhotoDto
            {
                Id = photo.Id,
                TrackingHistoryId = photo.TrackingHistoryId,
                PhotoUrl = photo.PhotoUrl,
                Caption = photo.Caption,
                FileSizeMB = photo.FileSizeMB,
                FileType = photo.FileType,
                UploadedAt = photo.UploadedAt
            };
        }

        public async Task DeletePhotoAsync(Guid photoId)
        {
            await _photos.DeleteAsync(photoId);
            await _photos.SaveChangesAsync();
        }

        public async Task<ModelSummaryDto> GetModelSummaryAsync(Guid modelId)
        {
            var list = await _elements.GetByModelIdAsync(modelId);
            var summary = new ModelSummaryDto
            {
                TotalElements = list.Count,
                NotStarted = list.Count(x => x.TrackingStatus == TrackingStatus.NotStarted),
                InProgress = list.Count(x => x.TrackingStatus == TrackingStatus.InProgress),
                Completed = list.Count(x => x.TrackingStatus == TrackingStatus.Completed),
                OnHold = list.Count(x => x.TrackingStatus == TrackingStatus.OnHold),
                AverageCompletion = list.Count == 0 ? 0 : (decimal)list.Average(x => x.CompletionPercentage)
            };
            return summary;
        }

        private static BuildingElementDto MapElement(BuildingElement e) => new()
        {
            Id = e.Id,
            ModelId = e.ModelId,
            Name = e.Name,
            ElementType = (int)e.ElementType,
            FloorLevel = e.FloorLevel,
            MeshIndicesJson = e.MeshIndices ?? "[]",
            TrackingStatus = (int)e.TrackingStatus,
            CompletionPercentage = e.CompletionPercentage,
            Color = e.Color ?? "#CCCCCC",
            CreatedAt = e.CreatedAt
        };

        private static TrackingHistoryDto MapHistory(ElementTrackingHistory h) => new()
        {
            Id = h.Id,
            BuildingElementId = h.BuildingElementId,
            TrackingDate = h.TrackingDate,
            PreviousPercentage = h.PreviousPercentage,
            NewPercentage = h.NewPercentage,
            PreviousStatus = h.PreviousStatus.HasValue ? (int)h.PreviousStatus.Value : (int?)null,
            NewStatus = (int)h.NewStatus,
            Notes = h.Notes
        };

        private static TrackingPhotoDto MapPhoto(TrackingPhoto p) => new()
        {
            Id = p.Id,
            TrackingHistoryId = p.TrackingHistoryId,
            PhotoUrl = p.PhotoUrl,
            Caption = p.Caption,
            FileSizeMB = p.FileSizeMB,
            FileType = p.FileType,
            Width = p.Width,
            Height = p.Height,
            UploadedAt = p.UploadedAt
        };
    }
}
