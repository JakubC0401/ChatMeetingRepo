var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ChatMeeting_API>("apiservice-chat");
builder.AddProject<Projects.ChatMeeting_MessageBroker>("apiservice-message");

builder.Build().Run();
