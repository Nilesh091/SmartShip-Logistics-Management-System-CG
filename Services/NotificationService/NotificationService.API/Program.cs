using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Messaging;
using Shared.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Email service
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// Dispatcher
builder.Services.AddScoped<IEventDispatcher, EventDispatcher>();

// Shared RabbitMQ Consumer
builder.Services.AddHostedService<RabbitMqConsumer>();

var app = builder.Build();

app.MapControllers();

app.Run();