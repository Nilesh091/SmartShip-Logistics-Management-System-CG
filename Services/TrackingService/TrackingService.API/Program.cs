using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TrackingService.Application.Repositories.Interfaces;
using TrackingService.Application.Services.Interfaces;
using TrackingService.Application.Services.Implementations;
using TrackingService.Infrastructure.Data;
using TrackingService.Infrastructure.Storage;
using TrackingService.Infrastructure.Services;
using TrackingService.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<TrackingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI
builder.Services.AddScoped<ITrackingRepository, TrackingRepository>();
builder.Services.AddScoped<ITrackingService, TrackingService.Application.Services.Implementations.TrackingService>();

// File Storage & Document Services
builder.Services.AddSingleton<LocalFileStorageService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDeliveryProofService, DeliveryProofService>();

builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    // Define JWT Auth scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\n\nExample: Bearer abc123xyz"
    });

    // Apply JWT globally
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tracking service API",
        Version = "v1",
        Description = "API for managing tracking operations in the logistics system."
    });
});
builder.Services.AddHostedService<RabbitMqConsumer>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();