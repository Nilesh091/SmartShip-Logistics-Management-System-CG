using System.Net;
using System.Text.Json;

namespace TrackingService.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument exception: {Message}", ex.Message);
                await WriteResponse(context, HttpStatusCode.BadRequest, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

                object body = _env.IsDevelopment()
                    ? new { message = "An unexpected error occurred.", error = ex.Message, innerError = ex.InnerException?.Message }
                    : new { message = "An unexpected error occurred." };

                await WriteResponse(context, HttpStatusCode.InternalServerError, body);
            }
        }

        private static async Task WriteResponse(HttpContext context, HttpStatusCode statusCode, object body)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(body));
        }
    }
}
