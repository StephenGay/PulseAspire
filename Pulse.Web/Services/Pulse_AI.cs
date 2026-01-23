using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pulse.Models.AI.AIds;
using Pulse.Models.CustomComponents;
using Pulse.Models.Customers;
using Pulse.Models.Misc;
using Pulse.Web.Tools;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pulse.Web.Services
{
    public class Pulse_AI
    {
        private readonly HttpClient _pulseApiClient;
        private readonly ILogger<Pulse_AI> _logger;
        private readonly OllamaService _ollamaService;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        // Instance-level state (not static—thread-safe per service instance)
        private string? _cachedSchema;
        private string? _cachedExamples;
        private DateTime _cacheSchemaExpiry = DateTime.MinValue;
        private DateTime _cacheExamplesExpiry = DateTime.MinValue;
        private readonly object _cacheLock = new();

        // Per-query state (reset per request to avoid cross-request contamination)
        private string _toolAttemptName = "";
        private int _toolAttempts = 0;
        private bool _cancelQuery = false;

        private const int MaxToolAttempts = 5;
        private const int MaxTotalQueryAttempts = 10;
        private const string DefaultModel = "gpt-oss:latest";

        public Pulse_AI(
            IHttpClientFactory httpClientFactory,
            OllamaService ollamaService,
            ILogger<Pulse_AI> logger,
            IConfiguration configuration)
        {
            _pulseApiClient = httpClientFactory.CreateClient("PulseApiClient");
            _ollamaService = ollamaService ?? throw new ArgumentNullException(nameof(ollamaService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Resets query state. Call before each new query to prevent cross-request contamination.
        /// </summary>
        private void ResetQueryState()
        {
            _toolAttemptName = "";
            _toolAttempts = 0;
            _cancelQuery = false;
        }

        public Task CancelQueryAsync()
        {
            _cancelQuery = true;
            return Task.CompletedTask;
        }

        public async Task<Dictionary<string, string>> GetSQLFromOllamaAsync(
            string userQuery,
            string model,
            CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(userQuery);
            ArgumentException.ThrowIfNullOrEmpty(model);

            var result = new Dictionary<string, string>
            {
                { "Status", "" },
                { "Comments", "" },
                { "SQL", "" }
            };

            try
            {
                var schemaText = await GetDetailedSchemaAsync(ct);
                if (string.IsNullOrEmpty(schemaText))
                {
                    result["Status"] = "Error";
                    result["Comments"] = "Unable to retrieve database schema.";
                    return result;
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
                    "/api/generate", ollamaRequest, ct);

                if (ollamaResponse?.Done != true || string.IsNullOrEmpty(ollamaResponse.Response))
                {
                    result["Status"] = "Error";
                    result["Comments"] = "Ollama generation incomplete.";
                    return result;
                }

                var sql = ExtractSqlFromResponse(ollamaResponse.Response);
                if (string.IsNullOrEmpty(sql))
                {
                    result["Status"] = "Error";
                    result["Comments"] = "No SQL statement found in response.";
                    return result;
                }

                result["Status"] = "Success";
                result["Comments"] = ExtractComments(ollamaResponse.Response);
                result["SQL"] = sql;

                _logger.LogInformation("SQL generated successfully with {Tokens} tokens", ollamaResponse.PromptEvalCount);
                return result;
            }
            catch (OperationCanceledException)
            {
                result["Status"] = "Cancelled";
                result["Comments"] = "Request was cancelled by user.";
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SQL");
                result["Status"] = "Error";
                result["Comments"] = $"Error: {ex.Message}";
                return result;
            }
        }

        private static string BuildSqlPrompt(string schema, string examples, string userQuery)
        {
            return $$"""
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
        }

        public async Task<string> AskPulseAIAsync(
    string prompt,
    string model,
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
                    //options = new { temperature = 0.0, num_predict = 8000 }
                };

                var response = await _ollamaService.PostAsync<OllamaGenerateResponse>(
                    "/api/generate", request, ct);

                if (response?.Done != true || string.IsNullOrEmpty(response.Response))
                    //if (response == null || !response.Done || string.IsNullOrEmpty(response.Response))
                {
                    _logger.LogWarning("Empty response from Ollama for prompt: {Prompt}", prompt[..50]);
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
        private static string ExtractSqlFromResponse(string response)
        {
            var trimmed = response.Trim();

            // Try [SQL Start]/[SQL End]
            var startIdx = trimmed.IndexOf("[SQL Start]", StringComparison.Ordinal);
            if (startIdx >= 0)
            {
                startIdx += "[SQL Start]".Length;
                var endIdx = trimmed.IndexOf("[SQL End]", startIdx, StringComparison.Ordinal);
                if (endIdx > startIdx)
                {
                    return trimmed.Substring(startIdx, endIdx - startIdx).Trim();
                }
            }

            // Try markdown ```sql
            startIdx = trimmed.IndexOf("```sql", StringComparison.OrdinalIgnoreCase);
            if (startIdx >= 0)
            {
                startIdx += 6;
                var endIdx = trimmed.IndexOf("```", startIdx, StringComparison.Ordinal);
                if (endIdx > startIdx)
                {
                    return trimmed.Substring(startIdx, endIdx - startIdx).Trim();
                }
            }

            // Fallback: Extract SELECT statement
            var selectIdx = trimmed.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
            if (selectIdx >= 0)
            {
                var endIdx = trimmed.IndexOf(";", selectIdx);
                endIdx = endIdx < 0 ? trimmed.Length : endIdx + 1;
                return trimmed.Substring(selectIdx, endIdx - selectIdx).Trim();
            }

            return null;
        }

        private static string ExtractComments(string response)
        {
            var sqlStart = response.IndexOf("```sql", StringComparison.OrdinalIgnoreCase);
            if (sqlStart < 0)
            {
                sqlStart = response.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
            }

            if (sqlStart <= 0) return response.Trim();

            var comments = response.Substring(0, sqlStart).Trim();
            return comments.EndsWith("```sql", StringComparison.OrdinalIgnoreCase)
                ? comments[..^6].Trim()
                : comments;
        }

        public async Task<string> GetDetailedSchemaAsync(CancellationToken ct = default)
        {
            lock (_cacheLock)
            {
                if (DateTime.UtcNow < _cacheSchemaExpiry && _cachedSchema != null)
                {
                    return _cachedSchema;
                }
            }

            try
            {
                var schemas = await GetSchemaFromApiAsync(ct);
                if (!schemas.Any()) return "";

                var schemaBuilder = new StringBuilder();
                foreach (var schema in schemas)
                {
                    AppendSchemaEntity(schemaBuilder, schema);
                }

                lock (_cacheLock)
                {
                    _cachedSchema = schemaBuilder.ToString();
                    _cacheSchemaExpiry = DateTime.UtcNow.AddMinutes(120);
                }

                return _cachedSchema;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch schema");
                return "";
            }
        }

        private static void AppendSchemaEntity(StringBuilder sb, SchemaDto schema)
        {
            sb.AppendLine($"Entity: {schema.EntityType} (Table: {schema.TableName})");
            sb.AppendLine($"Primary Keys: {string.Join(", ", schema.PrimaryKeys ?? Enumerable.Empty<string>())}");
            sb.AppendLine("Columns:");

            foreach (var col in schema.Columns ?? Enumerable.Empty<ColumnDto>())
            {
                sb.AppendLine($"  - {col.Name} ({col.DataType}, Nullable: {col.IsNullable}, PK: {col.IsPrimaryKey})");
            }

            sb.AppendLine("Relationships:");
            if (schema.Relationships?.Any() == true)
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

        public async Task<string> GetAiExamplesAsync(CancellationToken ct = default)
        {
            lock (_cacheLock)
            {
                if (DateTime.UtcNow < _cacheExamplesExpiry && _cachedExamples != null)
                {
                    return _cachedExamples;
                }
            }

            try
            {
                var response = await _pulseApiClient.GetAsync("/AI/examples", ct);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(ct);

                var aiQueries = JsonSerializer.Deserialize<List<AiQuery>>(content, _jsonOptions) ?? new();

                var examples = aiQueries.Any()
                    ? "\nExamples:\n" + string.Join("\n", aiQueries.Select(q =>
                        $"Q: {q.Question}\nSQL: {q.SqlQuery}"))
                    : "";

                lock (_cacheLock)
                {
                    _cachedExamples = examples;
                    _cacheExamplesExpiry = DateTime.UtcNow.AddMinutes(60);
                }

                return examples;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch examples");
                return "";
            }
        }

        public async Task<List<string>> GetOllamaModelsAsync(CancellationToken ct = default)
        {
            try
            {
                var tags = await _ollamaService.GetAsync<OllamaTagsResponse>("/api/tags", ct);
                return tags?.Models?.Select(m => m.Name).ToList() ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch Ollama models");
                return new List<string> { "llama3.1:latest", "gemma3:27b", "sqlcoder:15b" };
            }
        }

        public async Task<string> ChatWithOllamaAsync(
            List<OllamaMessage> messages,
            string model,
            bool enableSearch = true,
            CancellationToken ct = default,
            int queryLoop = 0)
        {
            if (_cancelQuery)
            {
                ResetQueryState();
                return "Query cancelled.";
            }

            if (queryLoop >= MaxTotalQueryAttempts)
            {
                ResetQueryState();
                return "Query exceeded maximum loop attempts. Please rephrase.";
            }

            try
            {
                var response = await ExecuteChatAsync(messages, model, enableSearch, ct, queryLoop);

                if (response?.Message.ToolCalls?.Any() == true)
                {
                    await HandleToolCalls(response.Message.ToolCalls, messages, model, ct);
                    return await ChatWithOllamaAsync(messages, model, enableSearch, ct, queryLoop + 1);
                }

                ResetQueryState();
                return response.Message?.Content?.Trim() ?? "No response.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chat error");
                ResetQueryState();
                return $"Error: {ex.Message}";
            }
        }

        private async Task<OllamaChatResponse> ExecuteChatAsync(
            List<OllamaMessage> messages,
            string model,
            bool enableSearch,
            CancellationToken ct,
            int queryLoop)
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

            _logger.LogInformation("Executing chat request (loop {Loop})", queryLoop);
            return await _ollamaService.PostAsync<OllamaChatResponse>("/api/chat", request, ct)
                ?? throw new InvalidOperationException("No response from Ollama");
        }

        private static OllamaMessage BuildSystemMessage(string schema, string examples, List<OllamaMessage> messages)
        {
            var userQuery = messages.Last(m => m.Role == "user")?.Content ?? "Unknown";

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
                        name = "execute_sql",
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
                        name = "web_search",
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
                        name = "web_fetch",
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

        public async Task HandleToolCalls(List<OllamaToolCall> toolCalls, List<OllamaMessage> messages, string model, CancellationToken ct)
        {
            var knownTools = new HashSet<string> { "execute_sql", "web_search", "web_fetch" };

            foreach (var toolCall in toolCalls)
            {
                var toolName = NormalizeTooName(toolCall.Function.Name);

                if (!knownTools.Contains(toolName))
                {
                    _logger.LogWarning("Unknown tool: {Tool}", toolName);
                    messages.Add(new OllamaMessage
                    {
                        Role = "tool",
                        Content = $"Invalid tool '{toolName}'. Use: {string.Join(", ", knownTools)}",
                        ToolName = toolName
                    });
                    continue;
                }

                var toolResult = await ExecuteToolAsync(toolName, toolCall.Function.Arguments, ct);
                messages.Add(new OllamaMessage { Role = "tool", Content = toolResult, ToolName = toolName });
            }
        }

        private static string NormalizeTooName(string? toolName)
        {
            if (string.IsNullOrEmpty(toolName)) return "unknown";

            var normalized = toolName.ToLowerInvariant();
            var knownTools = new[] { "execute_sql", "web_search", "web_fetch" };

            return knownTools.FirstOrDefault(t => normalized.Contains(t)) ?? normalized;
        }

        private async Task<string> ExecuteToolAsync(string toolName, JsonElement args, CancellationToken ct)
        {
            try
            {
                return toolName switch
                {
                    "execute_sql" => await ExecuteSqlAsync(args, ct),
                    "web_search" => await WebSearchAsync(args, ct),
                    "web_fetch" => await WebFetchAsync(args, ct),
                    _ => "Unknown tool"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tool {Tool} failed", toolName);
                return $"Error: {ex.Message}";
            }
        }

        private async Task<string> ExecuteSqlAsync(JsonElement args, CancellationToken ct)
        {
            if (!args.TryGetProperty("sql", out var sqlProp))
            {
                return "Error: Missing 'sql' argument";
            }

            var sql = sqlProp.GetString()?.Trim();
            if (string.IsNullOrEmpty(sql))
            {
                return "Error: SQL is empty";
            }

            if (!sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                return "Error: Only SELECT statements allowed";
            }

            try
            {
                _logger.LogInformation("Executing SQL: {Sql}", sql);
                var response = await _pulseApiClient.GetAsync(
                    $"/AI/execute:{Uri.EscapeDataString(sql)}", ct);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync(ct);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "SQL execution failed");
                return $"Database error: {ex.Message}";
            }
        }

        private async Task<string> WebSearchAsync(JsonElement args, CancellationToken ct)
        {
            if (!args.TryGetProperty("query", out var queryProp))
            {
                return "Error: Missing 'query' argument";
            }

            var query = queryProp.GetString();
            if (string.IsNullOrEmpty(query))
            {
                return "Error: Query is empty";
            }

            try
            {
                var searchUrl = _configuration["OllamaApi:WebSearchEndpoint"]
                    ?? "https://ollama.com/api/web_search";

                var response = await _ollamaService.PostAsync<OllamaWebSearchResponse>(
                    searchUrl, new { query }, ct);

                if (response?.Results?.Any() != true)
                {
                    return "No search results found.";
                }

                var results = string.Join(" | ", response.Results.Select(r =>
                    $"[{r.Title}]({r.Url}): {r.Snippet}"));

                return results.Length > 8000 ? results[..8000] + "..." : results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Web search failed");
                return $"Search error: {ex.Message}";
            }
        }

        private async Task<string> WebFetchAsync(JsonElement args, CancellationToken ct)
        {
            if (!args.TryGetProperty("url", out var urlProp))
            {
                return "Error: Missing 'url' argument";
            }

            var url = urlProp.GetString();
            if (string.IsNullOrEmpty(url))
            {
                return "Error: URL is empty";
            }

            try
            {
                var fetchUrl = _configuration["OllamaApi:WebFetchEndpoint"]
                    ?? "https://ollama.com/api/web_fetch";

                var response = await _ollamaService.PostAsync<OllamaWebFetchResponse>(
                    fetchUrl, new { url }, ct);

                var content = response?.Content ?? "";
                return content.Length > 12000 ? content[..12000] + "..." : content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Web fetch failed");
                return $"Fetch error: {ex.Message}";
            }
        }

        public async IAsyncEnumerable<string> StreamChatWithOllamaAsync(
    List<OllamaMessage> messages,
    string model,
    bool enableSearch = true,
    [EnumeratorCancellation] CancellationToken ct = default,
    int queryLoop = 0)
        {
            if (_cancelQuery)
            {
                ResetQueryState();
                yield return "Query cancelled.";
                yield break;
            }

            if (queryLoop >= MaxTotalQueryAttempts)
            {
                ResetQueryState();
                yield return "Maximum loop attempts exceeded.";
                yield break;
            }

            // No try-catch here—delegate to helper that handles exceptions internally
            var chunks = await ExecuteStreamChatAsync(messages, model, enableSearch, ct, queryLoop);
            foreach (var chunk in chunks)
            {
                yield return chunk;
            }
        }

        /// <summary>
        /// Executes streaming chat with internal exception handling.
        /// Returns results as a list (no async enumerable, so try-catch is allowed).
        /// </summary>
        private async Task<List<string>> ExecuteStreamChatAsync(
            List<OllamaMessage> messages,
            string model,
            bool enableSearch,
            CancellationToken ct,
            int queryLoop)
        {
            var results = new List<string>();

            try
            {
                var schemaText = await GetDetailedSchemaAsync(ct);
                if (string.IsNullOrEmpty(schemaText))
                {
                    results.Add("Error: Schema unavailable");
                    return results;
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
                    think = "high",
                    keep_alive = "30m",
                    stream = true,
                    options = new { temperature = 0.0, num_ctx = 32000 }
                };

                _logger.LogInformation("Starting chat stream (loop {Loop})", queryLoop);

                var accumulatedMessage = new OllamaMessage { Role = "assistant" };
                var inThinking = false;
                var hasContent = false;

                await foreach (var chunk in _ollamaService.ChatStreamAsync("/api/chat", request, ct))
                {
                    if (!string.IsNullOrEmpty(chunk.Message?.Thinking))
                    {
                        if (!inThinking)
                        {
                            inThinking = true;
                            results.Add("\n**Thinking:**\n");
                        }
                        results.Add(chunk.Message.Thinking);
                        hasContent = true;
                    }

                    if (!string.IsNullOrEmpty(chunk.Message?.Content))
                    {
                        if (inThinking)
                        {
                            inThinking = false;
                            results.Add("\n**Response:**\n");
                        }
                        results.Add(chunk.Message.Content);
                        hasContent = true;
                    }

                    if (chunk.Message?.ToolCalls?.Any() == true)
                    {
                        accumulatedMessage.ToolCalls ??= new();
                        accumulatedMessage.ToolCalls.AddRange(chunk.Message.ToolCalls);
                    }

                    if (chunk.Done)
                    {
                        if (accumulatedMessage.ToolCalls?.Any() == true)
                        {
                            results.Add("\n*Processing tool calls...*\n");
                            await HandleToolCalls(accumulatedMessage.ToolCalls, messages, model, ct);

                            // Recursively execute and collect results
                            var recursiveResults = await ExecuteStreamChatAsync(
                                messages, model, enableSearch, ct, queryLoop + 1);
                            results.AddRange(recursiveResults);
                        }
                        break;
                    }
                }

                if (!hasContent && results.Count == 0)
                {
                    results.Add("No response from Ollama.");
                }

                ResetQueryState();
            }
            catch (OperationCanceledException)
            {
                ResetQueryState();
                results.Add("\nRequest cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stream chat error");
                ResetQueryState();
                results.Add($"\nError: {ex.Message}");
            }

            return results;
        }
        // Helper methods for API calls
        public async Task<List<ClientSale>> GetClientSalesAsync(string clientId, CancellationToken ct = default)
        {
            try
            {
                var response = await _pulseApiClient.GetAsync(
                    $"/Customers/Details/{Uri.EscapeDataString(clientId)}/Sales", ct);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<List<ClientSale>>(json, _jsonOptions) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch client sales");
                return new();
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
                _logger.LogError(ex, "Failed to fetch client stats");
                return new();
            }
        }

        private async Task<List<SchemaDto>> GetSchemaFromApiAsync(CancellationToken ct = default)
        {
            try
            {
                var response = await _pulseApiClient.GetAsync("/AI/schema", ct);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<List<SchemaDto>>(json, _jsonOptions) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch schema");
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

                var specs = JsonSerializer.Deserialize<List<ClientRollerSpecification>>(json, _jsonOptions) ?? new();
                _logger.LogInformation("Fetched {Count} roller specifications for client {ClientId}", specs.Count, clientId);
                return specs;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch roller specifications for client {ClientId}", clientId);
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching roller specifications");
                return new();
            }
        }
    }
}