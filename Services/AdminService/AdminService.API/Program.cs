using AdminService.Application.Interfaces;
using AdminService.Infrastructure.Clients;
using AdminService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Shared.Logs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };
    });

builder.Services.AddAuthorization();
builder.Host.ConfigureSerilog("AdminService");

// HttpClient
var shipmentServiceUrl = builder.Configuration["Services:ShipmentService"];
var authServiceUrl = builder.Configuration["Services:AuthService"];

builder.Services.AddHttpClient<ShipmentClient>(client =>
{
    client.BaseAddress = new Uri(shipmentServiceUrl);
});

builder.Services.AddHttpClient<AuthClient>(client =>
{
    client.BaseAddress = new Uri(authServiceUrl);
});

// DI
builder.Services.AddScoped<IAdminService, AdminServicee>();
// builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
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
        Title = "Admin service API",
        Version = "v1",
        Description = "API for managing admin operations in the e-commerce system."
    });

}
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();