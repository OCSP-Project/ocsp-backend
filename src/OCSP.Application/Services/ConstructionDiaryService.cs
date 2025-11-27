using Microsoft.EntityFrameworkCore;
using OCSP.Application.DTOs.ConstructionDiary;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;

namespace OCSP.Application.Services
{
    public class ConstructionDiaryService : IConstructionDiaryService
    {
        private readonly ApplicationDbContext _context;

        public ConstructionDiaryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ConstructionDiaryDetailDto?> GetDiaryByDateAsync(Guid projectId, DateTime date, CancellationToken ct = default)
        {
            // Ensure date is UTC for PostgreSQL
            var searchDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

            var diary = await _context.ConstructionDiaries
                .Include(d => d.WorkItems)
                    .ThenInclude(w => w.LaborEntries)
                .Include(d => d.WorkItems)
                    .ThenInclude(w => w.EquipmentEntries)
                .Include(d => d.WeatherPeriods)
                .Include(d => d.Images)
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d => d.ProjectId == projectId && d.DiaryDate.Date == searchDate, ct);

            if (diary == null) return null;

            return MapToDetailDto(diary);
        }

        public async Task<List<ConstructionDiarySummaryDto>> GetDiariesByMonthAsync(Guid projectId, int year, int month, CancellationToken ct = default)
        {
            // Ensure dates are UTC for PostgreSQL
            var startDate = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(startDate.AddMonths(1).AddDays(-1), DateTimeKind.Utc);

            var diaries = await _context.ConstructionDiaries
                .Where(d => d.ProjectId == projectId &&
                           d.DiaryDate >= startDate &&
                           d.DiaryDate <= endDate)
                .Include(d => d.WorkItems)
                .Include(d => d.Images)
                .OrderBy(d => d.DiaryDate)
                .ToListAsync(ct);

            return diaries.Select(d => MapToSummaryDto(d)).ToList();
        }

        public async Task<List<ConstructionDiarySummaryDto>> GetAllDiariesByProjectAsync(Guid projectId, CancellationToken ct = default)
        {
            var diaries = await _context.ConstructionDiaries
                .Where(d => d.ProjectId == projectId)
                .Include(d => d.WorkItems)
                .Include(d => d.Images)
                .OrderByDescending(d => d.DiaryDate)
                .ToListAsync(ct);

            return diaries.Select(d => MapToSummaryDto(d)).ToList();
        }

        public async Task<ConstructionDiaryDetailDto> CreateDiaryAsync(CreateConstructionDiaryDto dto, Guid userId, CancellationToken ct = default)
        {
            // Ensure date is UTC for PostgreSQL
            var searchDate = DateTime.SpecifyKind(dto.DiaryDate.Date, DateTimeKind.Utc);

            // Check if diary already exists for this date
            var existing = await _context.ConstructionDiaries
                .FirstOrDefaultAsync(d => d.ProjectId == dto.ProjectId && d.DiaryDate.Date == searchDate, ct);

            if (existing != null)
                throw new InvalidOperationException($"Diary already exists for date {dto.DiaryDate:yyyy-MM-dd}");

            var diary = new ConstructionDiary
            {
                Id = Guid.NewGuid(),
                ProjectId = dto.ProjectId,
                DiaryDate = searchDate,
                ConstructionTeam = dto.ConstructionTeam,
                SafetyRating = (ConstructionRating)dto.SafetyRating,
                QualityRating = (ConstructionRating)dto.QualityRating,
                ProgressRating = (ConstructionRating)dto.ProgressRating,
                CleanlinessRating = (ConstructionRating)dto.CleanlinessRating,
                IncidentReport = dto.IncidentReport,
                Recommendations = dto.Recommendations,
                Notes = dto.Notes,
                SupervisorName = dto.SupervisorName,
                SupervisorPosition = dto.SupervisorPosition,
                ContractorName = dto.ContractorName,
                SupervisorUnitName = dto.SupervisorUnitName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString(),
                UpdatedBy = userId.ToString()
            };

            // Add work items
            foreach (var workItemDto in dto.WorkItems)
            {
                var workItem = await _context.WorkItems.FindAsync(new object[] { workItemDto.WorkItemId }, ct);
                if (workItem == null) continue;

                var diaryWorkItem = new DiaryWorkItem
                {
                    Id = Guid.NewGuid(),
                    ConstructionDiaryId = diary.Id,
                    WorkItemId = workItemDto.WorkItemId,
                    WorkItemName = workItem.Name,
                    ConstructionArea = workItemDto.ConstructionArea,
                    PlannedQuantity = workItem.PlannedQuantity ?? 0,
                    ConstructedQuantity = workItemDto.ConstructedQuantity,
                    RemainingQuantity = (workItem.PlannedQuantity ?? 0) - workItemDto.ConstructedQuantity,
                    Unit = workItem.Unit ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add labor entries
                foreach (var laborDto in workItemDto.LaborEntries)
                {
                    diaryWorkItem.LaborEntries.Add(new DiaryLabor
                    {
                        Id = Guid.NewGuid(),
                        DiaryWorkItemId = diaryWorkItem.Id,
                        LaborId = laborDto.LaborId,
                        LaborName = laborDto.LaborName,
                        Position = laborDto.Position,
                        WorkHours = laborDto.WorkHours,
                        Team = laborDto.Team,
                        Shift = laborDto.Shift,
                        Quantity = laborDto.Quantity,
                        Unit = laborDto.Unit,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                // Add equipment entries
                foreach (var equipDto in workItemDto.EquipmentEntries)
                {
                    diaryWorkItem.EquipmentEntries.Add(new DiaryEquipment
                    {
                        Id = Guid.NewGuid(),
                        DiaryWorkItemId = diaryWorkItem.Id,
                        EquipmentId = equipDto.EquipmentId,
                        EquipmentName = equipDto.EquipmentName,
                        Specifications = equipDto.Specifications,
                        HoursUsed = equipDto.HoursUsed,
                        Quantity = equipDto.Quantity,
                        Unit = equipDto.Unit,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                diary.WorkItems.Add(diaryWorkItem);
            }

            // Add weather periods
            foreach (var weatherDto in dto.WeatherPeriods)
            {
                diary.WeatherPeriods.Add(new DiaryWeatherPeriod
                {
                    Id = Guid.NewGuid(),
                    ConstructionDiaryId = diary.Id,
                    Period = weatherDto.Period,
                    Condition = weatherDto.Condition,
                    Temperature = weatherDto.Temperature,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // Add images
            foreach (var imageDto in dto.Images)
            {
                diary.Images.Add(new DiaryImage
                {
                    Id = Guid.NewGuid(),
                    ConstructionDiaryId = diary.Id,
                    Url = imageDto.Url,
                    Category = (ImageCategory)imageDto.Category,
                    Description = imageDto.Description,
                    UploadedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            _context.ConstructionDiaries.Add(diary);
            await _context.SaveChangesAsync(ct);

            return await GetDiaryByDateAsync(dto.ProjectId, dto.DiaryDate, ct)
                ?? throw new InvalidOperationException("Failed to retrieve created diary");
        }

        public async Task<ConstructionDiaryDetailDto> UpdateDiaryAsync(Guid diaryId, UpdateConstructionDiaryDto dto, Guid userId, CancellationToken ct = default)
        {
            var diary = await _context.ConstructionDiaries
                .Include(d => d.WorkItems)
                    .ThenInclude(w => w.LaborEntries)
                .Include(d => d.WorkItems)
                    .ThenInclude(w => w.EquipmentEntries)
                .Include(d => d.WeatherPeriods)
                .Include(d => d.Images)
                .FirstOrDefaultAsync(d => d.Id == diaryId, ct);

            if (diary == null)
                throw new ArgumentException("Diary not found");

            // Update basic info
            diary.ConstructionTeam = dto.ConstructionTeam;
            diary.SafetyRating = (ConstructionRating)dto.SafetyRating;
            diary.QualityRating = (ConstructionRating)dto.QualityRating;
            diary.ProgressRating = (ConstructionRating)dto.ProgressRating;
            diary.CleanlinessRating = (ConstructionRating)dto.CleanlinessRating;
            diary.IncidentReport = dto.IncidentReport;
            diary.Recommendations = dto.Recommendations;
            diary.Notes = dto.Notes;
            diary.SupervisorName = dto.SupervisorName;
            diary.SupervisorPosition = dto.SupervisorPosition;
            diary.ContractorName = dto.ContractorName;
            diary.SupervisorUnitName = dto.SupervisorUnitName;
            diary.UpdatedAt = DateTime.UtcNow;
            diary.UpdatedBy = userId.ToString();

            // Detach all old tracked entities BEFORE deleting to prevent EF from trying to update them
            Console.WriteLine($"[UPDATE DIARY] Starting detach process. Diary has {diary.WorkItems.Count} work items, {diary.WeatherPeriods.Count} weather periods, {diary.Images.Count} images");

            foreach (var workItem in diary.WorkItems.ToList())
            {
                Console.WriteLine($"[UPDATE DIARY] Detaching WorkItem {workItem.Id}, current state: {_context.Entry(workItem).State}");
                foreach (var labor in workItem.LaborEntries.ToList())
                {
                    Console.WriteLine($"[UPDATE DIARY] Detaching Labor {labor.Id}, state before: {_context.Entry(labor).State}");
                    _context.Entry(labor).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    Console.WriteLine($"[UPDATE DIARY] Labor {labor.Id}, state after: {_context.Entry(labor).State}");
                }
                foreach (var equip in workItem.EquipmentEntries.ToList())
                {
                    Console.WriteLine($"[UPDATE DIARY] Detaching Equipment {equip.Id}, state before: {_context.Entry(equip).State}");
                    _context.Entry(equip).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    Console.WriteLine($"[UPDATE DIARY] Equipment {equip.Id}, state after: {_context.Entry(equip).State}");
                }
                _context.Entry(workItem).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                Console.WriteLine($"[UPDATE DIARY] WorkItem {workItem.Id}, state after: {_context.Entry(workItem).State}");
            }

            foreach (var weather in diary.WeatherPeriods.ToList())
            {
                Console.WriteLine($"[UPDATE DIARY] Detaching Weather {weather.Id}, state before: {_context.Entry(weather).State}");
                _context.Entry(weather).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                Console.WriteLine($"[UPDATE DIARY] Weather {weather.Id}, state after: {_context.Entry(weather).State}");
            }

            foreach (var image in diary.Images.ToList())
            {
                Console.WriteLine($"[UPDATE DIARY] Detaching Image {image.Id}, state before: {_context.Entry(image).State}");
                _context.Entry(image).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                Console.WriteLine($"[UPDATE DIARY] Image {image.Id}, state after: {_context.Entry(image).State}");
            }

            Console.WriteLine($"[UPDATE DIARY] Clearing collections...");
            diary.WorkItems.Clear();
            diary.WeatherPeriods.Clear();
            diary.Images.Clear();
            Console.WriteLine($"[UPDATE DIARY] Collections cleared. Diary now has {diary.WorkItems.Count} work items, {diary.WeatherPeriods.Count} weather, {diary.Images.Count} images");

            // Remove existing nested entities - use direct SQL
            await _context.DiaryLabors
                .Where(l => _context.DiaryWorkItems
                    .Where(w => w.ConstructionDiaryId == diaryId)
                    .Select(w => w.Id)
                    .Contains(l.DiaryWorkItemId))
                .ExecuteDeleteAsync(ct);
            await _context.DiaryEquipments
                .Where(e => _context.DiaryWorkItems
                    .Where(w => w.ConstructionDiaryId == diaryId)
                    .Select(w => w.Id)
                    .Contains(e.DiaryWorkItemId))
                .ExecuteDeleteAsync(ct);
            await _context.DiaryWorkItems
                .Where(w => w.ConstructionDiaryId == diaryId)
                .ExecuteDeleteAsync(ct);
            await _context.DiaryWeatherPeriods
                .Where(w => w.ConstructionDiaryId == diaryId)
                .ExecuteDeleteAsync(ct);
            await _context.DiaryImages
                .Where(i => i.ConstructionDiaryId == diaryId)
                .ExecuteDeleteAsync(ct);

            // Re-add work items (same as create)
            foreach (var workItemDto in dto.WorkItems)
            {
                var workItem = await _context.WorkItems.FindAsync(new object[] { workItemDto.WorkItemId }, ct);
                if (workItem == null) continue;

                var diaryWorkItem = new DiaryWorkItem
                {
                    Id = Guid.NewGuid(),
                    ConstructionDiaryId = diary.Id,
                    WorkItemId = workItemDto.WorkItemId,
                    WorkItemName = workItem.Name,
                    ConstructionArea = workItemDto.ConstructionArea,
                    PlannedQuantity = workItem.PlannedQuantity ?? 0,
                    ConstructedQuantity = workItemDto.ConstructedQuantity,
                    RemainingQuantity = (workItem.PlannedQuantity ?? 0) - workItemDto.ConstructedQuantity,
                    Unit = workItem.Unit ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                foreach (var laborDto in workItemDto.LaborEntries)
                {
                    diaryWorkItem.LaborEntries.Add(new DiaryLabor
                    {
                        Id = Guid.NewGuid(),
                        DiaryWorkItemId = diaryWorkItem.Id,
                        LaborId = laborDto.LaborId,
                        LaborName = laborDto.LaborName,
                        Position = laborDto.Position,
                        WorkHours = laborDto.WorkHours,
                        Team = laborDto.Team,
                        Shift = laborDto.Shift,
                        Quantity = laborDto.Quantity,
                        Unit = laborDto.Unit,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                foreach (var equipDto in workItemDto.EquipmentEntries)
                {
                    diaryWorkItem.EquipmentEntries.Add(new DiaryEquipment
                    {
                        Id = Guid.NewGuid(),
                        DiaryWorkItemId = diaryWorkItem.Id,
                        EquipmentId = equipDto.EquipmentId,
                        EquipmentName = equipDto.EquipmentName,
                        Specifications = equipDto.Specifications,
                        HoursUsed = equipDto.HoursUsed,
                        Quantity = equipDto.Quantity,
                        Unit = equipDto.Unit,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                diary.WorkItems.Add(diaryWorkItem);

                // Explicitly set state to Added for work item and nested entities
                _context.Entry(diaryWorkItem).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                foreach (var labor in diaryWorkItem.LaborEntries)
                {
                    _context.Entry(labor).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
                foreach (var equip in diaryWorkItem.EquipmentEntries)
                {
                    _context.Entry(equip).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
            }

            foreach (var weatherDto in dto.WeatherPeriods)
            {
                var newWeather = new DiaryWeatherPeriod
                {
                    Id = Guid.NewGuid(),
                    ConstructionDiaryId = diary.Id,
                    Period = weatherDto.Period,
                    Condition = weatherDto.Condition,
                    Temperature = weatherDto.Temperature,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                diary.WeatherPeriods.Add(newWeather);

                // Explicitly set state to Added
                _context.Entry(newWeather).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            }

            Console.WriteLine($"[UPDATE DIARY] About to add {dto.Images.Count} images from DTO");
            foreach (var imageDto in dto.Images)
            {
                var newImageId = Guid.NewGuid();
                Console.WriteLine($"[UPDATE DIARY] Creating image with NEW ID: {newImageId}, Category: {imageDto.Category}, URL length: {imageDto.Url?.Length ?? 0}");

                var newImage = new DiaryImage
                {
                    Id = newImageId,
                    ConstructionDiaryId = diary.Id,
                    Url = imageDto.Url,
                    Category = (ImageCategory)imageDto.Category,
                    Description = imageDto.Description,
                    UploadedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                diary.Images.Add(newImage);

                // Explicitly set state to Added to ensure INSERT instead of UPDATE
                _context.Entry(newImage).State = Microsoft.EntityFrameworkCore.EntityState.Added;

                var imageEntry = _context.Entry(newImage);
                Console.WriteLine($"[UPDATE DIARY] Image {newImageId} added to collection, EF State after explicit set: {imageEntry.State}");
            }

            Console.WriteLine($"[UPDATE DIARY] Before SaveChanges - checking tracked entities:");
            var trackedEntities = _context.ChangeTracker.Entries().ToList();
            Console.WriteLine($"[UPDATE DIARY] Total tracked entities: {trackedEntities.Count}");
            foreach (var entry in trackedEntities)
            {
                Console.WriteLine($"[UPDATE DIARY] Tracked: {entry.Entity.GetType().Name} with ID {GetEntityId(entry.Entity)}, State: {entry.State}");
            }

            await _context.SaveChangesAsync(ct);

            Console.WriteLine($"[UPDATE DIARY] SaveChanges completed successfully!");

            return await GetDiaryByDateAsync(diary.ProjectId, diary.DiaryDate, ct)
                ?? throw new InvalidOperationException("Failed to retrieve updated diary");
        }

        private object GetEntityId(object entity)
        {
            var idProp = entity.GetType().GetProperty("Id");
            return idProp?.GetValue(entity) ?? "N/A";
        }

        public async Task DeleteDiaryAsync(Guid diaryId, Guid userId, CancellationToken ct = default)
        {
            var diary = await _context.ConstructionDiaries.FindAsync(new object[] { diaryId }, ct);

            if (diary == null)
                throw new ArgumentException("Diary not found");

            _context.ConstructionDiaries.Remove(diary);
            await _context.SaveChangesAsync(ct);
        }

        #region Mapping Methods

        private ConstructionDiaryDetailDto MapToDetailDto(ConstructionDiary diary)
        {
            return new ConstructionDiaryDetailDto
            {
                Id = diary.Id,
                ProjectId = diary.ProjectId,
                ProjectName = diary.Project?.Name,
                DiaryDate = diary.DiaryDate,
                ConstructionTeam = diary.ConstructionTeam,
                SafetyRating = (ConstructionRatingDto)diary.SafetyRating,
                QualityRating = (ConstructionRatingDto)diary.QualityRating,
                ProgressRating = (ConstructionRatingDto)diary.ProgressRating,
                CleanlinessRating = (ConstructionRatingDto)diary.CleanlinessRating,
                IncidentReport = diary.IncidentReport,
                Recommendations = diary.Recommendations,
                Notes = diary.Notes,
                SupervisorName = diary.SupervisorName,
                SupervisorPosition = diary.SupervisorPosition,
                ContractorName = diary.ContractorName,
                SupervisorUnitName = diary.SupervisorUnitName,
                CreatedAt = diary.CreatedAt,
                UpdatedAt = diary.UpdatedAt,
                CreatedBy = diary.CreatedBy,
                UpdatedBy = diary.UpdatedBy,
                WorkItems = diary.WorkItems.Select(w => MapToWorkItemDto(w)).ToList(),
                WeatherPeriods = diary.WeatherPeriods.Select(wp => MapToWeatherDto(wp)).ToList(),
                Images = diary.Images.Select(i => MapToImageDto(i)).ToList()
            };
        }

        private ConstructionDiarySummaryDto MapToSummaryDto(ConstructionDiary diary)
        {
            return new ConstructionDiarySummaryDto
            {
                Id = diary.Id,
                ProjectId = diary.ProjectId,
                DiaryDate = diary.DiaryDate,
                ConstructionTeam = diary.ConstructionTeam,
                WorkItemCount = diary.WorkItems.Count,
                ImageCount = diary.Images.Count,
                CreatedAt = diary.CreatedAt,
                CreatedBy = diary.CreatedBy
            };
        }

        private DiaryWorkItemDto MapToWorkItemDto(DiaryWorkItem workItem)
        {
            return new DiaryWorkItemDto
            {
                Id = workItem.Id,
                ConstructionDiaryId = workItem.ConstructionDiaryId,
                WorkItemId = workItem.WorkItemId,
                WorkItemName = workItem.WorkItemName,
                ConstructionArea = workItem.ConstructionArea,
                PlannedQuantity = workItem.PlannedQuantity,
                ConstructedQuantity = workItem.ConstructedQuantity,
                RemainingQuantity = workItem.RemainingQuantity,
                Unit = workItem.Unit,
                LaborEntries = workItem.LaborEntries.Select(l => MapToLaborDto(l)).ToList(),
                EquipmentEntries = workItem.EquipmentEntries.Select(e => MapToEquipmentDto(e)).ToList()
            };
        }

        private DiaryLaborDto MapToLaborDto(DiaryLabor labor)
        {
            return new DiaryLaborDto
            {
                Id = labor.Id,
                DiaryWorkItemId = labor.DiaryWorkItemId,
                LaborId = labor.LaborId,
                LaborName = labor.LaborName,
                Position = labor.Position,
                WorkHours = labor.WorkHours,
                Team = labor.Team,
                Shift = labor.Shift,
                Quantity = labor.Quantity,
                Unit = labor.Unit
            };
        }

        private DiaryEquipmentDto MapToEquipmentDto(DiaryEquipment equipment)
        {
            return new DiaryEquipmentDto
            {
                Id = equipment.Id,
                DiaryWorkItemId = equipment.DiaryWorkItemId,
                EquipmentId = equipment.EquipmentId,
                EquipmentName = equipment.EquipmentName,
                Specifications = equipment.Specifications,
                HoursUsed = equipment.HoursUsed,
                Quantity = equipment.Quantity,
                Unit = equipment.Unit
            };
        }

        private DiaryWeatherPeriodDto MapToWeatherDto(DiaryWeatherPeriod weather)
        {
            return new DiaryWeatherPeriodDto
            {
                Id = weather.Id,
                ConstructionDiaryId = weather.ConstructionDiaryId,
                Period = weather.Period,
                Condition = weather.Condition,
                Temperature = weather.Temperature
            };
        }

        private DiaryImageDto MapToImageDto(DiaryImage image)
        {
            return new DiaryImageDto
            {
                Id = image.Id,
                ConstructionDiaryId = image.ConstructionDiaryId,
                Url = image.Url,
                Category = (ImageCategoryDto)image.Category,
                Description = image.Description,
                UploadedAt = image.UploadedAt
            };
        }

        #endregion
    }
}
