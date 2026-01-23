namespace Pulse.Models.Api
{
    /// <summary>
    /// Custom exception for Pulse API errors.
    /// Provides structured error information for better error handling in Blazor components.
    /// </summary>
    public class PulseApiException : Exception
    {
        public string Endpoint { get; set; } = string.Empty;
        public System.Net.HttpStatusCode? StatusCode { get; set; }
        public string? ResponseContent { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

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
            System.Net.HttpStatusCode? statusCode = null,
            string? responseContent = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            Endpoint = endpoint;
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }

        public override string ToString()
        {
            var details = new System.Text.StringBuilder(base.ToString());

            if (!string.IsNullOrEmpty(Endpoint))
                details.AppendLine($"\nEndpoint: {Endpoint}");

            if (StatusCode.HasValue)
                details.AppendLine($"Status Code: {(int)StatusCode} ({StatusCode})");

            if (!string.IsNullOrEmpty(ResponseContent))
                details.AppendLine($"Response: {ResponseContent[..Math.Min(500, ResponseContent.Length)]}");

            details.AppendLine($"Occurred At: {OccurredAt:O}");

            return details.ToString();
        }

        /// <summary>
        /// Determines if the error is retryable.
        /// </summary>
        public bool IsRetryable =>
            StatusCode switch
            {
                System.Net.HttpStatusCode.RequestTimeout => true,
                System.Net.HttpStatusCode.TooManyRequests => true,
                System.Net.HttpStatusCode.ServiceUnavailable => true,
                System.Net.HttpStatusCode.GatewayTimeout => true,
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
    }
}