using ChatMeeting.Core.Domain;
using ChatMeeting.Core.Domain.Options;
using ChatMeeting.MessageBroker;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.Configure<KafkaOption>(options => configuration.GetSection(nameof(KafkaOption)).Bind(options));

var connectionString = configuration.GetValue<string>("ConnectionString");



builder.Services.AddHostedService<KafkaConsumer>();



builder.Services.AddDbContextFactory<ChatDbContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();
app.Run();
