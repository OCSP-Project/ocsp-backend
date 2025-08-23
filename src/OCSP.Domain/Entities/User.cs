

namespace OCSP.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public OCSP.Domain.Enums.UserRole Role { get; set; }

        // Authentication specific properties
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Profile properties - Tạm thời comment để giữ nguyên chức năng đăng nhập/đăng ký
        // public string? FirstName { get; set; }
        // public string? LastName { get; set; }
        // public string? PhoneNumber { get; set; }
        // public string? Address { get; set; }
        // public string? City { get; set; }
        // public string? State { get; set; }
        // public string? Country { get; set; }
        // public string? Bio { get; set; }
        // public string? AvatarUrl { get; set; }

        // Basic audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // 🔗 Navigation properties cho Chat
        public ICollection<ConversationParticipant> Conversations { get; set; } = new List<ConversationParticipant>();
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        
                // 🔗 Navigation properties cho Profile - Tạm thời comment
        // public ICollection<ProfileDocument> ProfileDocuments { get; set; } = new List<ProfileDocument>();
    }
}