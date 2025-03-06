using ChatMeeting.Core.Application.Services;
using ChatMeeting.Core.Domain.Dtos;
using ChatMeeting.Core.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChatMeeting.API.Hubs
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly UserConnectionService _userConnectionService;
        private string _username => _userConnectionService.GetClaimValue(Context.User, ClaimTypes.NameIdentifier);

        private string _userId => _userConnectionService.GetClaimValue(Context.User, JwtRegisteredClaimNames.Jti);

        private const string _mainChat = "Global";
        public MessageHub(UserConnectionService userConnectionService, IChatService chatService)
        {
            _userConnectionService = userConnectionService;
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
           
            _userConnectionService.AddConnection(_username, Context.ConnectionId);
           
            await JoinChat(_mainChat);
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _userConnectionService.RemoveConnection(_username);

            await LeaveChat(_mainChat);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToChat(string chatId, string message)
        {
            await Clients.Groups(_mainChat).SendAsync("ReceiveMessage", _username, message);
            var messageDto = CreateMessage(chatId, message);
            await _chatService.SaveMessage(messageDto);
        }

        private MessageDto CreateMessage(string chatId, string message)
        {
            return new MessageDto
            {
                MessageText = message,
                ChatId = Guid.Parse(chatId),
                SenderId = Guid.Parse(_userId),
                Sender = _username
            };
        }

        private async Task JoinChat(string chatName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatName);
        }

        private async Task LeaveChat(string mainChat)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, mainChat);
        }
    }
}
