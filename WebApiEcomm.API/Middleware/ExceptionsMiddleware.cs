using Microsoft.Extensions.Hosting;
using System.Net;
using System.Text.Json;
using WebApiEcomm.API.Helper;

namespace WebApiEcomm.API.Middleware
{
    public class ExceptionsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<ExceptionsMiddleware> _logger;

        public ExceptionsMiddleware(
            RequestDelegate next,
            IHostEnvironment hostEnvironment,
            ILogger<ExceptionsMiddleware> logger)
        {
            _next = next;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            ApplySecurityHeaders(context);

            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var response = _hostEnvironment.IsDevelopment()
                    ? new ApiExceptions((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace ?? string.Empty)
                    : new ApiExceptions(
                        (int)HttpStatusCode.InternalServerError,
                        "An error occurred while processing your request.");

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json).ConfigureAwait(false);
            }
        }

        private static void ApplySecurityHeaders(HttpContext context)
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "no-referrer");
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
        }
    }
}
