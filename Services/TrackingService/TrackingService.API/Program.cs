using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TrackingService.Application.Repositories.Interfaces;
using TrackingService.Application.Services.Interfaces;
using TrackingService.Application.Services.Implementations;
using TrackingService.Infrastructure.Data;
using TrackingService.Infrastructure.Storage;
using TrackingService.Infrastructure.Services;
using Shared.Logs;
using Shared.Messaging;
using TrackingService.Application.EventHandlers;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load .env file at startup
DotNetEnv.Env.Load();

// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "2004@Nilu"; // fallback to .env value
connectionString = string.Format(connectionString, dbPassword);

builder.Services.AddDbContext<TrackingDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    }));

// Logging
builder.Host.ConfigureSerilog("TrackingService");

// DI
builder.Services.AddScoped<ITrackingRepository, TrackingRepository>();
builder.Services.AddScoped<ITrackingService, TrackingService.Application.Services.Implementations.TrackingService>();

// File Storage & Document Services
builder.Services.AddSingleton<LocalFileStorageService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDeliveryProofService, DeliveryProofService>();

builder.Services.AddControllers();

// JWT Authentication
builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-secret-key-here-change-in-production";
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AuthService";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AuthServiceAPI";

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtKey))
    };
});

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
builder.Services.AddScoped<IEventDispatcher, TrackingEventDispatcher>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();