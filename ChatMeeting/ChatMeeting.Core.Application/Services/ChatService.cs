using ChatMeeting.Core.Domain.Consts;
using ChatMeeting.Core.Domain.Dtos;
using ChatMeeting.Core.Domain.Interfaces.Producer;
using ChatMeeting.Core.Domain.Interfaces.Repositories;
using ChatMeeting.Core.Domain.Interfaces.Services;
using ChatMeeting.Core.Domain.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatMeeting.Core.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<ChatService> _logger;
        private readonly IKafkaProducer _kafkaProducer;
        public ChatService(IChatRepository chatRepository, ILogger<ChatService> logger, IKafkaProducer kafkaProducer)
        {
            _chatRepository = chatRepository;
            _logger = logger;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<ChatDto> GetPaginatedChat(string chatName, int pageNumber, int pageSize)
        {
            var chat = await _chatRepository.GetChatWithMessages(chatName, pageNumber, pageSize);

            var chatDto = ConvertToChatDto(chat);

            return chatDto;
        }

        public async Task SaveMessage(MessageDto messageDto)
        {
            await _kafkaProducer.Produce(TopicKafka.Message, new Message<string, string>
            {
                Key = messageDto.MessageId.ToString(),
                Value = JsonSerializer.Serialize(messageDto)
            });
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
