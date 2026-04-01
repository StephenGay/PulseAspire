using Microsoft.Extensions.Caching.Memory;
using Pulse.ApiService.PulseAI.Services;
using Pulse.Models.AI.AIds;
using Pulse.Models.CustomComponents;
using Pulse.Models.Customers;
using Pulse.Models.Misc;
using Pulse.Web.Models;
using Pulse.Web.Tools;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pulse.Web.Services;

/// <summary>
/// AI service for natural language to SQL conversion and chat interactions.
/// Thread-safe: uses request-scoped context instead of instance state.
/// </summary>
public sealed class Pulse_AI(
    IHttpClientFactory httpClientFactory,
    OllamaService ollamaService,
    ILogger<Pulse_AI> logger,
    IConfiguration configuration,
    IMemoryCache cache)
{
    #region Constants

    private static class CacheKeys
    {
        public const string Schema = "pulse_ai_schema";
        public const string Examples = "pulse_ai_examples";
    }

    private static class ToolNames
    {
        public const string ExecuteSql = "execute_sql";
        public const string WebSearch = "web_search";
        public const string WebFetch = "web_fetch";
        public const string ExecuteActionSql = "execute_action_sql";

        public static readonly HashSet<string> All = [ExecuteSql, WebSearch, WebFetch, ExecuteActionSql];
    }

    private static class ApiRoutes
    {
        public const string Schema = "/AI/schema";
        public const string Examples = "/AI/examples";
        public const string Execute = "/AI/execute:";
        public const string Chat = "/api/chat";       // ✅ Ensure this exists
        public const string Generate = "/api/generate";
        public const string Tags = "/api/tags";
    }

    private const int MaxTotalQueryAttempts = 10;
    private const int MaxToolRetries = 6;
    private static readonly TimeSpan SchemaCacheDuration = TimeSpan.FromMinutes(120);
    private static readonly TimeSpan ExamplesCacheDuration = TimeSpan.FromMinutes(60);

    #endregion

    #region Fields

    private readonly HttpClient _pulseApiClient = httpClientFactory.CreateClient("PulseApiClient");
    private readonly OllamaService _ollamaService = ollamaService;
    private readonly ILogger<Pulse_AI> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IMemoryCache _cache = cache;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    #endregion

    #region SQL Validation

    /// <summary>
    /// Validates that SQL is SQL Server T-SQL and rejects SQLite/MySQL/PostgreSQL syntax.
    /// </summary>
    private static string? ValidateSqlServerSyntax(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return null;

        var sqlLower = sql.ToLower();

        // ❌ FORBIDDEN: SQLite syntax
        if (sqlLower.Contains("sqlite_master") || sqlLower.Contains("sqlite_temp_master"))
            return "❌ ERROR: SQLite syntax detected (sqlite_master). This is a SQL Server 2022 database. Use SQL Server syntax only.";

        if (sqlLower.Contains("pragma") && sqlLower.Contains("table_info"))
            return "❌ ERROR: SQLite PRAGMA syntax detected. This is SQL Server 2022 - use sys.columns or sp_help instead.";

        if (sqlLower.Contains("autoincrement"))
            return "❌ ERROR: SQLite AUTOINCREMENT detected. Use SQL Server IDENTITY or SEQUENCE instead.";

        if (sqlLower.Contains(" rowid"))
            return "❌ ERROR: SQLite ROWID detected. Use SQL Server row_number() or identity columns instead.";

        // ❌ FORBIDDEN: MySQL syntax
        if (sqlLower.Contains("auto_increment") && !sqlLower.Contains("identity"))
            return "❌ ERROR: MySQL AUTO_INCREMENT syntax detected. Use SQL Server IDENTITY instead.";

        if (sqlLower.Contains("`") && sqlLower.Contains("`"))
            return "❌ ERROR: MySQL backtick identifiers detected. Use SQL Server [brackets] instead.";

        // ❌ FORBIDDEN: PostgreSQL syntax
        if (sqlLower.Contains("::") && (sqlLower.Contains("::text") || sqlLower.Contains("::int")))
            return "❌ ERROR: PostgreSQL type casting detected. Use SQL Server CAST() instead.";

        if (sqlLower.Contains("serial") && !sqlLower.Contains("serializable"))
            return "❌ ERROR: PostgreSQL SERIAL data type detected. Use SQL Server IDENTITY or BIGINT instead.";

        return null; // Valid SQL Server syntax
    }

    #endregion

    #region SQL Generation

    /// <summary>
    /// Generates SQL from a natural language query.
    /// </summary>
    public async Task<SqlGenerationResult> GetSQLFromOllamaAsync(
        string userQuery,
        string model,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userQuery);
        ArgumentException.ThrowIfNullOrEmpty(model);

        try
        {
            var schemaText = await GetDetailedSchemaAsync(ct);
            if (string.IsNullOrEmpty(schemaText))
            {
                return SqlGenerationResult.Error("Unable to retrieve database schema.");
            }

            var examples = await GetAiExamplesAsync(ct);
            var prompt = BuildSqlPrompt(schemaText, examples, userQuery);

            var ollamaRequest = new
            {
                model,
                prompt,
                stream = false,
                options = new { temperature = 0.0, num_predict = 8000 }
            };

            var ollamaResponse = await _ollamaService.PostAsync<OllamaGenerateResponse>(
                ApiRoutes.Generate, ollamaRequest, ct);

            if (ollamaResponse?.Done != true || string.IsNullOrEmpty(ollamaResponse.Response))
            {
                return SqlGenerationResult.Error("Ollama generation incomplete.");
            }

            var sql = SqlExtractor.Extract(ollamaResponse.Response);
            if (string.IsNullOrEmpty(sql))
            {
                return SqlGenerationResult.Error("No SQL statement found in response.");
            }

            // 🚨 Validate SQL Server syntax
            var syntaxError = ValidateSqlServerSyntax(sql);
            if (syntaxError != null)
            {
                _logger.LogError("SQLite/MySQL/PostgreSQL syntax detected: {Query}", sql[..Math.Min(100, sql.Length)]);
                return SqlGenerationResult.Error(syntaxError);
            }

            _logger.LogInformation(
                "SQL generated successfully with {Tokens} tokens for query: {Query}",
                ollamaResponse.PromptEvalCount,
                userQuery[..Math.Min(50, userQuery.Length)]);

            return SqlGenerationResult.Ok(sql, SqlExtractor.ExtractComments(ollamaResponse.Response));
        }
        catch (OperationCanceledException)
        {
            return SqlGenerationResult.Cancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL for query: {Query}", userQuery);
            return SqlGenerationResult.Error($"Error: {ex.Message}");
        }
    }

    private static string BuildSqlPrompt(string schema, string examples, string userQuery) =>
        $$"""
        You are a SQL expert for SQL Server 2022.
        - Generate valid, efficient SELECT-only queries based on schema and user question.
        - Use exact table/column names from the schema.
        - Respect relationships for joins; handle nullability and data types.
        - Format DateTime as "dd-MM-yy".
        - Optimize with WHERE filters and JOINs only if needed.
        
        Schema:
        {{schema}}
        {{examples}}
        
        User Question: {{userQuery}}
        """;
    #endregion

    #region Simple Generation

    /// <summary>
    /// Simple prompt-response generation without tools.
    /// </summary>
    public async Task<string> AskPulseAIAsync(
        string prompt,
        string model="gpt-oss:latest",
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(prompt);
        ArgumentException.ThrowIfNullOrEmpty(model);

        try
        {
            var request = new
            {
                model,
                prompt,
                stream = false
            };

            var response = await _ollamaService.PostAsync<OllamaGenerateResponse>(
                ApiRoutes.Generate, request, ct);

            if (response is not { Done: true, Response.Length: > 0 })
            {
                _logger.LogWarning("Empty response from Ollama for prompt: {Prompt}",
                    prompt[..Math.Min(50, prompt.Length)]);
                return "No response from Ollama.";
            }

            _logger.LogInformation("Generated response with {Tokens} tokens", response.EvalCount);
            return response.Response.Trim();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request cancelled");
            return "Request was cancelled.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AskPulseAIAsync");
            return $"Error: {ex.Message}";
        }
    }

    #endregion

    #region Chat with Tools

    /// <summary>
    /// Non-streaming chat with tool support.
    /// </summary>
    public async Task<string> ChatWithOllamaAsync(
        List<OllamaMessage> messages,
        string model,
        bool enableSearch = true,
        string? userQry = null,
        CancellationToken ct = default,
        QueryContext? context = null,
        int queryLoop = 0)
    {
        context ??= new QueryContext();

        if (context.CancelQuery)
        {
            return "Query cancelled.";
        }

        if (queryLoop >= MaxTotalQueryAttempts)
        {
            return "Error: Query exceeded maximum loop attempts.";
        }

        try
        {
            var response = await ExecuteChatAsync(messages, model, enableSearch, ct);

            if (response is { Message.ToolCalls.Count: > 0 })
            {
                await HandleToolCallsAsync(response.Message.ToolCalls, messages, context, ct);
                return await ChatWithOllamaAsync(messages, model, enableSearch, userQry, ct, context, queryLoop + 1);
            }

            return response?.Message?.Content?.Trim() ?? "No response.";
        }
        catch (OperationCanceledException)
        {
            return "Request cancelled.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat error at loop {Loop}", queryLoop);
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Streaming chat with tool support.
    /// </summary>
    public async IAsyncEnumerable<string> StreamChatWithOllamaAsync(
        List<OllamaMessage> messages,
        string model,
        bool enableSearch = true,
        [EnumeratorCancellation] CancellationToken ct = default,
        QueryContext? context = null,
        int queryLoop = 0)
    {
        context ??= new QueryContext();

        if (context.CancelQuery)
        {
            yield return "Query cancelled.";
            yield break;
        }

        if (queryLoop >= MaxTotalQueryAttempts)
        {
            yield return "Error: Maximum loop attempts exceeded.";
            yield break;
        }

        // Prepare context outside the streaming loop
        var prepResult = await PrepareStreamContextAsync(ct);
        if (prepResult.Error != null)
        {
            yield return prepResult.Error;
            yield break;
        }

        var systemMessage = BuildSystemMessage(prepResult.Schema!, prepResult.Examples!, messages);
        var allMessages = new List<OllamaMessage> { systemMessage };
        allMessages.AddRange(messages);

        var tools = BuildToolsList(enableSearch);
        var request = new
        {
            model,
            messages = allMessages.ToArray(),
            tools = tools.ToArray(),
            think = "high",
            keep_alive = "30m",
            stream = true,
            options = new { temperature = 0.0, num_ctx = 32000 }
        };

        _logger.LogInformation("Starting chat stream (loop {Loop})", queryLoop);

        // Get the stream wrapper (handles initialization errors)
        var streamResult = await CreateStreamAsync(request, ct);
        if (streamResult.Error != null)
        {
            yield return streamResult.Error;
            yield break;
        }

        var accumulatedToolCalls = new List<OllamaToolCall>();
        var inThinking = false;
        var hasContent = false;

        // Iterate through chunks
        await foreach (var chunk in streamResult.Stream!)
        {
            if (chunk.Error != null)
            {
                yield return chunk.Error;
                yield break;
            }

            if (!string.IsNullOrEmpty(chunk.Thinking))
            {
                if (!inThinking)
                {
                    inThinking = true;
                    yield return "__THINKING__**Thinking:**\n";
                }
                yield return $"__THINKING__{chunk.Thinking}";
                hasContent = true;
            }

            if (!string.IsNullOrEmpty(chunk.Content))
            {
                if (inThinking)
                {
                    inThinking = false;
                    yield return "__FINAL_ANSWER__**Response:**\n";
                }
                yield return $"__FINAL_ANSWER__{chunk.Content}";
                hasContent = true;
            }

            if (chunk.ToolCalls is { Count: > 0 })
            {
                accumulatedToolCalls.AddRange(chunk.ToolCalls);
            }

            if (chunk.Done)
            {
                break;
            }
        }

        // Handle tool calls after streaming completes
        if (accumulatedToolCalls.Count > 0)
        {
            yield return "\n*Processing tool calls...*\n";

            var toolError = await HandleToolCallsSafeAsync(accumulatedToolCalls, messages, context, ct);
            if (toolError != null)
            {
                yield return toolError;
                yield break;
            }

            // Recursive streaming for tool results
            await foreach (var recursiveChunk in StreamChatWithOllamaAsync(
                messages, model, enableSearch, ct, context, queryLoop + 1))
            {
                yield return recursiveChunk;
            }
            yield break;
        }

        if (!hasContent)
        {
            yield return "No response from Ollama.";
        }
    }

    /// <summary>
    /// Prepares schema and examples for streaming.
    /// </summary>
    public async Task<(string? Schema, string? Examples, string? Error)> PrepareStreamContextAsync(
        CancellationToken ct)
    {
        try
        {
            var schemaText = await GetDetailedSchemaAsync(ct);
            if (string.IsNullOrEmpty(schemaText))
            {
                return (null, null, "Error: Schema unavailable");
            }

            var examples = await GetAiExamplesAsync(ct);
            return (schemaText, examples, null);
        }
        catch (OperationCanceledException)
        {
            return (null, null, "Request cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prepare stream context");
            return (null, null, $"Error loading context: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates the stream with error handling during initialization.
    /// Returns either the stream or an error message.
    /// </summary>
    private async Task<(IAsyncEnumerable<StreamChunk>? Stream, string? Error)> CreateStreamAsync(
        object request,
        CancellationToken ct)
    {
        try
        {
            // Test that we can start the stream
            var stream = StreamChunksCore(request, ct);
            return (stream, null);
        }
        catch (OperationCanceledException)
        {
            return (null, "Request cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create chat stream");
            return (null, $"Stream error: {ex.Message}");
        }
    }

    /// <summary>
    /// Core streaming logic - wraps exceptions into StreamChunk.Error.
    /// No yield inside catch blocks.
    /// </summary>
    private async IAsyncEnumerable<StreamChunk> StreamChunksCore(
        object request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var enumerator = _ollamaService.ChatStreamAsync(ApiRoutes.Chat, request, ct).GetAsyncEnumerator(ct);

        string? pendingError = null;

        try
        {
            while (true)
            {
                bool hasNext;
                OllamaChatChunk? current = null;

                try
                {
                    hasNext = await enumerator.MoveNextAsync();
                    if (hasNext)
                    {
                        current = enumerator.Current;
                    }
                }
                catch (OperationCanceledException)
                {
                    pendingError = "Request cancelled.";
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Stream iteration error");
                    pendingError = $"Stream error: {ex.Message}";
                    break;
                }

                if (!hasNext || current == null)
                {
                    break;
                }

                yield return new StreamChunk
                {
                    Thinking = current.Message?.Thinking,
                    Content = current.Message?.Content,
                    ToolCalls = current.Message?.ToolCalls,
                    Done = current.Done
                };

                if (current.Done)
                {
                    break;
                }
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        // Yield error outside of catch block
        if (pendingError != null)
        {
            yield return new StreamChunk { Error = pendingError };
        }
    }

    /// <summary>
    /// Handles tool calls with exception handling.
    /// </summary>
    private async Task<string?> HandleToolCallsSafeAsync(
        List<OllamaToolCall> toolCalls,
        List<OllamaMessage> messages,
        QueryContext context,
        CancellationToken ct)
    {
        try
        {
            await HandleToolCallsAsync(toolCalls, messages, context, ct);
            return null;
        }
        catch (OperationCanceledException)
        {
            return "Request cancelled.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool handling failed");
            return $"Tool error: {ex.Message}";
        }
    }

    /// <summary>
    /// Internal chunk representation for streaming.
    /// </summary>
    private sealed class StreamChunk
    {
        public string? Thinking { get; init; }
        public string? Content { get; init; }
        public List<OllamaToolCall>? ToolCalls { get; init; }
        public bool Done { get; init; }
        public string? Error { get; init; }
    }

    #endregion

    #region Tool Handling

    public async Task HandleToolCallsAsync(
        List<OllamaToolCall> toolCalls,
        List<OllamaMessage> messages,
        QueryContext context,
        CancellationToken ct)
    {
        foreach (var toolCall in toolCalls)
        {
            var toolName = NormalizeToolName(toolCall.Function?.Name);

            if (!ToolNames.All.Contains(toolName))
            {
                _logger.LogWarning("Unknown tool requested: {Tool}", toolName);
                messages.Add(new OllamaMessage
                {
                    Role = "tool",
                    Content = $"Invalid tool '{toolName}'. Available: {string.Join(", ", ToolNames.All)}",
                    ToolName = toolName
                });
                continue;
            }

            if (!context.ShouldRetryTool(toolName, MaxToolRetries))
            {
                _logger.LogWarning("Tool {Tool} exceeded max retries", toolName);
                messages.Add(new OllamaMessage
                {
                    Role = "tool",
                    Content = $"Tool '{toolName}' exceeded maximum retry attempts for query: {context.OriginalUserQuery}",
                    ToolName = toolName
                });
                continue;
            }

            var toolResult = await ExecuteToolAsync(toolName, toolCall.Function?.Arguments ?? default, context, ct);
            messages.Add(new OllamaMessage
            {
                Role = "tool",
                Content = toolResult,
                ToolName = toolName
            });
        }
    }

    public static string NormalizeToolName(string? toolName)
    {
        if (string.IsNullOrEmpty(toolName))
            return "unknown";

        var normalized = toolName.ToLowerInvariant().Trim();
        return ToolNames.All.FirstOrDefault(t => normalized.Contains(t)) ?? normalized;
    }

    public async Task<string> ExecuteToolAsync(string toolName, JsonElement args, QueryContext context, CancellationToken ct)
    {
        try
        {
            return toolName switch
            {
                ToolNames.ExecuteSql => await ExecuteSqlAsync(args, context, ct),
                ToolNames.WebSearch => await WebSearchAsync(args, context, ct),
                ToolNames.WebFetch => await WebFetchAsync(args, context, ct),
                ToolNames.ExecuteActionSql => await ExecuteActionSqlToolAsync(args, context, ct),
                _ => $"Unknown tool: {toolName}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool {Tool} failed", toolName);
            return $"Tool error: {ex.Message}";
        }
    }

    //public async Task<string> ExecuteToolAsync(string toolName, JsonElement args, CancellationToken ct)
    //{
    //    try
    //    {
    //        return toolName switch
    //        {
    //            ToolNames.ExecuteSql => await ExecuteSqlAsync(args, ct),
    //            ToolNames.WebSearch => await WebSearchAsync(args, ct),
    //            ToolNames.WebFetch => await WebFetchAsync(args, ct),
    //            ToolNames.ExecuteActionSql => await ExecuteActionSqlToolAsync(args, ct),
    //            _ => $"Unknown tool: {toolName}"
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Tool {Tool} failed", toolName);
    //        return $"Tool error: {ex.Message}";
    //    }
    //}

    private async Task<string> ExecuteSqlAsync(JsonElement args, QueryContext context, CancellationToken ct)
    {
        if (!args.TryGetProperty("sql", out var sqlProp))
            return "Error: Missing 'sql' argument";

        var sql = sqlProp.GetString()?.Trim();
        if (string.IsNullOrEmpty(sql))
            return "Error: SQL is empty";

        if (!sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            return "Error: Only SELECT statements allowed";

        _logger.LogInformation("Executing SQL: {Sql}", sql[..Math.Min(100, sql.Length)]);

        try
        {
            var response = await _pulseApiClient.GetAsync(
                $"{ApiRoutes.Execute}{Uri.EscapeDataString(sql)}", ct);

            // Check for HTTP errors
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("SQL execution returned {StatusCode}: {Error}", response.StatusCode, errorContent[..Math.Min(200, errorContent.Length)]);
                return $"❌ __SQL_ERROR__ ❌\n\n" +
                   $"**User asked**: {context.OriginalUserQuery}\n\n" +
                   $"**SQL Query Failed**:\n```sql\n{sql}\n```\n\n" +
                   $"**Error**: {errorContent}\n\n" +
                   $"You MUST generate a DIFFERENT SQL query and retry, OR use web_search if the query cannot be fixed.";
            }

            var result = await response.Content.ReadAsStringAsync(ct);

            // ✅ Check if result is empty (no data found)
            if (string.IsNullOrWhiteSpace(result) || result == "[]" || result == "{}" || result.Contains("No data found"))
            {
                _logger.LogWarning("SQL query returned no results");
                return $"⚠️ __SQL_NO_RESULTS__ ⚠️\n\n" +
                   $"**User asked**: {context.OriginalUserQuery}\n\n" +
                   $"**SQL query executed successfully but returned NO DATA from the database**:\n```sql\n{sql}\n```\n\n" +
                   $"This means the requested data does not exist in our database.\n\n" +
                   $"You MUST now use web_search to find this information externally.";
            }

            //return result;
            // ✅ Include user query context in successful SQL results
            return $"**User asked**: {context.OriginalUserQuery}\n\n" +
                   $"**SQL Query Results**:\n```sql\n{sql}\n```\n\n" +
                   $"**Data from database**:\n{result}";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "SQL execution failed with HTTP error");
            return $"__SQL_ERROR__\n\n**User asked**: {context.OriginalUserQuery}\n\n" +
                   $"Database connection failed: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL execution failed");
            return $"__SQL_ERROR__\n\n**User asked**: {context.OriginalUserQuery}\n\n" +
                   $"SQL error: {ex.Message}";
        }
    }

    /// <summary>
    /// Executes an action SQL query (INSERT, UPDATE, DELETE) with contextual information.
    /// </summary>
    private async Task<string> ExecuteActionSqlToolAsync(JsonElement args, QueryContext context, CancellationToken ct)
    {
        string sql = args.TryGetProperty("sql", out var sqlProp) ? sqlProp.GetString() ?? "" : "";
        if (string.IsNullOrEmpty(sql))
        {
            sql = args.TryGetProperty("query", out var sqlProp2) ? sqlProp2.GetString() ?? "" : "";
            if (string.IsNullOrEmpty(sql))
            {
                sql = args.TryGetProperty("arguments", out var sqlProp3) ? sqlProp3.GetString() ?? "" : "";
            }
        }

        if (string.IsNullOrEmpty(sql))
        {
            return "Error: Missing 'sql' argument.";
        }

        try
        {
            _logger.LogInformation("Executing SQL Action tool: {Sql}", sql);
            var response = await _pulseApiClient.GetAsync($"/AI/ExecuteAiUpdateInsertQry:{sql}", ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);

            // ✅ Include context in action SQL results
            return $"**User asked**: {context.OriginalUserQuery}\n\n" +
                   $"**Action SQL Executed**:\n```sql\n{sql}\n```\n\n" +
                   $"**Result**: {json} rows affected";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL tool execution error.");
            return $"Error executing SQL for query '{context.OriginalUserQuery}': {ex.Message}";
        }
    }

    /// <summary>
    /// Performs web search with contextual information.
    /// </summary>
    private async Task<string> WebSearchAsync(JsonElement args, QueryContext context, CancellationToken ct)
    {
        if (!args.TryGetProperty("query", out var queryProp))
            return "Error: Missing 'query' argument";

        var query = queryProp.GetString();
        if (string.IsNullOrEmpty(query))
            return "Error: Query is empty";

        var searchUrl = _configuration["OllamaApi:WebSearchEndpoint"]
            ?? "https://ollama.com/api/web_search";

        var response = await _ollamaService.PostAsync<OllamaWebSearchResponse>(
            searchUrl, new { query }, ct);

        if (response?.Results is not { Count: > 0 })
            return $"No search results found for '{query}' related to user question: {context.OriginalUserQuery}";

        var results = string.Join(" | ", response.Results.Select(r =>
            $"[{r.Title}]({r.Url}): {r.Snippet}"));

        // ✅ Include original user query in the tool result
        var contextualResult = $"**User asked**: {context.OriginalUserQuery}\n\n" +
                              $"**Web search results for '{query}'**:\n{results}";

        return contextualResult.Length > 8000 ? contextualResult[..8000] + "..." : contextualResult;
    }

    /// <summary>
    /// Fetches web page content with contextual information.
    /// </summary>
    private async Task<string> WebFetchAsync(JsonElement args, QueryContext context, CancellationToken ct)
    {
        if (!args.TryGetProperty("url", out var urlProp))
            return "Error: Missing 'url' argument";

        var url = urlProp.GetString();
        if (string.IsNullOrEmpty(url))
            return "Error: URL is empty";

        var fetchUrl = _configuration["OllamaApi:WebFetchEndpoint"]
            ?? "https://ollama.com/api/web_fetch";

        var response = await _ollamaService.PostAsync<OllamaWebFetchResponse>(
            fetchUrl, new { url }, ct);

        var content = response?.Content ?? "";

        // ✅ Include original user query and URL context in the tool result
        var contextualResult = $"**Original user question**: {context.OriginalUserQuery}\n\n" +
                              $"**Content fetched from {url}**:\n\n{content}";

        return contextualResult.Length > 12000 ? contextualResult[..12000] + "..." : contextualResult;
    }

    #endregion

    #region Schema & Examples Caching

    public async Task<string> GetDetailedSchemaAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(CacheKeys.Schema, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = SchemaCacheDuration;

            var schemas = await GetSchemaFromApiAsync(ct);
            if (schemas.Count == 0)
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30); // Short cache on failure
                return "";
            }

            var schemaBuilder = new StringBuilder();
            foreach (var schema in schemas)
            {
                AppendSchemaEntity(schemaBuilder, schema);
            }

            _logger.LogInformation("Schema cached: {TableCount} tables", schemas.Count);
            return schemaBuilder.ToString();
        }) ?? "";
    }

    public async Task<string> GetAiExamplesAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(CacheKeys.Examples, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = ExamplesCacheDuration;

            try
            {
                var response = await _pulseApiClient.GetAsync(ApiRoutes.Examples, ct);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(ct);

                var aiQueries = JsonSerializer.Deserialize<List<AiQuery>>(content, _jsonOptions) ?? [];

                if (aiQueries.Count == 0)
                    return "";

                _logger.LogInformation("Examples cached: {Count} queries", aiQueries.Count);
                return "\nExamples:\n" + string.Join("\n", aiQueries.Select(q =>
                    $"Q: {q.Question}\nSQL: {q.SqlQuery}"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch examples");
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return "";
            }
        }) ?? "";
    }

    private static void AppendSchemaEntity(StringBuilder sb, SchemaDto schema)
    {
        sb.AppendLine($"Entity: {schema.EntityType} (Table: {schema.TableName})");
        sb.AppendLine($"Primary Keys: {string.Join(", ", schema.PrimaryKeys ?? [])}");
        sb.AppendLine("Columns:");

        foreach (var col in schema.Columns ?? [])
        {
            sb.AppendLine($"  - {col.Name} ({col.DataType}, Nullable: {col.IsNullable}, PK: {col.IsPrimaryKey})");
        }

        sb.AppendLine("Relationships:");
        if (schema.Relationships is { Count: > 0 })
        {
            foreach (var rel in schema.Relationships)
            {
                sb.AppendLine($"  - {rel.NavigationName} → {rel.RelatedEntityType} ({rel.RelatedTableName})");
            }
        }
        else
        {
            sb.AppendLine("  - None");
        }
        sb.AppendLine();
    }

    #endregion

    #region Helper Methods

    public async Task<List<string>> GetOllamaModelsAsync(CancellationToken ct = default)
    {
        try
        {
            var tags = await _ollamaService.GetAsync<OllamaTagsResponse>(ApiRoutes.Tags, ct);
            return tags?.Models?.Select(m => m.Name).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Ollama models");
            return ["llama3.1:latest", "gpt-oss:latest"];
        }
    }

    private async Task<List<SchemaDto>> GetSchemaFromApiAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _pulseApiClient.GetAsync(ApiRoutes.Schema, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<SchemaDto>>(json, _jsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch schema");
            return [];
        }
    }

    public async Task<List<ClientSale>> GetClientSalesAsync(string clientId, CancellationToken ct = default)
    {
        try
        {
            var response = await _pulseApiClient.GetAsync(
                $"/Customers/Details/{Uri.EscapeDataString(clientId)}/Sales", ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<ClientSale>>(json, _jsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch client sales for {ClientId}", clientId);
            return [];
        }
    }

    public async Task<ClientCurrentStats> GetClientStatsAsync(string clientId, CancellationToken ct = default)
    {
        try
        {
            var response = await _pulseApiClient.GetAsync(
                $"/Customers/Details/{Uri.EscapeDataString(clientId)}/CurrentStats", ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<ClientCurrentStats>(json, _jsonOptions) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch client stats for {ClientId}", clientId);
            return new();
        }
    }

    public async Task<List<ClientRollerSpecification>> GetClientRollerSpecificationsAsync(
        string clientId,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(clientId);

        try
        {
            var response = await _pulseApiClient.GetAsync(
                $"/Customers/Details/{Uri.EscapeDataString(clientId)}/RollerSpecifications", ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);

            var specs = JsonSerializer.Deserialize<List<ClientRollerSpecification>>(json, _jsonOptions) ?? [];
            _logger.LogInformation("Fetched {Count} roller specifications for client {ClientId}",
                specs.Count, clientId);
            return specs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch roller specifications for {ClientId}", clientId);
            return [];
        }
    }

    #endregion

    #region Message Building

    private static OllamaMessage BuildSystemMessage(string schema, string examples, List<OllamaMessage> messages)
    {
        var userQuery = messages.LastOrDefault(m => m.Role == "user")?.Content ?? "Unknown";

        return new OllamaMessage
        {
            Role = "system",
            Content = $$"""
                You are PulseAI, assisting H&M Rollers with business inquiries.
                
                **Available Tools**: execute_sql, web_search, web_fetch
                - One web_search per query
                - Limit retries to 3 per tool
                - Stop after results; don't re-query
                
                **Database Schema**:
                {{schema}}
                
                **SQL Guidelines**:
                - SELECT only, no DDL/DML
                - Use exact table/column names
                - Handle nullability and data types
                - Optimize with WHERE and minimal JOINs
                
                {{examples}}
                
                **Query**: {{userQuery}}
                
                **Output Rules**:
                - Cite web sources
                - Don't return SQL as answer
                - All amounts in ZAR
                - Ask clarifying questions if needed
                - Say "I don't know" if unsure
                """
        };
    }

    private static List<object> BuildToolsList(bool enableSearch)
    {
        var tools = new List<object>
        {
            new
            {
                type = "function",
                function = new
                {
                    name = ToolNames.ExecuteSql,
                    description = "Execute SELECT query against dbPulse",
                    parameters = new
                    {
                        type = "object",
                        properties = new { sql = new { type = "string" } },
                        required = new[] { "sql" }
                    }
                }
            }
        };

        if (enableSearch)
        {
            tools.Add(new
            {
                type = "function",
                function = new
                {
                    name = ToolNames.WebSearch,
                    description = "Search the web",
                    parameters = new
                    {
                        type = "object",
                        properties = new { query = new { type = "string" } },
                        required = new[] { "query" }
                    }
                }
            });

            tools.Add(new
            {
                type = "function",
                function = new
                {
                    name = ToolNames.WebFetch,
                    description = "Fetch URL content",
                    parameters = new
                    {
                        type = "object",
                        properties = new { url = new { type = "string" } },
                        required = new[] { "url" }
                    }
                }
            });
        }

        return tools;
    }

    #endregion

    #region Chat Execution

    /// <summary>
    /// Executes a non-streaming chat request with tools.
    /// </summary>
    private async Task<OllamaChatResponse?> ExecuteChatAsync(
        List<OllamaMessage> messages,
        string model,
        bool enableSearch,
        CancellationToken ct)
    {
        var schemaText = await GetDetailedSchemaAsync(ct);
        if (string.IsNullOrEmpty(schemaText))
        {
            throw new InvalidOperationException("Schema unavailable");
        }

        var examples = await GetAiExamplesAsync(ct);
        var systemMessage = BuildSystemMessage(schemaText, examples, messages);

        var allMessages = new List<OllamaMessage> { systemMessage };
        allMessages.AddRange(messages);

        var tools = BuildToolsList(enableSearch);
        var request = new
        {
            model,
            messages = allMessages.ToArray(),
            tools = tools.ToArray(),
            think = true,
            keep_alive = "10m",
            stream = false,
            options = new { temperature = 0.0, num_ctx = 32000 }
        };

        _logger.LogInformation("Executing non-streaming chat request");

        return await _ollamaService.PostAsync<OllamaChatResponse>(ApiRoutes.Chat, request, ct);
    }

    #endregion
}