namespace Pulse.Web.Tools;

/// <summary>
/// Shared utility for extracting SQL from LLM responses.
/// </summary>
public static class SqlExtractor
{
    public static string? Extract(string? response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return null;

        var trimmed = response.Trim();

        // Try [SQL Start]/[SQL End] markers
        if (TryExtractBetween(trimmed, "[SQL Start]", "[SQL End]", out var sql))
            return sql;

        // Try markdown ```sql blocks
        if (TryExtractBetween(trimmed, "```sql", "```", out sql))
            return sql;

        // Try generic code block
        if (TryExtractBetween(trimmed, "```", "```", out sql) &&
            sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            return sql;

        // Fallback: Extract SELECT statement
        var selectIdx = trimmed.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
        if (selectIdx >= 0)
        {
            var endIdx = trimmed.IndexOf(';', selectIdx);
            endIdx = endIdx < 0 ? trimmed.Length : endIdx + 1;
            return trimmed[selectIdx..endIdx].Trim();
        }

        return null;
    }

    public static string ExtractComments(string? response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return string.Empty;

        var sqlStart = response.IndexOf("```sql", StringComparison.OrdinalIgnoreCase);
        if (sqlStart < 0)
            sqlStart = response.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);

        if (sqlStart <= 0)
            return response.Trim();

        var comments = response[..sqlStart].Trim();
        return comments.EndsWith("```sql", StringComparison.OrdinalIgnoreCase)
            ? comments[..^6].Trim()
            : comments;
    }

    private static bool TryExtractBetween(string text, string start, string end, out string? result)
    {
        result = null;
        var startIdx = text.IndexOf(start, StringComparison.OrdinalIgnoreCase);
        if (startIdx < 0) return false;

        startIdx += start.Length;
        var endIdx = text.IndexOf(end, startIdx, StringComparison.Ordinal);
        if (endIdx <= startIdx) return false;

        result = text[startIdx..endIdx].Trim();
        return !string.IsNullOrEmpty(result);
    }
}