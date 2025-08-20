using Microsoft.AspNetCore.SignalR;
using OCSP.Application.Services.Interfaces;

namespace OCSP.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task LeaveConversation(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task SendMessage(string conversationId, string senderId, string content)
        {
            var message = await _chatService.SendMessageAsync(Guid.Parse(conversationId), Guid.Parse(senderId), content);

            // Broadcast tới các client trong group
            await Clients.Group(conversationId).SendAsync("ReceiveMessage", new
            {
                message.Id,
                message.SenderId,
                message.Content,
                message.CreatedAt
            });
        }
    }
}
