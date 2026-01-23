using System.Text.Json.Serialization;

namespace Pulse.Models.Api
{
    /// <summary>
    /// Generic API response wrapper for standardized responses.
    /// Useful for Blazor components to handle loading/success/error states.
    /// </summary>
    public class ApiResponse<T> where T : class
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }

        [JsonPropertyName("message")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }

        [JsonPropertyName("errors")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string[]>? Errors { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ApiResponse()
        {
        }

        public ApiResponse(T data, string? message = null)
        {
            Success = true;
            Data = data;
            Message = message;
        }

        public ApiResponse(string message, Dictionary<string, string[]>? errors = null)
        {
            Success = false;
            Message = message;
            Errors = errors;
        }

        public static ApiResponse<T> SuccessResponse(T data, string? message = null) =>
            new(data, message);

        public static ApiResponse<T> ErrorResponse(string message, Dictionary<string, string[]>? errors = null) =>
            new(message, errors);
    }

    /// <summary>
    /// Non-generic API response wrapper.
    /// </summary>
    public class ApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }

        [JsonPropertyName("errors")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string[]>? Errors { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ApiResponse()
        {
        }

        public ApiResponse(bool success, string? message = null, Dictionary<string, string[]>? errors = null)
        {
            Success = success;
            Message = message;
            Errors = errors;
        }

        public static ApiResponse SuccessResponse(string? message = null) =>
            new(true, message);

        public static ApiResponse ErrorResponse(string message, Dictionary<string, string[]>? errors = null) =>
            new(false, message, errors);
    }
}