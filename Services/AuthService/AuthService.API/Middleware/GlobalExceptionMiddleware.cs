using System.Net;
using System.Text.Json;
using AuthService.Application.Exceptions;

namespace AuthService.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AuthServiceException ex)
            {
                _logger.LogWarning(ex, "AuthService domain exception: {Message}", ex.Message);
                await WriteResponse(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (GoogleOAuthException ex)
            {
                _logger.LogWarning(ex, "Google OAuth exception: {Message}", ex.Message);
                await WriteResponse(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await WriteResponse(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            }
        }

        private static async Task WriteResponse(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
        }
    }
}
