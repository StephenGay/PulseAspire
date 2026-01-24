using Pulse.Models.Api;
using System.Net;
using System.Text.Json;

namespace Pulse.ApiService.Middleware
{
    /// <summary>
    /// Global exception handling middleware.
    /// Converts all exceptions to consistent ApiResponse format.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                ArgumentException => new ApiResponse
                {
                    Success = false,
                    Message = exception.Message,
                    StatusCode = (int)HttpStatusCode.BadRequest
                },
                KeyNotFoundException => new ApiResponse
                {
                    Success = false,
                    Message = "Resource not found",
                    StatusCode = (int)HttpStatusCode.NotFound
                },
                UnauthorizedAccessException => new ApiResponse
                {
                    Success = false,
                    Message = "Unauthorized",
                    StatusCode = (int)HttpStatusCode.Unauthorized
                },
                _ => new ApiResponse
                {
                    Success = false,
                    Message = "Internal server error",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                }
            };

            context.Response.StatusCode = (int)response.StatusCode;
            return context.Response.WriteAsJsonAsync(response, _jsonOptions);
        }
    }
}