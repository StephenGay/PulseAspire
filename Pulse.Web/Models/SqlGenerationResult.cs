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
/// Per-request context to maintain conversation state across tool calls.
/// Stores the original user query to preserve context throughout recursive tool execution.
/// </summary>
public sealed class QueryContext
{
    /// <summary>
    /// The original user query that started this conversation turn.
    /// Used to provide context in all tool results.
    /// </summary>
    public string OriginalUserQuery { get; set; } = string.Empty;

    /// <summary>
    /// Total number of tool execution attempts across all tools.
    /// </summary>
    public int ToolAttempts { get; set; }

    /// <summary>
    /// Flag to cancel the current query processing.
    /// </summary>
    public bool CancelQuery { get; set; }

    /// <summary>
    /// Tracks individual tool retry counts to prevent infinite loops.
    /// Key: tool name, Value: number of attempts.
    /// </summary>
    public Dictionary<string, int> ToolAttemptCounts { get; } = new();

    /// <summary>
    /// Determines if a tool should be retried based on attempt count.
    /// </summary>
    /// <param name="toolName">Name of the tool to check.</param>
    /// <param name="maxAttempts">Maximum allowed attempts (default 3).</param>
    /// <returns>True if the tool can be retried; false if max attempts reached.</returns>
    public bool ShouldRetryTool(string toolName, int maxAttempts = 10)
    {
        ToolAttemptCounts.TryGetValue(toolName, out var count);
        ToolAttemptCounts[toolName] = count + 1;
        return count < maxAttempts;
    }
}