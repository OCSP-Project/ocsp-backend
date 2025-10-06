using Microsoft.EntityFrameworkCore;
using OCSP.Application.Services.Interfaces;
using OCSP.Application.DTOs.Chat;
using OCSP.Application.DTOs.Contractor;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;
using System.Text.RegularExpressions;

namespace OCSP.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly IContractorService _contractorService;

        // Regex patterns for contact info detection
        private static readonly List<Regex> ContactPatterns = new()
        {
            new Regex(@"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b", RegexOptions.Compiled), // Phone numbers
            new Regex(@"\b0\d{9,10}\b", RegexOptions.Compiled), // VN phone numbers
            new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled), // Email
            new Regex(@"\b(zalo|viber|telegram|whatsapp|facebook|fb|messenger|instagram|tiktok|youtube)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase), // Social media
            new Regex(@"\b(liên hệ|gọi|call|sdt|phone|số điện thoại|gmail|yahoo|hotmail|email|mail)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase), // Vietnamese contact keywords
            new Regex(@"(https?:\/\/)?(www\.)?[a-zA-Z0-9]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z]{2,})+", RegexOptions.Compiled | RegexOptions.IgnoreCase), // URLs/websites
            new Regex(@"\b(skype|wechat|line|discord)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase), // Other messaging apps
        };

        public ChatService(ApplicationDbContext context, IContractorService contractorService)
        {
            _context = context;
            _contractorService = contractorService;
        }

        public async Task<Conversation> StartConversationAsync(Guid? projectId, Guid[] participantIds) // Make projectId nullable
        {
            // Check if conversation already exists
            var existing = await _context.Conversations
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c =>
                    c.ProjectId == projectId && // Handle null projectId
                    c.Participants.Count == participantIds.Length &&
                    c.Participants.All(p => participantIds.Contains(p.UserId))
                );

            if (existing != null) return existing;

            var conversation = new Conversation
            {
                ProjectId = projectId, // Can be null for consultation chats
                Participants = participantIds.Select(id => new ConversationParticipant
                {
                    UserId = id
                }).ToList()
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return conversation;
        }

        public async Task<ChatMessageResult> SendMessageAsync(Guid conversationId, Guid senderId, string content)
        {
            // Validate message for contact info
            var validation = await ValidateMessageContentAsync(content, senderId, conversationId);

            var message = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                IsRead = false
            };

            _context.ChatMessages.Add(message);

            // Log communication for tracking
            var conversation = await _context.Conversations.FindAsync(conversationId);
            var otherParticipant = await _context.ConversationParticipants
                .Where(p => p.ConversationId == conversationId && p.UserId != senderId)
                .FirstOrDefaultAsync();

            if (otherParticipant != null && conversation != null)
            {
                await _contractorService.LogCommunicationAsync(
                    senderId,
                    otherParticipant.UserId,
                    content,
                    Domain.Enums.CommunicationType.Chat,
                    conversation.ProjectId);
            }

            await _context.SaveChangesAsync();

            return new ChatMessageResult
            {
                Message = message,
                Warning = validation
            };
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid conversationId)
        {
            return await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId)
        {
            return await _context.Conversations
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .ToListAsync();
        }

        private async Task<CommunicationWarningDto?> ValidateMessageContentAsync(string content, Guid senderId, Guid conversationId)
        {
            var containsContactInfo = ContactPatterns.Any(pattern => pattern.IsMatch(content));

            if (containsContactInfo)
            {
                // Get sender's warning count
                var warningCount = await _context.Communications
                    .Where(c => c.FromUserId == senderId && c.ContainsContactInfo)
                    .CountAsync();

                var warningLevel = warningCount switch
                {
                    0 => 1,
                    1 or 2 => 2,
                    >= 3 => 3,
                    _ => 1  // Add default case to handle negative values
                };

                var warningMessage = warningLevel switch
                {
                    1 => "⚠️ Cảnh báo: Chúng tôi phát hiện tin nhắn có thể chứa thông tin liên hệ. Vui lòng sử dụng hệ thống chat của OCSP để đảm bảo bảo mật và quyền lợi của bạn.",
                    2 => "⚠️ Cảnh báo lần 2: Việc chia sẻ thông tin liên hệ vi phạm chính sách platform. Tiếp tục vi phạm có thể dẫn đến hạn chế tài khoản.",
                    _ => "🚫 Cảnh báo cuối: Tài khoản của bạn sẽ bị hạn chế nếu tiếp tục vi phạm chính sách. Mọi giao dịch phải thực hiện qua OCSP."
                };

                return new CommunicationWarningDto
                {
                    Message = warningMessage,
                    Reason = "Phát hiện thông tin liên hệ trong tin nhắn",
                    WarningLevel = warningLevel,
                    RequiresAcknowledgment = warningLevel >= 2
                };
            }

            return null;
        }
    }
}