namespace Pulse.Web.Models;

/// <summary>
/// Result type for SQL generation operations.
/// </summary>
public sealed record SqlGenerationResult(
    bool Success,
    string? Sql = null,
    string? Comments = null,
    string? ErrorMessage = null)
{
    public static SqlGenerationResult Ok(string sql, string? comments = null)
        => new(true, sql, comments);

    public static SqlGenerationResult Error(string message)
        => new(false, ErrorMessage: message);

    public static SqlGenerationResult Cancelled()
        => new(false, ErrorMessage: "Request was cancelled.");
}

/// <summary>
/// Per-request context to avoid shared instance state.
/// </summary>
public sealed class QueryContext
{
    public int ToolAttempts { get; set; }
    public bool CancelQuery { get; set; }
    public Dictionary<string, int> ToolAttemptCounts { get; } = new();

    public bool ShouldRetryTool(string toolName, int maxAttempts = 3)
    {
        ToolAttemptCounts.TryGetValue(toolName, out var count);
        ToolAttemptCounts[toolName] = count + 1;
        return count < maxAttempts;
    }
}