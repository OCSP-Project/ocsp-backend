using OCSP.Application.DTOs.Budget;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace OCSP.Application.Services
{
    public class WorkItemService : IWorkItemService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public WorkItemService(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<List<WorkItemDto>> GetAllByProjectAsync(Guid projectId, bool rootLevelOnly = false, bool includeChildren = true, CancellationToken ct = default)
        {
            var query = _context.Set<WorkItem>()
                .Where(w => w.ProjectId == projectId && !w.IsDeleted);

            // When including children, always start from root to build proper hierarchy
            if (rootLevelOnly || includeChildren)
            {
                query = query.Where(w => w.ParentId == null);
            }

            var workItems = await query
                .OrderBy(w => w.SortOrder)
                .ToListAsync(ct);

            var result = new List<WorkItemDto>();

            foreach (var item in workItems)
            {
                var dto = await MapToDto(item, includeChildren, ct);
                result.Add(dto);
            }

            return result;
        }

        public async Task<WorkItemDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var workItem = await _context.Set<WorkItem>()
                .Include(w => w.Children)
                .Include(w => w.Materials)
                .Include(w => w.Documents)
                .Include(w => w.Comments)
                .Include(w => w.Activities)
                .Include(w => w.UpdateHistory)
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, ct);

            if (workItem == null) return null;

            var dto = new WorkItemDetailDto
            {
                Id = workItem.Id,
                ProjectId = workItem.ProjectId,
                Code = workItem.Code,
                OrderNumber = workItem.OrderNumber,
                Name = workItem.Name,
                Description = workItem.Description,
                ParentId = workItem.ParentId,
                Level = workItem.Level,
                SortOrder = workItem.SortOrder,
                Type = workItem.Type.ToString(),
                HasChildren = workItem.Children.Any(),
                PlannedStartDate = workItem.PlannedStartDate,
                PlannedEndDate = workItem.PlannedEndDate,
                ActualStartDate = workItem.ActualStartDate,
                ActualEndDate = workItem.ActualEndDate,
                PlannedDuration = workItem.PlannedDuration,
                ActualDuration = workItem.ActualDuration,
                Progress = workItem.Progress,
                Status = workItem.Status.ToString(),
                StatusLabel = GetStatusLabel(workItem.Status),
                StatusColor = GetStatusColor(workItem.Status),
                PlannedQuantity = workItem.PlannedQuantity,
                Unit = workItem.Unit,
                ActualQuantity = workItem.ActualQuantity,
                UnitPrice = workItem.UnitPrice,
                TotalAmount = workItem.TotalAmount,
                CompactionFactor = workItem.CompactionFactor,
                Assignees = ParseAssignees(workItem.AssigneeIds),
                ResponsiblePerson = await GetResponsiblePerson(workItem.ResponsiblePersonId, ct),
                IsExpanded = workItem.IsExpanded,
                IsHidden = workItem.IsHidden,
                Notes = workItem.Notes,
                CreatedAt = workItem.CreatedAt,
                UpdatedAt = workItem.UpdatedAt,

                // Include related data
                Children = workItem.Children.Select(c => MapToSimpleDto(c)).ToList(),
                Materials = workItem.Materials.Select(m => new WorkItemMaterialDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Code = m.Code,
                    Unit = m.Unit,
                    PlannedQuantity = m.PlannedQuantity,
                    ActualQuantity = m.ActualQuantity,
                    UnitPrice = m.UnitPrice,
                    TotalAmount = m.TotalAmount,
                    Supplier = m.Supplier
                }).ToList(),
                Documents = workItem.Documents.Select(d => new WorkItemDocumentDto
                {
                    Id = d.Id,
                    DocumentType = d.DocumentType.ToString(),
                    FileName = d.FileName,
                    FilePath = d.FilePath,
                    Description = d.Description,
                    FileSize = d.FileSize,
                    FileSizeFormatted = FormatFileSize(d.FileSize),
                    UploadedAt = d.UploadedAt,
                    UploadedByUsername = "User" // TODO: Get from User relation
                }).ToList(),
                Comments = BuildCommentTree(workItem.Comments.Where(c => !c.IsDeleted).ToList()),
                Activities = workItem.Activities.Select(a => new WorkItemActivityDto
                {
                    Id = a.Id,
                    Type = a.Type.ToString(),
                    Description = a.Description,
                    PerformedByUsername = "User", // TODO: Get from User relation
                    Timestamp = a.Timestamp,
                    Notes = a.Notes
                }).OrderByDescending(a => a.Timestamp).ToList(),
                UpdateHistory = workItem.UpdateHistory.Select(h => new WorkItemUpdateHistoryDto
                {
                    Id = h.Id,
                    FieldName = h.FieldName,
                    OldValue = h.OldValue,
                    NewValue = h.NewValue,
                    UpdatedByUsername = "User", // TODO: Get from User relation
                    UpdatedAt = h.UpdatedAt,
                    Reason = h.Reason
                }).OrderByDescending(h => h.UpdatedAt).ToList()
            };

            return dto;
        }

        public async Task<List<WorkItemDto>> GetChildrenAsync(Guid parentId, CancellationToken ct = default)
        {
            var children = await _context.Set<WorkItem>()
                .Where(w => w.ParentId == parentId && !w.IsDeleted)
                .OrderBy(w => w.SortOrder)
                .ToListAsync(ct);

            var result = new List<WorkItemDto>();
            foreach (var child in children)
            {
                result.Add(await MapToDto(child, false, ct));
            }

            return result;
        }

        public async Task<WorkItemDto> CreateAsync(CreateWorkItemDto dto, Guid currentUserId, CancellationToken ct = default)
        {
            // Validate project exists
            var projectExists = await _context.Set<Project>().AnyAsync(p => p.Id == dto.ProjectId, ct);
            if (!projectExists)
                throw new ArgumentException("Project not found");

            // Validate parent if specified
            if (dto.ParentId.HasValue)
            {
                var parentExists = await _context.Set<WorkItem>().AnyAsync(w => w.Id == dto.ParentId.Value && !w.IsDeleted, ct);
                if (!parentExists)
                    throw new ArgumentException("Parent work item not found");
            }

            // Calculate level
            int level = 1;
            if (dto.ParentId.HasValue)
            {
                var parent = await _context.Set<WorkItem>().FindAsync(dto.ParentId.Value);
                level = parent!.Level + 1;
            }

            // Calculate end date
            DateTime? plannedEndDate = null;
            if (dto.PlannedStartDate.HasValue && dto.PlannedDuration > 0)
            {
                plannedEndDate = dto.PlannedStartDate.Value.AddDays(dto.PlannedDuration);
            }

            // Calculate total amount
            decimal? totalAmount = null;
            if (dto.PlannedQuantity.HasValue && dto.UnitPrice.HasValue)
            {
                totalAmount = dto.PlannedQuantity.Value * dto.UnitPrice.Value;
            }

            var workItem = new WorkItem
            {
                Id = Guid.NewGuid(),
                ProjectId = dto.ProjectId,
                ParentId = dto.ParentId,
                Code = dto.Code,
                OrderNumber = dto.OrderNumber,
                Name = dto.Name,
                Description = dto.Description,
                Level = level,
                SortOrder = await GetNextSortOrder(dto.ProjectId, dto.ParentId, ct),
                Type = WorkItemType.Task,
                PlannedStartDate = dto.PlannedStartDate,
                PlannedEndDate = plannedEndDate,
                PlannedDuration = dto.PlannedDuration,
                PlannedQuantity = dto.PlannedQuantity,
                Unit = dto.Unit,
                UnitPrice = dto.UnitPrice,
                TotalAmount = totalAmount,
                CompactionFactor = dto.CompactionFactor,
                AssigneeIds = dto.AssigneeIds != null ? JsonSerializer.Serialize(dto.AssigneeIds) : null,
                ResponsiblePersonId = dto.ResponsiblePersonId?.ToString(),
                PrerequisiteIds = dto.PrerequisiteIds != null ? JsonSerializer.Serialize(dto.PrerequisiteIds) : null,
                Notes = dto.Notes,
                Status = WorkItemStatus.NotStarted,
                Progress = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId.ToString(),
                UpdatedBy = currentUserId.ToString()
            };

            _context.Set<WorkItem>().Add(workItem);

            // Log activity
            var activity = new WorkItemActivity
            {
                Id = Guid.NewGuid(),
                WorkItemId = workItem.Id,
                Type = ActivityType.Created,
                Description = $"Work item '{workItem.Name}' created",
                PerformedById = currentUserId,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.Set<WorkItemActivity>().Add(activity);

            await _context.SaveChangesAsync(ct);

            return await MapToDto(workItem, false, ct);
        }

        public async Task<WorkItemDto> UpdateAsync(Guid id, UpdateWorkItemDto dto, Guid currentUserId, CancellationToken ct = default)
        {
            var workItem = await _context.Set<WorkItem>()
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, ct);

            if (workItem == null)
                throw new ArgumentException("Work item not found");

            // Track changes for history
            var changes = new List<(string Field, string? OldValue, string? NewValue)>();

            if (dto.Name != null && dto.Name != workItem.Name)
            {
                changes.Add(("Name", workItem.Name, dto.Name));
                workItem.Name = dto.Name;
            }

            if (dto.Description != null)
            {
                changes.Add(("Description", workItem.Description, dto.Description));
                workItem.Description = dto.Description;
            }

            if (dto.PlannedStartDate.HasValue)
            {
                changes.Add(("PlannedStartDate", workItem.PlannedStartDate?.ToString(), dto.PlannedStartDate?.ToString()));
                workItem.PlannedStartDate = dto.PlannedStartDate;
            }

            if (dto.PlannedEndDate.HasValue)
            {
                workItem.PlannedEndDate = dto.PlannedEndDate;
            }

            if (dto.PlannedDuration.HasValue)
            {
                workItem.PlannedDuration = dto.PlannedDuration.Value;
                if (workItem.PlannedStartDate.HasValue)
                {
                    workItem.PlannedEndDate = workItem.PlannedStartDate.Value.AddDays(dto.PlannedDuration.Value);
                }
            }

            if (dto.Progress.HasValue)
            {
                changes.Add(("Progress", workItem.Progress.ToString(), dto.Progress?.ToString()));
                workItem.Progress = dto.Progress.Value;
            }

            if (dto.Status != null)
            {
                changes.Add(("Status", workItem.Status.ToString(), dto.Status));
                workItem.Status = Enum.Parse<WorkItemStatus>(dto.Status);
            }

            if (dto.ActualQuantity.HasValue)
            {
                workItem.ActualQuantity = dto.ActualQuantity;
            }

            if (dto.ActualStartDate.HasValue)
            {
                workItem.ActualStartDate = dto.ActualStartDate;
            }

            if (dto.ActualEndDate.HasValue)
            {
                workItem.ActualEndDate = dto.ActualEndDate;
                if (workItem.ActualStartDate.HasValue)
                {
                    workItem.ActualDuration = (int)(dto.ActualEndDate.Value - workItem.ActualStartDate.Value).TotalDays;
                }
            }

            if (dto.AssigneeIds != null)
            {
                workItem.AssigneeIds = JsonSerializer.Serialize(dto.AssigneeIds);
            }

            if (dto.ResponsiblePersonId.HasValue)
            {
                workItem.ResponsiblePersonId = dto.ResponsiblePersonId.Value.ToString();
            }

            if (dto.Notes != null)
            {
                workItem.Notes = dto.Notes;
            }

            workItem.UpdatedAt = DateTime.UtcNow;
            workItem.UpdatedBy = currentUserId.ToString();

            // Save history
            foreach (var change in changes)
            {
                var history = new WorkItemUpdateHistory
                {
                    Id = Guid.NewGuid(),
                    WorkItemId = workItem.Id,
                    FieldName = change.Field,
                    OldValue = change.OldValue,
                    NewValue = change.NewValue,
                    UpdatedById = currentUserId,
                    UpdatedAt = DateTime.UtcNow,
                    Reason = dto.Reason,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Set<WorkItemUpdateHistory>().Add(history);
            }

            // Log activity
            var activity = new WorkItemActivity
            {
                Id = Guid.NewGuid(),
                WorkItemId = workItem.Id,
                Type = ActivityType.InfoChanged,
                Description = $"Work item updated: {string.Join(", ", changes.Select(c => c.Field))}",
                PerformedById = currentUserId,
                Timestamp = DateTime.UtcNow,
                Notes = dto.Reason,
                CreatedAt = DateTime.UtcNow
            };
            _context.Set<WorkItemActivity>().Add(activity);

            await _context.SaveChangesAsync(ct);

            return await MapToDto(workItem, false, ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var workItem = await _context.Set<WorkItem>()
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, ct);

            if (workItem == null)
                throw new ArgumentException("Work item not found");

            // Recursively delete all children first
            await DeleteChildrenRecursiveAsync(id, ct);

            // Soft delete the item itself
            workItem.IsDeleted = true;
            workItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
        }

        private async Task DeleteChildrenRecursiveAsync(Guid parentId, CancellationToken ct)
        {
            var children = await _context.Set<WorkItem>()
                .Where(w => w.ParentId == parentId && !w.IsDeleted)
                .ToListAsync(ct);

            foreach (var child in children)
            {
                // Recursively delete grandchildren
                await DeleteChildrenRecursiveAsync(child.Id, ct);

                // Soft delete the child
                child.IsDeleted = true;
                child.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task HardDeleteAllByProjectAsync(Guid projectId, CancellationToken ct = default)
        {
            // Get all work items for the project (including soft deleted)
            var workItems = await _context.Set<WorkItem>()
                .Where(w => w.ProjectId == projectId)
                .ToListAsync(ct);

            if (workItems.Count == 0)
                return;

            // Delete related data first (cascade delete might not work for all)
            var workItemIds = workItems.Select(w => w.Id).ToList();

            // Delete activities
            var activities = await _context.Set<WorkItemActivity>()
                .Where(a => workItemIds.Contains(a.WorkItemId))
                .ToListAsync(ct);
            _context.Set<WorkItemActivity>().RemoveRange(activities);

            // Delete comments
            var comments = await _context.Set<WorkItemComment>()
                .Where(c => workItemIds.Contains(c.WorkItemId))
                .ToListAsync(ct);
            _context.Set<WorkItemComment>().RemoveRange(comments);

            // Delete documents
            var documents = await _context.Set<WorkItemDocument>()
                .Where(d => workItemIds.Contains(d.WorkItemId))
                .ToListAsync(ct);
            _context.Set<WorkItemDocument>().RemoveRange(documents);

            // Delete materials
            var materials = await _context.Set<WorkItemMaterial>()
                .Where(m => workItemIds.Contains(m.WorkItemId))
                .ToListAsync(ct);
            _context.Set<WorkItemMaterial>().RemoveRange(materials);

            // Delete update history
            var updateHistories = await _context.Set<WorkItemUpdateHistory>()
                .Where(h => workItemIds.Contains(h.WorkItemId))
                .ToListAsync(ct);
            _context.Set<WorkItemUpdateHistory>().RemoveRange(updateHistories);

            // Finally, delete all work items
            _context.Set<WorkItem>().RemoveRange(workItems);

            await _context.SaveChangesAsync(ct);
        }

        public async Task<WorkItemDto> UpdateProgressAsync(Guid id, UpdateProgressDto dto, Guid currentUserId, CancellationToken ct = default)
        {
            var workItem = await _context.Set<WorkItem>()
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, ct);

            if (workItem == null)
                throw new ArgumentException("Work item not found");

            var oldProgress = workItem.Progress;
            workItem.Progress = dto.Progress;

            if (dto.ActualQuantity.HasValue)
            {
                workItem.ActualQuantity = dto.ActualQuantity;
            }

            // Auto-update status based on progress
            if (dto.Progress >= 100)
            {
                workItem.Status = WorkItemStatus.Completed;
                workItem.ActualEndDate = DateTime.UtcNow;
            }
            else if (dto.Progress > 0 && workItem.Status == WorkItemStatus.NotStarted)
            {
                workItem.Status = WorkItemStatus.InProgress;
                workItem.ActualStartDate = DateTime.UtcNow;
            }

            workItem.UpdatedAt = DateTime.UtcNow;
            workItem.UpdatedBy = currentUserId.ToString();

            // Log activity
            var activity = new WorkItemActivity
            {
                Id = Guid.NewGuid(),
                WorkItemId = workItem.Id,
                Type = ActivityType.ProgressUpdated,
                Description = $"Progress updated from {oldProgress}% to {dto.Progress}%",
                PerformedById = currentUserId,
                Timestamp = DateTime.UtcNow,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };
            _context.Set<WorkItemActivity>().Add(activity);

            await _context.SaveChangesAsync(ct);

            return await MapToDto(workItem, false, ct);
        }

        public async Task<WorkItemDto> UpdateStatusAsync(Guid id, string status, Guid currentUserId, CancellationToken ct = default)
        {
            var workItem = await _context.Set<WorkItem>()
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, ct);

            if (workItem == null)
                throw new ArgumentException("Work item not found");

            var oldStatus = workItem.Status;
            workItem.Status = Enum.Parse<WorkItemStatus>(status);
            workItem.UpdatedAt = DateTime.UtcNow;
            workItem.UpdatedBy = currentUserId.ToString();

            // Update dates based on status
            if (workItem.Status == WorkItemStatus.InProgress && !workItem.ActualStartDate.HasValue)
            {
                workItem.ActualStartDate = DateTime.UtcNow;
            }
            else if (workItem.Status == WorkItemStatus.Completed && !workItem.ActualEndDate.HasValue)
            {
                workItem.ActualEndDate = DateTime.UtcNow;
                workItem.Progress = 100;
            }

            // Log activity
            var activity = new WorkItemActivity
            {
                Id = Guid.NewGuid(),
                WorkItemId = workItem.Id,
                Type = ActivityType.StatusChanged,
                Description = $"Status changed from {oldStatus} to {status}",
                PerformedById = currentUserId,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.Set<WorkItemActivity>().Add(activity);

            await _context.SaveChangesAsync(ct);

            return await MapToDto(workItem, false, ct);
        }

        public async Task<WorkItemCommentDto> AddCommentAsync(Guid workItemId, AddCommentDto dto, Guid currentUserId, CancellationToken ct = default)
        {
            var workItemExists = await _context.Set<WorkItem>().AnyAsync(w => w.Id == workItemId && !w.IsDeleted, ct);
            if (!workItemExists)
                throw new ArgumentException("Work item not found");

            var comment = new WorkItemComment
            {
                Id = Guid.NewGuid(),
                WorkItemId = workItemId,
                Content = dto.Content,
                ParentCommentId = dto.ParentCommentId,
                CreatedById = currentUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Handle file attachments
            if (dto.Attachments != null && dto.Attachments.Any())
            {
                var attachmentUrls = new List<string>();
                foreach (var file in dto.Attachments)
                {
                    using var stream = file.OpenReadStream();
                    var url = await _fileService.UploadFileAsync(stream, file.FileName, "work-item-comments");
                    attachmentUrls.Add(url);
                }
                comment.Attachments = JsonSerializer.Serialize(attachmentUrls);
            }

            _context.Set<WorkItemComment>().Add(comment);

            // Log activity
            var activity = new WorkItemActivity
            {
                Id = Guid.NewGuid(),
                WorkItemId = workItemId,
                Type = ActivityType.CommentAdded,
                Description = "New comment added",
                PerformedById = currentUserId,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.Set<WorkItemActivity>().Add(activity);

            await _context.SaveChangesAsync(ct);

            return new WorkItemCommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                WorkItemId = comment.WorkItemId,
                CreatedById = comment.CreatedById,
                CreatedByName = "User", // TODO: Get from User relation
                CreatedByAvatar = "",
                CreatedByRole = "",
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                ParentCommentId = comment.ParentCommentId,
                Attachments = comment.Attachments
            };
        }

        public async Task<List<WorkItemCommentDto>> GetCommentsAsync(Guid workItemId, CancellationToken ct = default)
        {
            var comments = await _context.Set<WorkItemComment>()
                .Where(c => c.WorkItemId == workItemId && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync(ct);

            return BuildCommentTree(comments);
        }

        public async Task<WorkItemDocumentDto> AddDocumentAsync(Guid workItemId, IFormFile file, string documentType, string? description, Guid currentUserId, CancellationToken ct = default)
        {
            var workItemExists = await _context.Set<WorkItem>().AnyAsync(w => w.Id == workItemId && !w.IsDeleted, ct);
            if (!workItemExists)
                throw new ArgumentException("Work item not found");

            using var stream = file.OpenReadStream();
            var filePath = await _fileService.UploadFileAsync(stream, file.FileName, "work-item-documents");

            var document = new WorkItemDocument
            {
                Id = Guid.NewGuid(),
                WorkItemId = workItemId,
                DocumentType = Enum.Parse<WorkItemDocumentType>(documentType),
                FileName = file.FileName,
                FilePath = filePath,
                Description = description,
                FileType = file.ContentType,
                FileSize = file.Length,
                UploadedById = currentUserId,
                UploadedAt = DateTime.UtcNow,
                IsLatest = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Set<WorkItemDocument>().Add(document);

            // Log activity
            var activity = new WorkItemActivity
            {
                Id = Guid.NewGuid(),
                WorkItemId = workItemId,
                Type = ActivityType.DocumentAdded,
                Description = $"Document '{file.FileName}' uploaded",
                PerformedById = currentUserId,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.Set<WorkItemActivity>().Add(activity);

            await _context.SaveChangesAsync(ct);

            return new WorkItemDocumentDto
            {
                Id = document.Id,
                DocumentType = document.DocumentType.ToString(),
                FileName = document.FileName,
                FilePath = document.FilePath,
                Description = document.Description,
                FileSize = document.FileSize,
                FileSizeFormatted = FormatFileSize(document.FileSize),
                UploadedAt = document.UploadedAt,
                UploadedByUsername = "User" // TODO: Get from User relation
            };
        }

        public async Task<List<WorkItemDocumentDto>> GetDocumentsAsync(Guid workItemId, CancellationToken ct = default)
        {
            var documents = await _context.Set<WorkItemDocument>()
                .Where(d => d.WorkItemId == workItemId && !d.IsDeleted)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync(ct);

            return documents.Select(d => new WorkItemDocumentDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType.ToString(),
                FileName = d.FileName,
                FilePath = d.FilePath,
                Description = d.Description,
                FileSize = d.FileSize,
                FileSizeFormatted = FormatFileSize(d.FileSize),
                UploadedAt = d.UploadedAt,
                UploadedByUsername = "User" // TODO: Get from User relation
            }).ToList();
        }

        public async Task<List<WorkItemActivityDto>> GetActivitiesAsync(Guid workItemId, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var activities = await _context.Set<WorkItemActivity>()
                .Where(a => a.WorkItemId == workItemId)
                .OrderByDescending(a => a.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return activities.Select(a => new WorkItemActivityDto
            {
                Id = a.Id,
                Type = a.Type.ToString(),
                Description = a.Description,
                PerformedByUsername = "User", // TODO: Get from User relation
                Timestamp = a.Timestamp,
                Notes = a.Notes
            }).ToList();
        }

        public async Task<List<WorkItemUpdateHistoryDto>> GetUpdateHistoryAsync(Guid workItemId, CancellationToken ct = default)
        {
            var history = await _context.Set<WorkItemUpdateHistory>()
                .Where(h => h.WorkItemId == workItemId)
                .OrderByDescending(h => h.UpdatedAt)
                .ToListAsync(ct);

            return history.Select(h => new WorkItemUpdateHistoryDto
            {
                Id = h.Id,
                FieldName = h.FieldName,
                OldValue = h.OldValue,
                NewValue = h.NewValue,
                UpdatedByUsername = "User", // TODO: Get from User relation
                UpdatedAt = h.UpdatedAt,
                Reason = h.Reason
            }).ToList();
        }

        public async Task<ImportBudgetResponseDto> ImportFromExcelAsync(Guid projectId, IFormFile file, bool overwriteExisting, Guid currentUserId, CancellationToken ct = default)
        {
            using var stream = file.OpenReadStream();
            using var package = new OfficeOpenXml.ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            if (rowCount < 6)
                throw new ArgumentException("Excel file format is invalid");

            var importedItems = new List<WorkItemDto>();
            var errors = new List<string>();
            var parentStack = new Dictionary<int, Guid>(); // level -> parentId
            int sortOrder = 0;

            // Start from row 6 (data rows)
            for (int row = 6; row <= rowCount; row++)
            {
                try
                {
                    var stt = worksheet.Cells[row, 1].Text?.Trim();
                    var tenCongViec = worksheet.Cells[row, 2].Text?.Trim();

                    if (string.IsNullOrEmpty(tenCongViec))
                        continue;

                    // Determine level and parent
                    var (level, parentId) = DetermineHierarchy(stt, parentStack);

                    var workItem = new WorkItem
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projectId,
                        Code = stt ?? $"W{sortOrder:D5}",
                        OrderNumber = stt ?? sortOrder.ToString(),
                        Name = tenCongViec,
                        Level = level,
                        ParentId = parentId,
                        SortOrder = sortOrder++,
                        Type = level == 1 ? WorkItemType.Phase : (level == 2 ? WorkItemType.Task : WorkItemType.SubTask),
                        Status = WorkItemStatus.NotStarted,
                        Progress = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = currentUserId.ToString(),
                        UpdatedBy = currentUserId.ToString()
                    };

                    // Parse người thực hiện (column 3)
                    var nguoiThucHien = worksheet.Cells[row, 3].Text?.Trim();
                    if (!string.IsNullOrEmpty(nguoiThucHien))
                    {
                        workItem.Notes = $"Người thực hiện: {nguoiThucHien}";
                    }

                    // Parse thời gian (column 4)
                    var thoiGian = worksheet.Cells[row, 4].Text?.Trim();
                    if (int.TryParse(thoiGian?.Replace("ngày", "").Trim(), out int duration))
                    {
                        workItem.PlannedDuration = duration;
                    }

                    // Parse dates - try both Value (for Excel date) and Text (for formatted string)
                    var startDateCell = worksheet.Cells[row, 5];
                    var endDateCell = worksheet.Cells[row, 6];
                    var actualStartCell = worksheet.Cells[row, 7];

                    // Debug logging
                    Console.WriteLine($"Row {row} - {tenCongViec}");
                    Console.WriteLine($"  Start Cell Value: {startDateCell.Value?.GetType().Name} = {startDateCell.Value}");
                    Console.WriteLine($"  Start Cell Text: {startDateCell.Text}");
                    Console.WriteLine($"  End Cell Value: {endDateCell.Value?.GetType().Name} = {endDateCell.Value}");
                    Console.WriteLine($"  End Cell Text: {endDateCell.Text}");

                    // Try parsing start date - prioritize TEXT over DateTime to respect Excel's display format
                    if (DateTime.TryParseExact(startDateCell.Text?.Trim(),
                        new[] { "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy" },
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime parsedStart))
                    {
                        workItem.PlannedStartDate = parsedStart;
                        Console.WriteLine($"  ✓ Parsed start date (Text exact): {parsedStart:yyyy-MM-dd}");
                    }
                    else if (DateTime.TryParse(startDateCell.Text?.Trim(),
                        new System.Globalization.CultureInfo("vi-VN"),
                        System.Globalization.DateTimeStyles.None,
                        out DateTime parsedStartDefault))
                    {
                        workItem.PlannedStartDate = parsedStartDefault;
                        Console.WriteLine($"  ✓ Parsed start date (Text default): {parsedStartDefault:yyyy-MM-dd}");
                    }
                    else if (startDateCell.Value is DateTime sd)
                    {
                        workItem.PlannedStartDate = sd;
                        Console.WriteLine($"  ⚠ Parsed start date (DateTime - may be ambiguous): {sd:yyyy-MM-dd}");
                    }
                    else
                    {
                        Console.WriteLine($"  ✗ Failed to parse start date");
                    }

                    // Try parsing end date - prioritize TEXT over DateTime to respect Excel's display format
                    if (DateTime.TryParseExact(endDateCell.Text?.Trim(),
                        new[] { "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy" },
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime parsedEnd))
                    {
                        workItem.PlannedEndDate = parsedEnd;
                        Console.WriteLine($"  ✓ Parsed end date (Text exact): {parsedEnd:yyyy-MM-dd}");
                    }
                    else if (DateTime.TryParse(endDateCell.Text?.Trim(),
                        new System.Globalization.CultureInfo("vi-VN"),
                        System.Globalization.DateTimeStyles.None,
                        out DateTime parsedEndDefault))
                    {
                        workItem.PlannedEndDate = parsedEndDefault;
                        Console.WriteLine($"  ✓ Parsed end date (Text default): {parsedEndDefault:yyyy-MM-dd}");
                    }
                    else if (endDateCell.Value is DateTime ed)
                    {
                        workItem.PlannedEndDate = ed;
                        Console.WriteLine($"  ⚠ Parsed end date (DateTime - may be ambiguous): {ed:yyyy-MM-dd}");
                    }
                    else
                    {
                        Console.WriteLine($"  ✗ Failed to parse end date");
                    }

                    // Try parsing actual start date
                    if (actualStartCell.Value is DateTime asd)
                    {
                        workItem.ActualStartDate = asd;
                    }
                    else if (DateTime.TryParse(actualStartCell.Text?.Trim(),
                        new System.Globalization.CultureInfo("vi-VN"),
                        System.Globalization.DateTimeStyles.None,
                        out DateTime parsedActualStart))
                    {
                        workItem.ActualStartDate = parsedActualStart;
                    }

                    // Parse progress (column 9)
                    var tienDo = worksheet.Cells[row, 9].Text?.Trim().Replace("%", "");
                    if (decimal.TryParse(tienDo, out decimal progress))
                    {
                        workItem.Progress = progress;
                        if (progress >= 100)
                            workItem.Status = WorkItemStatus.Completed;
                        else if (progress > 0)
                            workItem.Status = WorkItemStatus.InProgress;
                    }

                    // Parse unit and quantity (columns 10, 11)
                    workItem.Unit = worksheet.Cells[row, 10].Text?.Trim();

                    var khoiLuongKH = worksheet.Cells[row, 11].Text?.Trim();
                    if (decimal.TryParse(khoiLuongKH?.Replace(",", ""), out decimal plannedQty))
                        workItem.PlannedQuantity = plannedQty;

                    var khoiLuongTT = worksheet.Cells[row, 12].Text?.Trim();
                    if (decimal.TryParse(khoiLuongTT?.Replace(",", ""), out decimal actualQty))
                        workItem.ActualQuantity = actualQty;

                    _context.Set<WorkItem>().Add(workItem);

                    // Update parent stack for hierarchy
                    parentStack[level] = workItem.Id;

                    // Remove deeper levels
                    var keysToRemove = parentStack.Keys.Where(k => k > level).ToList();
                    foreach (var key in keysToRemove)
                        parentStack.Remove(key);

                    importedItems.Add(await MapToDto(workItem, false, ct));
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync(ct);

            return new ImportBudgetResponseDto
            {
                Success = errors.Count == 0,
                Message = errors.Count == 0 ? "Import successful" : $"Import completed with {errors.Count} errors",
                ImportedCount = importedItems.Count,
                ErrorCount = errors.Count,
                Errors = errors,
                ImportedItems = importedItems
            };
        }

        private (int level, Guid? parentId) DetermineHierarchy(string? stt, Dictionary<int, Guid> parentStack)
        {
            if (string.IsNullOrEmpty(stt))
                return (1, null);

            stt = stt.Trim();

            // Level 1: I., II., III., IV., V. (Roman numerals with dot)
            if (System.Text.RegularExpressions.Regex.IsMatch(stt, @"^[IVX]+\.?\s*$"))
                return (1, null);

            // Level 4: 1.1.1, 1.2.1... (check this first before level 3)
            if (System.Text.RegularExpressions.Regex.IsMatch(stt, @"^\d+\.\d+\.\d+$"))
            {
                var parentId = parentStack.ContainsKey(3) ? parentStack[3] : (Guid?)null;
                return (4, parentId);
            }

            // Level 3: 1.1, 1.2, 2.1...
            if (System.Text.RegularExpressions.Regex.IsMatch(stt, @"^\d+\.\d+$"))
            {
                var parentId = parentStack.ContainsKey(2) ? parentStack[2] : (Guid?)null;
                return (3, parentId);
            }

            // Level 2: 1, 2, 3... (just numbers with optional dot/period)
            if (System.Text.RegularExpressions.Regex.IsMatch(stt, @"^\d+\.?\s*$"))
            {
                var parentId = parentStack.ContainsKey(1) ? parentStack[1] : (Guid?)null;
                return (2, parentId);
            }

            // Default to level 1 if no pattern matches
            return (1, null);
        }

        public async Task<byte[]> ExportToExcelAsync(Guid projectId, CancellationToken ct = default)
        {
            var workItems = await _context.Set<WorkItem>()
                .Where(w => w.ProjectId == projectId && !w.IsDeleted)
                .OrderBy(w => w.SortOrder)
                .ToListAsync(ct);

            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Work Items");

            // Headers
            worksheet.Cells[4, 1].Value = "STT";
            worksheet.Cells[4, 2].Value = "Công việc";
            worksheet.Cells[4, 3].Value = "Người thực hiện";
            worksheet.Cells[4, 4].Value = "Thời gian";
            worksheet.Cells[4, 5].Value = "Bắt đầu KH";
            worksheet.Cells[4, 6].Value = "Kết thúc KH";
            worksheet.Cells[4, 7].Value = "Bắt đầu TT";
            worksheet.Cells[4, 8].Value = "Kết thúc TT";
            worksheet.Cells[4, 9].Value = "Tiến độ";
            worksheet.Cells[4, 10].Value = "Đơn vị";
            worksheet.Cells[4, 11].Value = "Khối lượng KH";
            worksheet.Cells[4, 12].Value = "Khối lượng TT";
            worksheet.Cells[4, 13].Value = "Khối lượng CL";

            int row = 6;
            foreach (var item in workItems)
            {
                worksheet.Cells[row, 1].Value = item.OrderNumber;
                worksheet.Cells[row, 2].Value = item.Name;
                worksheet.Cells[row, 4].Value = item.PlannedDuration > 0 ? $"{item.PlannedDuration} ngày" : "";
                worksheet.Cells[row, 5].Value = item.PlannedStartDate?.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 6].Value = item.PlannedEndDate?.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 7].Value = item.ActualStartDate?.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 8].Value = item.ActualEndDate?.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 9].Value = $"{item.Progress}%";
                worksheet.Cells[row, 10].Value = item.Unit;
                worksheet.Cells[row, 11].Value = item.PlannedQuantity;
                worksheet.Cells[row, 12].Value = item.ActualQuantity;
                worksheet.Cells[row, 13].Value = (item.PlannedQuantity ?? 0) - (item.ActualQuantity ?? 0);
                row++;
            }

            return await package.GetAsByteArrayAsync(ct);
        }

        public async Task<GanttChartDataDto> GetGanttChartDataAsync(Guid projectId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
        {
            var project = await _context.Set<Project>().FindAsync(projectId);
            if (project == null)
                throw new ArgumentException("Project not found");

            var workItems = await _context.Set<WorkItem>()
                .Where(w => w.ProjectId == projectId && !w.IsDeleted)
                .OrderBy(w => w.SortOrder)
                .ToListAsync(ct);

            if (workItems.Count == 0)
            {
                return new GanttChartDataDto
                {
                    ProjectId = projectId,
                    ProjectName = project.Name,
                    ProjectStartDate = project.StartDate,
                    ProjectEndDate = project.EstimatedCompletionDate ?? DateTime.UtcNow.AddMonths(6),
                    Items = new List<GanttTaskDto>(),
                    Timeline = GenerateTimeline(project.StartDate, project.EstimatedCompletionDate ?? DateTime.UtcNow.AddMonths(6))
                };
            }

            var projectStart = workItems.Min(w => w.PlannedStartDate) ?? project.StartDate;
            var projectEnd = workItems.Max(w => w.PlannedEndDate) ?? project.EstimatedCompletionDate ?? DateTime.UtcNow.AddMonths(6);

            // Create a dictionary to build the hierarchy
            var taskDict = new Dictionary<Guid, GanttTaskDto>();

            foreach (var item in workItems)
            {
                var dayOffset = 0;
                var duration = 0;
                var barWidth = 0;

                if (item.PlannedStartDate.HasValue && item.PlannedEndDate.HasValue)
                {
                    dayOffset = (int)(item.PlannedStartDate.Value - projectStart).TotalDays;
                    duration = (int)(item.PlannedEndDate.Value - item.PlannedStartDate.Value).TotalDays;
                    barWidth = duration * 60; // 60px per day
                }

                var task = new GanttTaskDto
                {
                    Id = item.Id,
                    Code = item.Code,
                    OrderNumber = item.OrderNumber,
                    Name = item.Name,
                    ParentId = item.ParentId,
                    Level = item.Level,
                    Order = item.SortOrder,
                    HasChildren = false, // Will be set later
                    IsExpanded = item.IsExpanded,
                    StartDate = item.PlannedStartDate,
                    EndDate = item.PlannedEndDate,
                    Duration = duration,
                    Progress = item.Progress,
                    Status = item.Status.ToString(),
                    StatusColor = GetStatusColor(item.Status),
                    Assignees = ParseAssignees(item.AssigneeIds),
                    Dependencies = ParsePrerequisites(item.PrerequisiteIds),
                    Children = new List<GanttTaskDto>(),
                    DayOffset = dayOffset,
                    BarWidth = barWidth
                };

                taskDict[item.Id] = task;
            }

            // Build hierarchy
            var rootTasks = new List<GanttTaskDto>();

            foreach (var task in taskDict.Values)
            {
                if (task.ParentId.HasValue && taskDict.ContainsKey(task.ParentId.Value))
                {
                    var parent = taskDict[task.ParentId.Value];
                    parent.Children.Add(task);
                    parent.HasChildren = true;
                }
                else
                {
                    rootTasks.Add(task);
                }
            }

            var timeline = GenerateTimeline(projectStart, projectEnd);

            return new GanttChartDataDto
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                ProjectStartDate = projectStart,
                ProjectEndDate = projectEnd,
                Items = rootTasks,
                Timeline = timeline
            };
        }

        public async Task UpdateWorkItemStatusesAsync(Guid projectId, CancellationToken ct = default)
        {
            var workItems = await _context.Set<WorkItem>()
                .Where(w => w.ProjectId == projectId && !w.IsDeleted)
                .ToListAsync(ct);

            var now = DateTime.UtcNow.Date;

            foreach (var item in workItems)
            {
                var newStatus = CalculateStatus(item, now);
                if (item.Status != newStatus)
                {
                    item.Status = newStatus;
                    item.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync(ct);
        }

        // Private helper methods
        private async Task<WorkItemDto> MapToDto(WorkItem item, bool includeChildren, CancellationToken ct)
        {
            var dto = MapToSimpleDto(item);
            dto.Assignees = ParseAssignees(item.AssigneeIds);
            dto.ResponsiblePerson = await GetResponsiblePerson(item.ResponsiblePersonId, ct);
            dto.CommentCount = await _context.Set<WorkItemComment>().CountAsync(c => c.WorkItemId == item.Id && !c.IsDeleted, ct);
            dto.DocumentCount = await _context.Set<WorkItemDocument>().CountAsync(d => d.WorkItemId == item.Id && !d.IsDeleted, ct);
            dto.MaterialCount = await _context.Set<WorkItemMaterial>().CountAsync(m => m.WorkItemId == item.Id, ct);

            // Load children if requested
            if (includeChildren)
            {
                var children = await _context.Set<WorkItem>()
                    .Where(w => w.ParentId == item.Id && !w.IsDeleted)
                    .OrderBy(w => w.SortOrder)
                    .ToListAsync(ct);

                dto.Children = new List<WorkItemDto>();
                foreach (var child in children)
                {
                    var childDto = await MapToDto(child, true, ct); // Recursive
                    dto.Children.Add(childDto);
                }
            }

            return dto;
        }

        private WorkItemDto MapToSimpleDto(WorkItem item)
        {
            return new WorkItemDto
            {
                Id = item.Id,
                ProjectId = item.ProjectId,
                Code = item.Code,
                OrderNumber = item.OrderNumber,
                Name = item.Name,
                Description = item.Description,
                ParentId = item.ParentId,
                Level = item.Level,
                SortOrder = item.SortOrder,
                Type = item.Type.ToString(),
                PlannedStartDate = item.PlannedStartDate,
                PlannedEndDate = item.PlannedEndDate,
                ActualStartDate = item.ActualStartDate,
                ActualEndDate = item.ActualEndDate,
                PlannedDuration = item.PlannedDuration,
                ActualDuration = item.ActualDuration,
                Progress = item.Progress,
                Status = item.Status.ToString(),
                StatusLabel = GetStatusLabel(item.Status),
                StatusColor = GetStatusColor(item.Status),
                PlannedQuantity = item.PlannedQuantity,
                Unit = item.Unit,
                ActualQuantity = item.ActualQuantity,
                UnitPrice = item.UnitPrice,
                TotalAmount = item.TotalAmount,
                CompactionFactor = item.CompactionFactor,
                IsExpanded = item.IsExpanded,
                IsHidden = item.IsHidden,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };
        }

        private async Task<int> GetNextSortOrder(Guid projectId, Guid? parentId, CancellationToken ct)
        {
            var query = _context.Set<WorkItem>().Where(w => w.ProjectId == projectId && !w.IsDeleted);

            if (parentId.HasValue)
                query = query.Where(w => w.ParentId == parentId);
            else
                query = query.Where(w => w.ParentId == null);

            var maxOrder = await query.MaxAsync(w => (int?)w.SortOrder, ct);
            return (maxOrder ?? 0) + 1;
        }

        private List<AssigneeDto> ParseAssignees(string? assigneeIdsJson)
        {
            if (string.IsNullOrEmpty(assigneeIdsJson)) return new List<AssigneeDto>();

            try
            {
                var ids = JsonSerializer.Deserialize<List<Guid>>(assigneeIdsJson) ?? new List<Guid>();
                // TODO: Fetch actual user data
                return ids.Select(id => new AssigneeDto
                {
                    UserId = id,
                    Username = "User",
                    FullName = "User Name"
                }).ToList();
            }
            catch
            {
                return new List<AssigneeDto>();
            }
        }

        private async Task<ResponsiblePersonDto?> GetResponsiblePerson(string? responsiblePersonId, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(responsiblePersonId)) return null;

            if (Guid.TryParse(responsiblePersonId, out var userId))
            {
                // TODO: Fetch actual user data
                return new ResponsiblePersonDto
                {
                    UserId = userId,
                    Username = "User",
                    FullName = "User Name"
                };
            }

            return null;
        }

        private List<Guid> ParsePrerequisites(string? prerequisiteIdsJson)
        {
            if (string.IsNullOrEmpty(prerequisiteIdsJson)) return new List<Guid>();

            try
            {
                return JsonSerializer.Deserialize<List<Guid>>(prerequisiteIdsJson) ?? new List<Guid>();
            }
            catch
            {
                return new List<Guid>();
            }
        }

        private List<Guid> ParseAssigneeIds(string? assigneeIdsJson)
        {
            if (string.IsNullOrEmpty(assigneeIdsJson)) return new List<Guid>();

            try
            {
                return JsonSerializer.Deserialize<List<Guid>>(assigneeIdsJson) ?? new List<Guid>();
            }
            catch
            {
                return new List<Guid>();
            }
        }

        private List<WorkItemCommentDto> BuildCommentTree(List<WorkItemComment> comments)
        {
            var commentDtos = comments.Select(c => new WorkItemCommentDto
            {
                Id = c.Id,
                Content = c.Content,
                WorkItemId = c.WorkItemId,
                CreatedById = c.CreatedById,
                CreatedByName = "User", // TODO: Get from User relation
                CreatedByAvatar = "",
                CreatedByRole = "",
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                ParentCommentId = c.ParentCommentId,
                Attachments = c.Attachments,
                Replies = new List<WorkItemCommentDto>()
            }).ToList();

            var rootComments = commentDtos.Where(c => c.ParentCommentId == null).ToList();

            foreach (var root in rootComments)
            {
                PopulateReplies(root, commentDtos);
            }

            return rootComments;
        }

        private void PopulateReplies(WorkItemCommentDto parent, List<WorkItemCommentDto> allComments)
        {
            parent.Replies = allComments.Where(c => c.ParentCommentId == parent.Id).ToList();
            foreach (var reply in parent.Replies)
            {
                PopulateReplies(reply, allComments);
            }
        }

        private string GetStatusLabel(WorkItemStatus status)
        {
            return status switch
            {
                WorkItemStatus.NotStarted => "Not Started",
                WorkItemStatus.InProgress => "In Progress",
                WorkItemStatus.Completed => "Completed",
                WorkItemStatus.Overdue => "Overdue",
                WorkItemStatus.Paused => "Paused",
                WorkItemStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }

        private string GetStatusColor(WorkItemStatus status)
        {
            return status switch
            {
                WorkItemStatus.NotStarted => "#9E9E9E",
                WorkItemStatus.InProgress => "#2196F3",
                WorkItemStatus.Completed => "#4CAF50",
                WorkItemStatus.Overdue => "#F44336",
                WorkItemStatus.Paused => "#FF9800",
                WorkItemStatus.Cancelled => "#757575",
                _ => "#9E9E9E"
            };
        }

        private WorkItemStatus CalculateStatus(WorkItem item, DateTime now)
        {
            if (item.Progress >= 100) return WorkItemStatus.Completed;
            if (!item.PlannedStartDate.HasValue || item.PlannedStartDate.Value > now) return WorkItemStatus.NotStarted;
            if (item.PlannedEndDate.HasValue && item.PlannedEndDate.Value < now && item.Progress < 100) return WorkItemStatus.Overdue;
            if (item.PlannedStartDate.Value <= now) return WorkItemStatus.InProgress;
            return WorkItemStatus.NotStarted;
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private GanttTimelineDto GenerateTimeline(DateTime start, DateTime end)
        {
            var totalDays = (int)(end - start).TotalDays;
            var totalWeeks = (int)Math.Ceiling(totalDays / 7.0);

            var months = new List<GanttMonthDto>();
            var weeks = new List<GanttWeekDto>();

            var current = start;
            int weekNumber = 1;

            while (current < end)
            {
                var weekEnd = current.AddDays(6) > end ? end : current.AddDays(6);

                weeks.Add(new GanttWeekDto
                {
                    WeekNumber = weekNumber++,
                    StartDate = current,
                    EndDate = weekEnd,
                    Label = $"Week {weekNumber - 1}",
                    PixelWidth = 60
                });

                current = current.AddDays(7);
            }

            // Group weeks by month
            var monthGroups = weeks.GroupBy(w => new { w.StartDate.Year, w.StartDate.Month });
            foreach (var group in monthGroups)
            {
                months.Add(new GanttMonthDto
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    Label = new DateTime(group.Key.Year, group.Key.Month, 1).ToString("MMMM yyyy"),
                    WeekCount = group.Count(),
                    PixelWidth = group.Count() * 60
                });
            }

            return new GanttTimelineDto
            {
                StartDate = start,
                EndDate = end,
                TotalDays = totalDays,
                TotalWeeks = totalWeeks,
                Months = months,
                Weeks = weeks
            };
        }

        public async Task<WorkItemDto> AssignUsersAsync(Guid id, List<Guid> userIds, Guid currentUserId, CancellationToken ct = default)
        {
            var workItem = await _context.WorkItems
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, ct);

            if (workItem == null)
                throw new ArgumentException("Work item not found");

            // Update AssigneeIds as JSON
            workItem.AssigneeIds = userIds.Count > 0 ? JsonSerializer.Serialize(userIds) : null;
            workItem.UpdatedAt = DateTime.UtcNow;
            workItem.UpdatedBy = currentUserId.ToString();

            await _context.SaveChangesAsync(ct);

            // Log activity
            var activity = new WorkItemActivity
            {
                WorkItemId = id,
                Type = Domain.Entities.ActivityType.AssigneeAdded,
                Description = $"Assigned {userIds.Count} user(s)",
                PerformedById = currentUserId,
                Timestamp = DateTime.UtcNow
            };
            _context.WorkItemActivities.Add(activity);
            await _context.SaveChangesAsync(ct);

            return await MapToDto(workItem, false, ct);
        }

        public async Task<WorkItemDto> UnassignUserAsync(Guid id, Guid userId, Guid currentUserId, CancellationToken ct = default)
        {
            var workItem = await _context.WorkItems
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, ct);

            if (workItem == null)
                throw new ArgumentException("Work item not found");

            // Parse current assignees
            var currentAssignees = ParseAssigneeIds(workItem.AssigneeIds);

            // Remove the user
            currentAssignees.Remove(userId);

            // Update AssigneeIds
            workItem.AssigneeIds = currentAssignees.Count > 0 ? JsonSerializer.Serialize(currentAssignees) : null;
            workItem.UpdatedAt = DateTime.UtcNow;
            workItem.UpdatedBy = currentUserId.ToString();

            await _context.SaveChangesAsync(ct);

            // Log activity
            var activity = new WorkItemActivity
            {
                WorkItemId = id,
                Type = Domain.Entities.ActivityType.AssigneeRemoved,
                Description = $"Unassigned user {userId}",
                PerformedById = currentUserId,
                Timestamp = DateTime.UtcNow
            };
            _context.WorkItemActivities.Add(activity);
            await _context.SaveChangesAsync(ct);

            return await MapToDto(workItem, false, ct);
        }
    }
}
