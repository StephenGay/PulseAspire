using System.Net;
using System.Text;

namespace Pulse.Models.Api
{
    /// <summary>
    /// Custom exception for Pulse API errors.
    /// Provides structured error information with Blazor-friendly properties.
    /// </summary>
    public class PulseApiException : Exception
    {
        public string Endpoint { get; set; } = string.Empty;
        public HttpStatusCode? StatusCode { get; set; }
        public string? ResponseContent { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User-friendly error message for display in Blazor UI.
        /// </summary>
        public string UserMessage { get; set; } = "An error occurred. Please try again.";

        /// <summary>
        /// Error category for routing to appropriate UI error handler.
        /// </summary>
        public ErrorCategory Category { get; set; } = ErrorCategory.Unknown;

        /// <summary>
        /// Validation errors keyed by field name (for form validation display).
        /// </summary>
        public Dictionary<string, string[]> FieldErrors { get; set; } = new();

        public PulseApiException(string message) : base(message)
        {
        }

        public PulseApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PulseApiException(
            string message,
            string endpoint,
            HttpStatusCode? statusCode = null,
            string? responseContent = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            Endpoint = endpoint;
            StatusCode = statusCode;
            ResponseContent = responseContent;

            // Auto-categorize and set user message
            CategorizeError();
        }

        /// <summary>
        /// Creates exception from ApiResponse error data.
        /// </summary>
        public static PulseApiException FromApiResponse(ApiResponse response, string endpoint)
        {
            var ex = new PulseApiException(
                response.Message ?? "API request failed",
                endpoint,
                statusCode: (HttpStatusCode?)(response.StatusCode ?? 500))
            {
                FieldErrors = response.Errors ?? new()
            };

            ex.CategorizeError();
            return ex;
        }

        /// <summary>
        /// Categorizes the error based on status code and content.
        /// </summary>
        private void CategorizeError()
        {
            if (!StatusCode.HasValue)
            {
                Category = ErrorCategory.Network;
                UserMessage = "Network connection failed. Please check your internet.";
                return;
            }

            Category = (int)StatusCode switch
            {
                400 => ErrorCategory.Validation,
                401 => ErrorCategory.Authentication,
                403 => ErrorCategory.Authorization,
                404 => ErrorCategory.NotFound,
                409 => ErrorCategory.Conflict,
                429 => ErrorCategory.RateLimited,
                >= 500 => ErrorCategory.Server,
                _ => ErrorCategory.Unknown
            };

            UserMessage = Category switch
            {
                ErrorCategory.Validation => "Please check your input and try again.",
                ErrorCategory.Authentication => "Your session has expired. Please log in again.",
                ErrorCategory.Authorization => "You don't have permission to perform this action.",
                ErrorCategory.NotFound => "The requested resource was not found.",
                ErrorCategory.Conflict => "This action conflicts with existing data.",
                ErrorCategory.RateLimited => "Too many requests. Please wait a moment and try again.",
                ErrorCategory.Server => "A server error occurred. Please try again later.",
                ErrorCategory.Network => "Network connection error. Please check your internet.",
                _ => "An unexpected error occurred. Please try again."
            };
        }

        /// <summary>
        /// Adds field validation error (for form validation UI).
        /// </summary>
        public void AddFieldError(string fieldName, string message)
        {
            if (!FieldErrors.ContainsKey(fieldName))
            {
                FieldErrors[fieldName] = new[] { message };
            }
            else
            {
                var existing = FieldErrors[fieldName].ToList();
                existing.Add(message);
                FieldErrors[fieldName] = existing.ToArray();
            }
        }

        /// <summary>
        /// Determines if the error is retryable by client.
        /// </summary>
        public bool IsRetryable =>
            StatusCode switch
            {
                HttpStatusCode.RequestTimeout => true,
                HttpStatusCode.TooManyRequests => true,
                HttpStatusCode.ServiceUnavailable => true,
                HttpStatusCode.GatewayTimeout => true,
                HttpStatusCode.BadGateway => true,
                _ => false
            };

        /// <summary>
        /// Determines if the error is a client fault (4xx).
        /// </summary>
        public bool IsClientError =>
            StatusCode.HasValue && (int)StatusCode >= 400 && (int)StatusCode < 500;

        /// <summary>
        /// Determines if the error is a server fault (5xx).
        /// </summary>
        public bool IsServerError =>
            StatusCode.HasValue && (int)StatusCode >= 500 && (int)StatusCode < 600;

        /// <summary>
        /// Determines if user authentication is required.
        /// </summary>
        public bool RequiresReauthentication =>
            Category == ErrorCategory.Authentication || StatusCode == HttpStatusCode.Unauthorized;

        /// <summary>
        /// Gets a detailed error description for logging.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());

            if (!string.IsNullOrEmpty(Endpoint))
                sb.AppendLine($"\nEndpoint: {Endpoint}");

            if (StatusCode.HasValue)
                sb.AppendLine($"Status Code: {(int)StatusCode} ({StatusCode})");

            sb.AppendLine($"Category: {Category}");
            sb.AppendLine($"User Message: {UserMessage}");

            if (FieldErrors.Any())
            {
                sb.AppendLine("Field Errors:");
                foreach (var (field, errors) in FieldErrors)
                {
                    sb.AppendLine($"  {field}: {string.Join(", ", errors)}");
                }
            }

            if (!string.IsNullOrEmpty(ResponseContent))
                sb.AppendLine($"Response: {ResponseContent[..Math.Min(500, ResponseContent.Length)]}");

            sb.AppendLine($"Occurred At: {OccurredAt:O}");

            return sb.ToString();
        }
    }

    /// <summary>
    /// Error categories for routing in Blazor error handlers.
    /// </summary>
    public enum ErrorCategory
    {
        /// <summary>Validation error (400 Bad Request)</summary>
        Validation,

        /// <summary>Authentication required (401 Unauthorized)</summary>
        Authentication,

        /// <summary>Authorization failed (403 Forbidden)</summary>
        Authorization,

        /// <summary>Resource not found (404 Not Found)</summary>
        NotFound,

        /// <summary>Conflict with existing data (409 Conflict)</summary>
        Conflict,

        /// <summary>Too many requests (429 Too Many Requests)</summary>
        RateLimited,

        /// <summary>Server error (5xx)</summary>
        Server,

        /// <summary>Network connection error</summary>
        Network,

        /// <summary>Unknown error</summary>
        Unknown
    }
}