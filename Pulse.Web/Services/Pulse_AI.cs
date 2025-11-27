using Microsoft.Extensions.Logging;
using Pulse.Models.CustomComponents;
using Pulse.Models.Customers;
using Pulse.Models.Misc;
using Pulse.Web.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
// 

namespace Pulse.Web.Services
{
    public class Pulse_AI
    {
        private readonly HttpClient _pulseApiClient;
        private readonly ILogger<Pulse_AI> _logger;
        private readonly OllamaService _ollamaservice;
        private readonly string _ollamaApiKey;
        private static string? _cachedSchema;
        private static string? _cachedExamples;
        private static DateTime _cacheSchemaExpiry = DateTime.MinValue;
        private static DateTime _cacheExamplesExpiry = DateTime.MinValue;
        private static readonly object _cacheLock = new();
        private static string _toolAttemptName = "";
        private static int _toolAttempts = 0;
        private static bool _CancelQuery = false;
        private const int MAX_TOOL_ATTEMPTS = 5;
        private const int MAX_TOTAL_QRY_ATTEMPTS = 10;
        private const string DEFAULT_MODEL = "gpt-oss:latest";

        public Pulse_AI(IHttpClientFactory httpClientFactory, OllamaService ollamaService, ILogger<Pulse_AI> logger, IConfiguration configuration)
        {
            _pulseApiClient = httpClientFactory.CreateClient("PulseApiClient");
            _ollamaservice = ollamaService;
            _logger = logger;
            
        }

        public Task CancelQueryAsync()
        {
            _CancelQuery = true;
            return Task.CompletedTask;
        }
        public async Task<Dictionary<string, string>> GetSQLFromOllamaAsync(string userQuery, string strModel, CancellationToken ct = default)
        {
            var dResult = new Dictionary<string, string>
            {
                { "Status", "" },
                { "Comments", "" },
                { "SQL", "" }
            };

            string schemaText;
            lock (_cacheLock)
            {
                schemaText = DateTime.UtcNow < _cacheSchemaExpiry ? _cachedSchema ?? "" : "";
            }
            if (string.IsNullOrEmpty(schemaText))
            {
                schemaText = await GetDetailedSchemaAsync(ct);
            }
            if (string.IsNullOrEmpty(schemaText))
            {
                dResult["Status"] = "Error";
                dResult["Comments"] = "Unable to retrieve database schema information.";
                return dResult;
            }

            string examples = await GetAiExamplesAsync(ct);

            string isRetry = "";
            if (userQuery.StartsWith("The previous SQL query was incorrect. Please provide a revised SQL query. "))
            {
                isRetry = "NOTE: The previous SQL query given was incorrect. Please provide a revised SQL query.";
                userQuery = userQuery.Replace("The previous SQL query was incorrect. Please provide a revised SQL query. ", "");
            }

            var strPrompt = $"""
                You are a SQL expert for a SQL Server 2022 database.
                - You will be provided with the database schema and a user question.
                - Generate a valid, efficient SQL query (SELECT only, no DDL/DML) based on the schema and user question.
                - Use the table/column names that are in the schema.
                - Respect relationships for joins
                - Handle nullability and data types (e.g., string filters with LIKE, dates with CAST).
                - Format DateTime data types as "dd-MM-yy"
                - Optimize: Use WHERE for filters, JOINs only if needed, TOP if limiting results.
              
                Schema:
                {schemaText}
                {examples}
                User Question:
                {userQuery}
                """;

            var ollamaRequest = new
            {
                model = strModel,
                prompt = strPrompt,
                stream = false,
                options = new { temperature = 0.0, num_predict = 8000 }
            };

            try
            {
                var ollamaResponse = await _ollamaservice.PostAsync<OllamaGenerateResponse>("/api/generate", ollamaRequest, ct);
                if (ollamaResponse == null || !ollamaResponse.Done)
                {
                    dResult["Status"] = "Error";
                    dResult["Comments"] = "Ollama generation incomplete.";
                    return dResult;
                }

                var fullResponse = ollamaResponse.Response?.Trim() ?? "";
                if (string.IsNullOrEmpty(fullResponse))
                {
                    dResult["Status"] = "Error";
                    dResult["Comments"] = "Ollama returned an empty response.";
                    return dResult;
                }

                var sqlStartAt = fullResponse.IndexOf("```sql", StringComparison.OrdinalIgnoreCase);
                if (sqlStartAt < 0)
                {
                    sqlStartAt = fullResponse.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    sqlStartAt += 6;
                }
                if (sqlStartAt < 0)
                {
                    dResult["Status"] = "Error";
                    dResult["Comments"] = "Ollama did not return a SQL statement.";
                    return dResult;
                }

                var sqlEndAt = fullResponse.IndexOf("```", sqlStartAt, StringComparison.OrdinalIgnoreCase);
                if (sqlEndAt < 0) sqlEndAt = fullResponse.Length;

                var sql = fullResponse.Substring(sqlStartAt, sqlEndAt - sqlStartAt).Trim();
                dResult["Status"] = "Success";
                var comments = fullResponse.Substring(0, sqlStartAt).Trim();
                if (comments.EndsWith("```sql", StringComparison.OrdinalIgnoreCase))
                {
                    comments = comments.Substring(0, comments.Length - 6).Trim();
                }
                dResult["Comments"] = comments;
                dResult["SQL"] = sql;
                return dResult;
            }
            catch (Exception ex) when (ex is TimeoutException || ex is TaskCanceledException)
            {
                _logger.LogError(ex, "Ollama request timed out or canceled.");
                dResult["Status"] = "Error";
                dResult["Comments"] = "Ollama timed out or canceled—try a simpler query.";
                return dResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSQLFromOllamaAsync.");
                dResult["Status"] = "Error";
                dResult["Comments"] = $"Unexpected error: {ex.Message}";
                return dResult;
            }
        }
        private async Task<string> GetDetailedSchemaAsync(CancellationToken ct = default)
        {
            lock (_cacheLock)
            {
                if (DateTime.UtcNow < _cacheSchemaExpiry && _cachedSchema != null) return _cachedSchema;
            }

            try
            {
                var schemas = await GetSchemaFromApiAsync(ct);
                if (!schemas.Any())
                {
                    return "";
                }

                var schemaBuilder = new StringBuilder();
                foreach (var schema in schemas)
                {
                    schemaBuilder.AppendLine($"Entity: {schema.EntityType} (Table: {schema.TableName})");
                    schemaBuilder.AppendLine($"Primary Keys: {string.Join(", ", schema.PrimaryKeys ?? Enumerable.Empty<string>())}");
                    schemaBuilder.AppendLine("Columns:");
                    foreach (var col in schema.Columns ?? Enumerable.Empty<ColumnDto>())
                    {
                        schemaBuilder.AppendLine($"- {col.Name} (Type: {col.DataType}, Nullable: {col.IsNullable}, PK: {col.IsPrimaryKey})");
                    }
                    schemaBuilder.AppendLine("Relationships:");
                    if (schema.Relationships?.Any() != true)
                    {
                        schemaBuilder.AppendLine("- None");
                    }
                    else
                    {
                        foreach (var rel in schema.Relationships)
                        {
                            schemaBuilder.AppendLine($"- To {rel.RelatedEntityType} (Table: {rel.RelatedTableName}), Navigation: {rel.NavigationName}, FK Columns: {string.Join(", ", rel.ForeignKeyColumns ?? Enumerable.Empty<string>())}, Cardinality: {rel.Cardinality}");
                        }
                    }
                    schemaBuilder.AppendLine();
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
                _logger.LogError(ex, "Failed to fetch or process schema.");
                return "";
            }
        }
        public async Task<string> GetAiExamplesAsync(CancellationToken ct = default)
        {
            lock (_cacheLock)
            {
                if (DateTime.UtcNow < _cacheExamplesExpiry && _cachedExamples != null) return _cachedExamples;
            }

            try
            {
                var response = await _pulseApiClient.GetAsync("/AI/examples", ct);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(ct);
                var aiQueries = JsonSerializer.Deserialize<List<AiQuery>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AiQuery>();

                var examples = aiQueries.Any() ? "\nExamples of correct queries:\n" + string.Join("\n", aiQueries.Select(ex => $"Question: {ex.Question}\nSQL: {ex.SqlQuery}\n")) : "";

                lock (_cacheLock)
                {
                    _cachedExamples = examples;
                    _cacheExamplesExpiry = DateTime.UtcNow.AddMinutes(60);
                }
                return examples;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch AI examples.");
                return "";
            }
        }
        public async Task<List<string>> GetOllamaModelsAsync(CancellationToken ct = default)
        {
            try
            {
                var tags = await _ollamaservice.GetAsync<OllamaTagsResponse>("/api/tags", ct);
                return tags?.Models?.Select(m => m.Name).ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch Ollama models - Using fallback.");
                return new List<string> { "llama3.1:latest", "gemma3:27b", "sqlcoder:15b" };
            }
        }
        public async Task<string> ChatWithOllamaAsync(List<OllamaMessage> historyMessages, string sModel, bool enableSearch = true, CancellationToken ct = default, int qryLoop = 0)
        {
            if (_CancelQuery)
            {
                _CancelQuery = false;
                _toolAttempts = 0;
                _toolAttemptName = "";
                return "Ok, I cancelled the query.";
            }
            if (qryLoop >= MAX_TOTAL_QRY_ATTEMPTS)
            {
                _toolAttempts = 0;
                _toolAttemptName = "";
                return "My apologies, I seem to be caught in a loop. Maybe try and rephrase the question.";
            }

            var schemaText = await GetDetailedSchemaAsync(ct);
            if (string.IsNullOrEmpty(schemaText))
            {
                return "Error: Unable to retrieve database schema information.";
            }

            var examples = await GetAiExamplesAsync(ct);

            var systemMessage = new OllamaMessage
            {
                Role = "system",
                //Content = $"""
                //    You are PulseAI, a helpful AI for H&M Rollers[](https://www.hmrollers.com/), covering rollers with Rubber, Polyurethane, etc., for paper, steel, food processing industries. Answer queries on operations, products, services, or general topics using:
                //    - Database schema for internal data (e.g., customer counts, roller materials).
                //    - 'web_search' for trends/external info; 'web_fetch' for URL content.
                //    - 'execute_sql' to run SQL queries (SELECT only).
                //    Schema:
                //    {schemaText}
                //    Examples:
                //    {examples}
                //    User Question: {historyMessages.LastOrDefault(m => m.Role == "user")?.Content ?? ""}
                //    **Steps**:
                //    1. Check schema for relevant tables (e.g., ClientMaster for customers).
                //    2. If DB needed, generate concise SELECT query (use exact table/column names, WHERE for filters, JOIN only if required).
                //    3. Run query via 'execute_sql'.
                //    4. If external info needed, use 'web_search' once, then 'web_fetch' if specific URL required.
                //    5. After one tool call per type, deliver final answer — no further calls.

                //    (Do not include the SQL Query in your response)
                //    **Output Format** (always use this structure):
                //    **Web Search Results**(if used):
                //    1. [Title](URL): Snippet...
                //    2. ...
                //    **SQL Results**(if used): Summary of data, e.g., "42 customers start with A."
                //    Answer: Concise response with citations [1], [2].
                //    End with offer to help further.
                //    **Rules**:
                //    - Use schema for SQL; respect joins, nullability, data types (e.g., LIKE for strings).
                //    - Cite web sources. If no answer, say "I don't know."
                //    - One tool call per type; stop after results.
                //    """
                Content = $"""
                    You are PulseAI, a helpful AI performing services for H&M Rollers, a company that primarily covers industrial rollers with Rubber, Polyurethane, and other specialized materials.
                    Website: https://www.hmrollers.com/ . We cover rollers for various industries including paper, steel, food processing, and more. You will be assisting your colleagues by answering 
                    questions related to our business operations, products, and services as well as general topics.

                    To enable you to do this effectively, you will rely on your general knowledge, access to web search tools, and querying our internal SQL Server 2022 database. You will decide when 
                    to use each resource to best answer the user's questions. Sometimes, you may need to use multiple resources in sequence to gather the necessary information. 

                    -If you need to look up recent information, trends or other information not contained in the database, you can use the 'web_search' tool
                        to perform web searches and the 'web_fetch' tool to retrieve full content from specific URLs

                    The user question is:
                    {historyMessages.Last(m => m.Role == "user")?.Content}

                    - Check the following schema to see if you need information from the database:
                    Schema:
                    {schemaText}

                    {examples}

                    If you need information from the database, create a SQL Statement based on the Schema using these guidelines:
                    - Generate a valid, efficient SQL query (SELECT only, no DDL/DML) based on the Schema and user question.
                    - Ensure that you use the exact table and column names from the schema.
                    - Respect relationships for joins
                    - Handle nullability and data types (e.g., string filters with LIKE, dates with CAST).
                    - Optimize: Use WHERE for filters, JOINs only if needed, TOP if limiting results.

                    - Use the 'execute_sql' tool using SQL Query as parameter to run your queries and retrieve results.
                    - If you require more information to answer a question, you can ask clarifying questions to the user.
                    - Do not return a SQL Query as the answer. Use these results to formulate accurate and relevant responses to the user's questions.

                    - If tool results (e.g., web_search) are provided in the conversation, use them to refine your answer.
                    - **CRITICAL: After tool results, provide the final answer without further tool calls. Do not re-search or re-query the same info.**
                    - Always aim to provide clear, concise, and accurate information in your responses.
                    - Remember to cite sources when using web search results to support your answers.
                    - If you do not know the answer to a question, then say you do not know. Do not make up an answer.
                    - All amounts are South African Rands (ZAR)
                    - End your answers with a friendly remark or offer further assistance.
                    """
            };

            var messages = new List<OllamaMessage> { systemMessage };
            messages.AddRange(historyMessages);

            var tools = new List<object>();
            if (enableSearch)
            {
                tools.Add(new { type = "function", function = new { name = "web_search", description = "Search the web for current information.", parameters = new { type = "object", properties = new { query = new { type = "string", description = "Search query" } }, required = new[] { "query" } } } });
                tools.Add(new { type = "function", function = new { name = "web_fetch", description = "Fetch full content from a URL.", parameters = new { type = "object", properties = new { url = new { type = "string", description = "URL to fetch" } }, required = new[] { "url" } } } });
            }
            tools.Add(new { type = "function", function = new { name = "execute_sql", description = "Execute a SELECT SQL query against the dbPulse database and return results.", parameters = new { type = "object", properties = new { sql = new { type = "string", description = "The SQL query to execute" } }, required = new[] { "sql" } } } });
            // Commented: execute_action_sql for safety (enable with user auth)
            // tools.Add(new { type = "function", function = new { name = "execute_action_sql", ... } });

            var canThink = sModel is "gpt-oss:latest" or "qwen3:32b" or "deepseek-r1:14b";

            var ollamaRequest = new
            {
                model = sModel,
                messages = messages.ToArray(),
                tools = tools.ToArray(),
                think = canThink,
                stream = false,
                options = new { temperature = 0.0, num_ctx = 32000 }
            };

            try
            {
                _logger.LogInformation("Starting Ollama chat for query: {Query}", historyMessages.LastOrDefault()?.Content);
                var ollamaResponse = await _ollamaservice.PostAsync<OllamaChatResponse>("/api/chat", ollamaRequest, ct);
                if (ollamaResponse?.Message?.ToolCalls?.Any() == true)
                {
                    await HandleToolCalls(ollamaResponse.Message.ToolCalls, messages, sModel, ct);
                    return await ChatWithOllamaAsync(messages, sModel, enableSearch, ct, qryLoop + 1);
                }
                _logger.LogInformation("Ollama chat completed.");
                return ollamaResponse?.Message?.Content?.Trim() ?? "No response from Ollama.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ChatWithOllamaAsync.");
                _toolAttempts = 0;
                _toolAttemptName = "";
                return $"Error: {ex.Message}";
                //throw new Exception($"Ollama chat failed: {ex.Message}");
            }
        }
        private async Task HandleToolCalls(List<OllamaToolCall> toolCalls, List<OllamaMessage> messages, string model, CancellationToken ct)
        {
            var knownTools = new HashSet<string> { "execute_sql", "web_search", "web_fetch" /*, "execute_action_sql" if enabled */ };

            foreach (var toolCall in toolCalls)
            {
                var args = toolCall.Function.Arguments; // JsonElement
                var toolName = toolCall.Function.Name ?? "unknown"; // Null-check for safety

                string toolResult;

                if (_toolAttemptName == toolName && _toolAttempts >= MAX_TOOL_ATTEMPTS)
                {
                    toolResult = $"Error: Max attempts ({MAX_TOOL_ATTEMPTS}) reached for tool '{toolName}'. Skipping.";
                    _logger.LogWarning(toolResult);
                }
                else if (!knownTools.Contains(toolName))
                {
                    if(toolName == "assistant<|channel|>commentary") 
                    {
                        toolResult = "Ignoring commentary tool call.";
                        _logger.LogInformation("Ignoring commentary tool call.");
                        continue;
                    }
                    toolResult = $"Invalid tool name '{toolName}'. Available tools: {string.Join(", ", knownTools)}. Please use a valid tool or respond directly.";
                    _logger.LogWarning("Invalid tool call attempted: {Name}", toolName);
                }
                else
                {
                    try
                    {
                        switch (toolName)
                        {
                            case "execute_sql":
                                if (!args.TryGetProperty("sql", out var sqlProp))
                                {
                                    toolResult = "Error: Missing 'sql' argument for execute_sql.";
                                    break;
                                }
                                var sql = sqlProp.GetString() ?? "";
                                toolResult = await ExecuteSqlToolAsync(sql, ct);
                                break;

                            case "web_search":
                                if (!args.TryGetProperty("query", out var queryProp))
                                {
                                    toolResult = "Error: Missing 'query' argument for web_search.";
                                    break;
                                }
                                var query = queryProp.GetString() ?? "";
                                toolResult = await WebSearchAsync(query, ct);
                                break;

                            case "web_fetch":
                                if (!args.TryGetProperty("url", out var urlProp))
                                {
                                    toolResult = "Error: Missing 'url' argument for web_fetch.";
                                    break;
                                }
                                var url = urlProp.GetString() ?? "";
                                toolResult = await WebFetchAsync(url, ct);
                                break;

                            // Add case for "execute_action_sql" if enabling it

                            default:
                                toolResult = $"Unexpected tool: {toolName}"; // Shouldn't hit if checked above
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Tool '{toolName}' execution failed.");
                        toolResult = $"Error in tool '{toolName}': {ex.Message}";
                    }
                }

                // Update attempt tracking
                if (toolResult.StartsWith("Error") || toolResult.StartsWith("Invalid"))
                {
                    if (_toolAttemptName == toolName) _toolAttempts++;
                    else
                    {
                        _toolAttemptName = toolName;
                        _toolAttempts = 1;
                    }
                }
                else
                {
                    _toolAttempts = 0;
                    _toolAttemptName = "";
                }

                // Only add if not skipping max attempts
                if (!toolResult.Contains("Skipping"))
                {
                    messages.Add(new OllamaMessage { Role = "tool", Content = toolResult, ToolName = toolName });
                }
            }
        }
        private async Task<string> ExecuteSqlToolAsync(string sql, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return "Error: Missing 'sql' argument.";
            }

            try
            {
                _logger.LogInformation("Executing SQL tool: {Sql}", sql);
                sql = sql.Replace("\n", " "); // Fixed
                var response = await _pulseApiClient.GetAsync($"/AI/execute:{Uri.EscapeDataString(sql)}", ct);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQL tool execution error.");
                return $"Error executing SQL: {ex.Message}";
            }
        }
        private async Task<string> ExecuteActionSqlToolAsync(JsonElement args, CancellationToken ct)
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
            //var apiClient = _httpClientFactory.CreateClient("PulseApiClient");
            try
            {
                _logger.LogInformation("Executing SQL Action tool: {Sql}", sql);
                // TESTING PUTTING THIS BACK
                //sql = sql.Replace("\n", " ");
                var response = await _pulseApiClient.GetAsync($"/AI/ExecuteAiUpdateInsertQry:{sql}", ct);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                //var result = JsonSerializer.Deserialize<string>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); // Return raw JSON data for Ollama to use in next turn
                return $"{json} rows affected";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQL tool execution error.");
                return $"Error executing SQL: {ex.Message}";
            }
        }
        private async Task<string> WebSearchAsync(string query, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(query))
            {
                return "Error: Missing 'query' argument for web_search.";
            }

            var searchRequest = new { query };

            try
            {
                _logger.LogInformation("Calling web_search with query: {Query}", query);
                var searchResponse = await _ollamaservice.PostAsync<OllamaWebSearchResponse>("https://ollama.com/api/web_search", searchRequest, ct); // Note: Consider replacing with local or other API for production
                if (searchResponse?.Results == null || !searchResponse.Results.Any())
                {
                    return "No web search results found.";
                }

                var resultText = string.Join("|", searchResponse.Results.Select(r => $"[{r.Title}]({r.Url}): {r.Snippet}"));
                return resultText.Length > 8000 ? resultText[..8000] + "..." : resultText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Web search error.");
                return $"Error in web search: {ex.Message}";
            }
        }
        private async Task<string> WebFetchAsync(string url, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(url))
            {
                return "Error: Missing 'url' argument for web_fetch.";
            }

            var fetchRequest = new { url };

            try
            {
                var fetchResponse = await _ollamaservice.PostAsync<OllamaWebFetchResponse>("https://ollama.com/api/web_fetch", fetchRequest, ct);
                var contentText = fetchResponse?.Content ?? "";
                return contentText.Length > 8000 ? contentText[..8000] + "..." : contentText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Web fetch error.");
                return $"Error in web fetch: {ex.Message}";
            }
        }
        public async Task<List<ClientSales>> GetClientSalesAsync(string clientID, CancellationToken ct = default)
        {
            try
            {
                var response = await _pulseApiClient.GetAsync($"/Customers/Details/{Uri.EscapeDataString(clientID)}/Sales", ct);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<List<ClientSales>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ClientSales>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching client sales.");
                return new List<ClientSales>();
            }
        }
        public async Task<ClientCurrentStats> GetClientStatsAsync(string fullclientID, CancellationToken ct = default)
        {
            try
            {
                var response = await _pulseApiClient.GetAsync($"/Customers/Details/{Uri.EscapeDataString(fullclientID)}/CurrentStats", ct);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<ClientCurrentStats>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new ClientCurrentStats();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetClientStatsAsync.");
                return new ClientCurrentStats();
            }
        }
        public async Task<List<ClientRollerSpecification>> GetClientRollerSpecificationsAsync(string clientID, CancellationToken ct = default)
        {
            try
            {
                var response = await _pulseApiClient.GetAsync($"/Customers/Details/{Uri.EscapeDataString(clientID)}/RollerSpecifications", ct);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<List<ClientRollerSpecification>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ClientRollerSpecification>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetClientRollerSpecificationsAsync.");
                return new List<ClientRollerSpecification>();
            }
        }
        public async Task<string> AskPulseAIAsync(string prompt, string sModel, CancellationToken ct = default)
        {
            var ollamaRequest = new { model = sModel, prompt, stream = false };

            try
            {
                var ollamaResult = await _ollamaservice.PostAsync<OllamaGenerateResponse>("/api/generate", ollamaRequest, ct);
                return ollamaResult?.Response?.Trim() ?? "No analysis from Ollama.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AskPulseAIAsync.");
                return $"Error: {ex.Message}";
            }
        }
        private async Task<List<SchemaDto>> GetSchemaFromApiAsync(CancellationToken ct = default)
        {
            try
            {
                var response = await _pulseApiClient.GetAsync("/AI/schema", ct);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                _logger.LogDebug("Fetched schema JSON: {Json}", json);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, AllowTrailingCommas = true };
                return JsonSerializer.Deserialize<List<SchemaDto>>(json, options) ?? ParseSchemaFallback(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Schema API call failed.");
                return new List<SchemaDto>();
            }
        }
        private List<SchemaDto> ParseSchemaFallback(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var schemas = new List<SchemaDto>();
                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var schemaElem in root.EnumerateArray())
                    {
                        var schema = new SchemaDto
                        {
                            EntityType = schemaElem.TryGetProperty("entityType", out var et) ? et.GetString() : null,
                            TableName = schemaElem.TryGetProperty("tableName", out var tn) ? tn.GetString() : null,
                            PrimaryKeys = schemaElem.TryGetProperty("primaryKeys", out var pk) && pk.ValueKind == JsonValueKind.Array ? pk.EnumerateArray().Select(p => p.GetString()).Where(s => s != null).ToList() : new List<string>(),
                            Columns = schemaElem.TryGetProperty("columns", out var cols) && cols.ValueKind == JsonValueKind.Array ?
                                cols.EnumerateArray().Select(c => new ColumnDto
                                {
                                    Name = c.TryGetProperty("name", out var n) ? n.GetString() : null,
                                    DataType = c.TryGetProperty("dataType", out var dt) ? dt.GetString() : null,
                                    IsNullable = c.TryGetProperty("isNullable", out var nullable) ? nullable.GetBoolean() : false,
                                    IsPrimaryKey = c.TryGetProperty("isPrimaryKey", out var isPk) ? isPk.GetBoolean() : false
                                }).Where(c => c.Name != null).ToList() : new List<ColumnDto>(),
                            Relationships = schemaElem.TryGetProperty("relationships", out var rels) && rels.ValueKind == JsonValueKind.Array ?
                                rels.EnumerateArray().Select(r => new RelationshipDto
                                {
                                    NavigationName = r.TryGetProperty("navigationName", out var nn) ? nn.GetString() : null,
                                    RelatedEntityType = r.TryGetProperty("relatedEntityType", out var ret) ? ret.GetString() : null,
                                    RelatedTableName = r.TryGetProperty("relatedTableName", out var rtn) ? rtn.GetString() : null,
                                    ForeignKeyColumns = r.TryGetProperty("foreignKeyColumns", out var fk) && fk.ValueKind == JsonValueKind.Array ? fk.EnumerateArray().Select(f => f.GetString()).Where(f => f != null).ToList() : new List<string>(),
                                    Cardinality = r.TryGetProperty("cardinality", out var card) ? card.GetString() : null
                                }).Where(r => r.NavigationName != null).ToList() : null
                        };
                        if (schema.EntityType != null && schema.TableName != null)
                        {
                            schemas.Add(schema);
                        }
                    }
                }
                return schemas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallback schema parse failed.");
                return new List<SchemaDto>();
            }
        }
    }
}