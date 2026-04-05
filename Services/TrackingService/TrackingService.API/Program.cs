using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using TrackingService.Application.Repositories.Interfaces;
using TrackingService.Application.Services.Interfaces;
using TrackingService.Application.Services.Implementations;
using TrackingService.Infrastructure.Data;
using TrackingService.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<TrackingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// RabbitMQ
var rabbitMqHostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
builder.Services.AddSingleton<IConnectionFactory>(new ConnectionFactory
{
    HostName = rabbitMqHostName
});

// DI
builder.Services.AddScoped<ITrackingRepository, TrackingRepository>();
builder.Services.AddScoped<ITrackingService, TrackingService.Application.Services.Implementations.TrackingService>();
builder.Services.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();
builder.Services.AddSingleton<RabbitMQConsumer>();

builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Tracking Service API",
        Version = "v1",
        Description = "API for managing shipment tracking and events"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tracking Service API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


// 🔥 START CONSUMER (IMPROVED LOGGING)
var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    var consumer = app.Services.GetRequiredService<RabbitMQConsumer>();

    logger.LogInformation("Starting RabbitMQ Consumer...");

    await consumer.StartAsync(builder.Configuration["RabbitMQ:HostName"] ?? "localhost");

    logger.LogInformation("RabbitMQ Consumer started successfully ✅");
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to start RabbitMQ Consumer ❌");
}

app.Run();