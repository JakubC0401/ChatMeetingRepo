using ChatMeeting.Core.Domain.Dtos;
using ChatMeeting.Core.Domain.Interfaces.Repositories;
using ChatMeeting.Core.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatMeeting.Core.Domain.Interfaces.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<ChatService> _logger;
        public ChatService(IChatRepository chatRepository, ILogger<ChatService> logger)
        {
            _chatRepository = chatRepository;
            _logger = logger;
        }

        public async Task<ChatDto> GetPaginatedChat(string chatName, int pageNumber, int pageSize)
        {
            var chat = await _chatRepository.GetChatWithMessages(chatName, pageNumber, pageSize);

            var chatDto = ConvertToChatDto(chat);

            return chatDto;
        }

        private ChatDto ConvertToChatDto(Chat chat)
        {
            var chatDto = new ChatDto
            {
                Id = chat.ChatId,
                Name = chat.Name,
                Messages = chat.Messages?
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new MessageDto
                    {
                        MessageId = x.MessageId,
                        Sender = x.Sender.Username,
                        MessageText = x.MessageText,
                        CreatedAt = x.CreatedAt
                    })
                    .ToHashSet()
            };
            return chatDto;
        }
    }
}
