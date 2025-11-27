namespace OCSP.Application.DTOs.Budget
{
    public class WorkItemCommentDto
    {
        public Guid Id { get; set; }
        public Guid WorkItemId { get; set; }
        public Guid CreatedById { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public string CreatedByAvatar { get; set; } = string.Empty;
        public string CreatedByRole { get; set; } = string.Empty; // Role in project
        public string Content { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
        public List<WorkItemCommentDto>? Replies { get; set; }
        public List<MentionedUserDto> MentionedUsers { get; set; } = new();
        public string? Attachments { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class MentionedUserDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
