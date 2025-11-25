using Microsoft.EntityFrameworkCore;
using OCSP.Application.DTOs.Budget;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;

namespace OCSP.Application.Services
{
    public class WorkItemCommentService : IWorkItemCommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public WorkItemCommentService(
            ApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<WorkItemCommentDto> CreateAsync(CreateWorkItemCommentDto dto, Guid userId, CancellationToken ct = default)
        {
            // Verify work item exists
            var workItem = await _context.WorkItems
                .Include(w => w.Project)
                .FirstOrDefaultAsync(w => w.Id == dto.WorkItemId, ct);

            if (workItem == null)
                throw new ArgumentException("Work item not found");

            // Verify parent comment if replying
            if (dto.ParentCommentId.HasValue)
            {
                var parentExists = await _context.WorkItemComments
                    .AnyAsync(c => c.Id == dto.ParentCommentId.Value && c.WorkItemId == dto.WorkItemId, ct);

                if (!parentExists)
                    throw new ArgumentException("Parent comment not found");
            }

            // Create comment
            var comment = new WorkItemComment
            {
                Id = Guid.NewGuid(),
                WorkItemId = dto.WorkItemId,
                CreatedById = userId,
                Content = dto.Content,
                ParentCommentId = dto.ParentCommentId,
                Attachments = dto.Attachments,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.WorkItemComments.Add(comment);

            // Add mentions
            if (dto.MentionedUserIds != null && dto.MentionedUserIds.Any())
            {
                var uniqueMentions = dto.MentionedUserIds.Distinct().ToList();
                foreach (var mentionedUserId in uniqueMentions)
                {
                    // Verify mentioned user exists and is in project
                    var userInProject = await IsUserInProjectAsync(workItem.ProjectId, mentionedUserId, ct);
                    if (!userInProject)
                        continue; // Skip invalid mentions

                    var mention = new WorkItemCommentMention
                    {
                        Id = Guid.NewGuid(),
                        CommentId = comment.Id,
                        MentionedUserId = mentionedUserId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.WorkItemCommentMentions.Add(mention);

                    // Send notification to mentioned user
                    await SendMentionNotificationAsync(comment, workItem, mentionedUserId, userId, ct);
                }
            }

            await _context.SaveChangesAsync(ct);

            // Return created comment with details
            return await GetByIdAsync(comment.Id, ct)
                   ?? throw new InvalidOperationException("Failed to retrieve created comment");
        }

        public async Task<WorkItemCommentDto> UpdateAsync(Guid commentId, UpdateWorkItemCommentDto dto, Guid userId, CancellationToken ct = default)
        {
            var comment = await _context.WorkItemComments
                .Include(c => c.Mentions)
                .Include(c => c.WorkItem)
                    .ThenInclude(w => w.Project)
                .FirstOrDefaultAsync(c => c.Id == commentId, ct);

            if (comment == null)
                throw new ArgumentException("Comment not found");

            // Only creator can update
            if (comment.CreatedById != userId)
                throw new UnauthorizedAccessException("You can only update your own comments");

            // Update content
            comment.Content = dto.Content;
            comment.Attachments = dto.Attachments;
            comment.UpdatedAt = DateTime.UtcNow;

            // Update mentions - remove old and add new
            var existingMentions = comment.Mentions.ToList();
            _context.WorkItemCommentMentions.RemoveRange(existingMentions);

            if (dto.MentionedUserIds != null && dto.MentionedUserIds.Any())
            {
                var uniqueMentions = dto.MentionedUserIds.Distinct().ToList();
                var newMentionedUserIds = uniqueMentions.Except(existingMentions.Select(m => m.MentionedUserId)).ToList();

                foreach (var mentionedUserId in uniqueMentions)
                {
                    var userInProject = await IsUserInProjectAsync(comment.WorkItem.ProjectId, mentionedUserId, ct);
                    if (!userInProject)
                        continue;

                    var mention = new WorkItemCommentMention
                    {
                        Id = Guid.NewGuid(),
                        CommentId = comment.Id,
                        MentionedUserId = mentionedUserId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.WorkItemCommentMentions.Add(mention);

                    // Send notification only for NEW mentions
                    if (newMentionedUserIds.Contains(mentionedUserId))
                    {
                        await SendMentionNotificationAsync(comment, comment.WorkItem, mentionedUserId, userId, ct);
                    }
                }
            }

            await _context.SaveChangesAsync(ct);

            return await GetByIdAsync(commentId, ct)
                   ?? throw new InvalidOperationException("Failed to retrieve updated comment");
        }

        public async Task DeleteAsync(Guid commentId, Guid userId, CancellationToken ct = default)
        {
            var comment = await _context.WorkItemComments
                .FirstOrDefaultAsync(c => c.Id == commentId, ct);

            if (comment == null)
                throw new ArgumentException("Comment not found");

            // Only creator can delete
            if (comment.CreatedById != userId)
                throw new UnauthorizedAccessException("You can only delete your own comments");

            // Soft delete
            comment.IsDeleted = true;
            comment.DeletedAt = DateTime.UtcNow;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
        }

        public async Task<List<WorkItemCommentDto>> GetByWorkItemIdAsync(Guid workItemId, CancellationToken ct = default)
        {
            var comments = await _context.WorkItemComments
                .Include(c => c.CreatedBy)
                .Include(c => c.Mentions)
                    .ThenInclude(m => m.MentionedUser)
                .Include(c => c.Replies.Where(r => !r.IsDeleted))
                    .ThenInclude(r => r.CreatedBy)
                .Where(c => c.WorkItemId == workItemId && !c.IsDeleted && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(ct);

            // Get work item to determine user roles
            var workItem = await _context.WorkItems
                .Include(w => w.Project)
                .FirstOrDefaultAsync(w => w.Id == workItemId, ct);

            if (workItem == null)
                return new List<WorkItemCommentDto>();

            return comments.Select(c => MapToDto(c, workItem.ProjectId)).ToList();
        }

        public async Task<WorkItemCommentDto?> GetByIdAsync(Guid commentId, CancellationToken ct = default)
        {
            var comment = await _context.WorkItemComments
                .Include(c => c.CreatedBy)
                .Include(c => c.Mentions)
                    .ThenInclude(m => m.MentionedUser)
                .Include(c => c.Replies.Where(r => !r.IsDeleted))
                    .ThenInclude(r => r.CreatedBy)
                .Include(c => c.WorkItem)
                .FirstOrDefaultAsync(c => c.Id == commentId, ct);

            if (comment == null || comment.IsDeleted)
                return null;

            return MapToDto(comment, comment.WorkItem.ProjectId);
        }

        #region Private Helper Methods

        private async Task<bool> IsUserInProjectAsync(Guid projectId, Guid userId, CancellationToken ct)
        {
            // Check if user is homeowner
            var isHomeowner = await _context.Projects
                .AnyAsync(p => p.Id == projectId && p.HomeownerId == userId, ct);

            if (isHomeowner)
                return true;

            // Check if user is contractor
            var isContractor = await _context.Projects
                .AnyAsync(p => p.Id == projectId && p.ContractorId == userId, ct);

            if (isContractor)
                return true;

            // Check if user is participant (supervisor or other role)
            var isParticipant = await _context.ProjectParticipants
                .AnyAsync(p => p.ProjectId == projectId && p.UserId == userId, ct);

            return isParticipant;
        }

        private async Task<string> GetUserRoleInProjectAsync(Guid projectId, Guid userId, CancellationToken ct)
        {
            var project = await _context.Projects.FindAsync(new object[] { projectId }, ct);

            if (project == null)
                return "Unknown";

            if (project.HomeownerId == userId)
                return "Chủ đầu tư";

            if (project.ContractorId == userId)
                return "Nhà thầu";

            var participant = await _context.ProjectParticipants
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.UserId == userId, ct);

            if (participant != null)
            {
                return participant.DetailedRole switch
                {
                    ProjectParticipantRole.MainSupervisor => "Giám sát chính",
                    ProjectParticipantRole.SubSupervisor => "Giám sát phụ",
                    _ => "Thành viên"
                };
            }

            return "Thành viên";
        }

        private async Task SendMentionNotificationAsync(
            WorkItemComment comment,
            WorkItem workItem,
            Guid mentionedUserId,
            Guid mentioningUserId,
            CancellationToken ct)
        {
            var mentioningUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == mentioningUserId, ct);

            if (mentioningUser == null)
                return;

            var title = $"{mentioningUser.Username} đã nhắc đến bạn trong thảo luận";
            var message = $"{mentioningUser.Username} đã nhắc đến bạn trong thảo luận về công việc \"{workItem.Name}\"";

            await _notificationService.CreateAsync(new DTOs.Notification.CreateNotificationDto
            {
                UserId = mentionedUserId,
                Title = title,
                Message = message,
                Type = NotificationType.WorkItemCommentMention,
                ReferenceId = comment.Id,
                ActionUrl = $"/projects/{workItem.ProjectId}/budget?workItemId={workItem.Id}&commentId={comment.Id}",
                ProjectId = workItem.ProjectId
            }, ct);
        }

        private WorkItemCommentDto MapToDto(WorkItemComment comment, Guid projectId)
        {
            var userRole = GetUserRoleInProjectAsync(projectId, comment.CreatedById, CancellationToken.None).Result;

            return new WorkItemCommentDto
            {
                Id = comment.Id,
                WorkItemId = comment.WorkItemId,
                CreatedById = comment.CreatedById,
                CreatedByName = comment.CreatedBy?.Username ?? "Unknown",
                CreatedByAvatar = "",
                CreatedByRole = userRole,
                Content = comment.Content,
                ParentCommentId = comment.ParentCommentId,
                Replies = comment.Replies?
                    .Where(r => !r.IsDeleted)
                    .Select(r => MapToDto(r, projectId))
                    .ToList(),
                MentionedUsers = comment.Mentions?
                    .Select(m => new MentionedUserDto
                    {
                        UserId = m.MentionedUserId,
                        Username = m.MentionedUser?.Username ?? "",
                        FullName = m.MentionedUser?.Username ?? ""
                    })
                    .ToList() ?? new List<MentionedUserDto>(),
                Attachments = comment.Attachments,
                IsDeleted = comment.IsDeleted,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }

        #endregion
    }
}
