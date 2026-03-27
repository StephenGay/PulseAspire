using System.Text.Json.Serialization;

namespace Pulse.Models.Api;

/// <summary>
/// Response wrapper for AI-generated SQL query execution with detailed feedback.
/// </summary>
public class AiQueryResponse<T> where T : class
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    [JsonPropertyName("validationErrors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<AiQueryValidationError>? ValidationErrors { get; set; }

    [JsonPropertyName("executionDetails")]
    public AiQueryExecutionDetails ExecutionDetails { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static AiQueryResponse<T> SuccessResponse(T data, string? message = null, int rowCount = 0, double executionTimeMs = 0)
    {
        return new AiQueryResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Query executed successfully",
            ExecutionDetails = new AiQueryExecutionDetails
            {
                RowCount = rowCount,
                ExecutionTimeMs = executionTimeMs,
                WasValidated = true
            }
        };
    }

    public static AiQueryResponse<T> ValidationErrorResponse(List<AiQueryValidationError> errors)
    {
        return new AiQueryResponse<T>
        {
            Success = false,
            Message = "Query validation failed. Please review the errors and regenerate the query.",
            ValidationErrors = errors,
            ExecutionDetails = new AiQueryExecutionDetails
            {
                WasValidated = true,
                ValidationPassed = false
            }
        };
    }

    public static AiQueryResponse<T> ExecutionErrorResponse(string message, Exception? exception = null)
    {
        return new AiQueryResponse<T>
        {
            Success = false,
            Message = message,
            ValidationErrors = exception != null ? new List<AiQueryValidationError>
            {
                new AiQueryValidationError
                {
                    ErrorType = "ExecutionError",
                    Message = exception.Message,
                    Suggestion = "Check the SQL syntax and ensure all referenced objects exist in the database."
                }
            } : null,
            ExecutionDetails = new AiQueryExecutionDetails
            {
                WasValidated = true,
                ValidationPassed = true,
                ExecutionFailed = true
            }
        };
    }
}

public class AiQueryValidationError
{
    [JsonPropertyName("errorType")]
    public string ErrorType { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("objectName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ObjectName { get; set; }

    [JsonPropertyName("suggestion")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Suggestion { get; set; }

    [JsonPropertyName("availableAlternatives")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? AvailableAlternatives { get; set; }
}

public class AiQueryExecutionDetails
{
    [JsonPropertyName("rowCount")]
    public int RowCount { get; set; }

    [JsonPropertyName("executionTimeMs")]
    public double ExecutionTimeMs { get; set; }

    [JsonPropertyName("wasValidated")]
    public bool WasValidated { get; set; }

    [JsonPropertyName("validationPassed")]
    public bool ValidationPassed { get; set; } = true;

    [JsonPropertyName("executionFailed")]
    public bool ExecutionFailed { get; set; }

    [JsonPropertyName("queryType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? QueryType { get; set; }
}

public record AiQueryRequest(string Query, int? Timeout = 30);