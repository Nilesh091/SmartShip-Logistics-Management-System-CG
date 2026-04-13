using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using Shared.Logs;
var builder = WebApplication.CreateBuilder(args);

// Load Ocelot config
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// ======================
// 🔹 SERVICES
// ======================

// Ocelot
builder.Services.AddOcelot(builder.Configuration);
builder.Host.ConfigureSerilog("GatewayAPI");
// Swagger for Ocelot
builder.Services.AddSwaggerForOcelot(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = jwtSettings["Key"]
          ?? throw new Exception("JWT Key is missing");

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)
            )
        };
    });

builder.Services.AddAuthorization();

// ======================
// 🔹 PIPELINE
// ======================

var app = builder.Build();

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Debug (optional)
app.Use(async (context, next) =>
{
    Console.WriteLine($"AUTH HEADER: {context.Request.Headers["Authorization"]}");
    await next();
});

// 🔥 IMPORTANT: Swagger BEFORE Ocelot
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerForOcelotUI(opt =>
    {
        opt.PathToSwaggerGenerator = "/swagger/docs";
    });
}

// Ocelot LAST
await app.UseOcelot();

app.Run();