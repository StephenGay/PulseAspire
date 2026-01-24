using System.Diagnostics;

namespace Pulse.ApiService.Middleware
{
    /// <summary>
    /// Logs all requests and responses with duration.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                stopwatch.Stop();

                // Only log non-health-check requests
                if (!context.Request.Path.StartsWithSegments("/health"))
                {
                    _logger.LogInformation(
                        "HTTP {Method} {Path} returned {StatusCode} in {Duration}ms",
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds);
                }

                // Reset stream position to beginning before copying
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }
}