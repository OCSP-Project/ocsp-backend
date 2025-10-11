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
            var result = await _chatService.SendMessageAsync(
                Guid.Parse(conversationId),
                Guid.Parse(senderId),
                content);

            // Access properties from result.Message, not result directly
            var messageDto = new
            {
                Id = result.Message.Id,                    // result.Message.Id instead of result.Id
                SenderId = result.Message.SenderId,        // result.Message.SenderId
                Content = result.Message.Content,          // result.Message.Content
                CreatedAt = result.Message.CreatedAt,      // result.Message.CreatedAt
                Warning = result.Warning                   // Include warning info
            };

            await Clients.Group(conversationId).SendAsync("ReceiveMessage", messageDto);

            // If there's a warning, send it separately to the sender
            if (result.Warning != null)
            {
                await Clients.Caller.SendAsync("ReceiveWarning", result.Warning);
            }
        }
    }
}
