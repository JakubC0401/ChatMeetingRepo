
using ChatMeeting.Core.Domain;
using ChatMeeting.Core.Domain.Consts;
using ChatMeeting.Core.Domain.Dtos;
using ChatMeeting.Core.Domain.Models;
using ChatMeeting.Core.Domain.Options;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ChatMeeting.MessageBroker
{
    public class KafkaConsumer : BackgroundService
    {
        private readonly KafkaOption _kafkaOption;
        private readonly ILogger<KafkaConsumer> _logger;
        private readonly IDbContextFactory<ChatDbContext> _dbContextFactory;

        public KafkaConsumer(IDbContextFactory<ChatDbContext> dbContextFactory, ILogger<KafkaConsumer> logger,  IOptions<KafkaOption> options)
        {
            _kafkaOption = options.Value;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Consume(TopicKafka.Message, stoppingToken);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occured while consuming messages");
            }
        }

        private async Task Consume(string topic, CancellationToken stoppingToken)
        {
            var config = CreateConsumerConfig();
            using var consumer = new ConsumerBuilder<string,string>(config).Build();

            consumer.Subscribe(topic);
            _logger.LogInformation($"Subscribed to topic {topic}");

            while (!stoppingToken.IsCancellationRequested) 
            {
                try
                {
                    var cosumeResult = consumer.Consume(stoppingToken);

                    await ProcessMessage(cosumeResult.Message.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occured while processing the message{ex.Message}", ex);
                    await Task.Delay(1000);
                }
               
            }
        }

        private async Task ProcessMessage(string value)
        {
            var messageDto = JsonSerializer.Deserialize<MessageDto>(value);
            var message = CreateMessage(messageDto);

            await SaveMessageToDatabase(message);
        }

        private async Task SaveMessageToDatabase(Message message)
        {
            try
            {
                var dbContext = _dbContextFactory.CreateDbContext();
                await dbContext.Messages.AddAsync(message);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation($"Save message to database: {message}");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"An error occured while saving message: {message} to database");
                throw;
            }
        }

        private Message CreateMessage(MessageDto messageDto)
        {
            return new Message
            {
                MessageId = messageDto.MessageId,
                SenderId = messageDto.SenderId,
                CreatedAt = messageDto.CreatedAt,
                MessageText = messageDto.MessageText,
                ChatId = messageDto.ChatId
            };
        }

        private ConsumerConfig CreateConsumerConfig()
        {
            return new ConsumerConfig
            {
                GroupId = GroupKafka.Message,
                BootstrapServers = _kafkaOption.Url,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        }
    }
}
