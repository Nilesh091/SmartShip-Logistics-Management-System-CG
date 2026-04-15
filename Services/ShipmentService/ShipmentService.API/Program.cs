using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShipmentService.Application.Services;
using ShipmentService.Application.Repositories;
using ShipmentService.Infrastructure.Persistence;
using ShipmentService.Infrastructure.Repositories;
using System.Text;
using Shared.Messaging;
using Shared.Logs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(options =>
// {
//     // Add JWT security definition
//     options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
//         Name = "Authorization",
//         In = ParameterLocation.Header,
//         Type = SecuritySchemeType.Http,
//         Scheme = "Bearer",
//         BearerFormat = "JWT"
//     });

//     // Add API info
//     // options.SwaggerDoc("v1", new OpenApiInfo
//     // {
//     //     Title = "ShipmentService API",
//     //     Version = "v1.0",
//     //     Description = "API for managing shipments with JWT authentication",
//     //     Contact = new OpenApiContact
//     //     {
//     //         Name = "SmartShip Logistics",
//     //         Url = new Uri("https://smartship.com")
//     //     }
//     // });
// });
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
        Title = "Shipment service API",
        Version = "v1",
        Description = "API for managing shipment operations in the logistics system."
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
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    })
);

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-security");
var issuer = jwtSettings["Issuer"] ?? "AuthService";
var audience = jwtSettings["Audience"] ?? "AuthServiceAPI";
builder.Host.ConfigureSerilog("ShipmentService");
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

// Add Application Services
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IShipmentService, ShipmentService.Application.Services.ShipmentService>();

// Add Logging
builder.Services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    config.AddDebug();
});
builder.Services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();

var app = builder.Build();

// Initialize RabbitMQ publisher
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // app.UseSwaggerUI(options =>
    // {
    //     options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    //     options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    //     options.DefaultModelsExpandDepth(2);
    // });
    app.UseSwaggerUI();
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
