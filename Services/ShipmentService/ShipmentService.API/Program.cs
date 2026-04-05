using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShipmentService.Application.Services;
using ShipmentService.Application.Repositories;
using ShipmentService.Application.MessagePublishing;
using ShipmentService.Infrastructure.Persistence;
using ShipmentService.Infrastructure.Repositories;
using ShipmentService.Infrastructure.MessagePublishing;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add JWT security definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    // Add security requirement to all operations
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

    // Add API info
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ShipmentService API",
        Version = "v1.0",
        Description = "API for managing shipments with JWT authentication",
        Contact = new OpenApiContact
        {
            Name = "SmartShip Logistics",
            Url = new Uri("https://smartship.com")
        }
    });
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=.;Database=ShipmentServiceDb;Integrated Security=true;TrustServerCertificate=true;";

builder.Services.AddDbContext<ShipmentDbContext>(options =>
    options.UseSqlServer(connectionString)
);

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-security");
var issuer = jwtSettings["Issuer"] ?? "AuthService";
var audience = jwtSettings["Audience"] ?? "AuthServiceAPI";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add Message Publishing & Consuming configuration
var rabbitMQHost = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
var queueName = builder.Configuration["RabbitMQ:QueueName"] ?? "shipment-status-changed-event-events";

// Add Application Services
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();

// Add Message Polling for GET requests
builder.Services.AddScoped<IMessagePoller>(sp =>
    new RabbitMQMessagePoller(
        rabbitMQHost,
        queueName,
        sp.GetRequiredService<ILoggerFactory>().CreateLogger<RabbitMQMessagePoller>()
    )
);

builder.Services.AddScoped<IShipmentService, ShipmentService.Application.Services.ShipmentService>();

// Add Message Publishing
builder.Services.AddScoped<IMessagePublisher>(sp =>
    new MessagePublisher(rabbitMQHost, sp.GetRequiredService<ILogger<MessagePublisher>>())
);

// Add Message Consuming (Background Service)
builder.Services.AddScoped<IMessageConsumer>(sp =>
    new MessageConsumer(
        rabbitMQHost,
        queueName,
        sp.GetRequiredService<IShipmentRepository>(),
        sp.GetRequiredService<ILoggerFactory>().CreateLogger<MessageConsumer>()
    )
);

builder.Services.AddHostedService<ShipmentService.API.BackgroundServices.ShipmentStatusConsumerBackgroundService>();

// Add Logging
builder.Services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ShipmentService API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.DefaultModelsExpandDepth(2);
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ShipmentDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
