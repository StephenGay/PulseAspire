using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pulse.Models.AI.AIds;
using Pulse.Models.CustomComponents;
using Pulse.Models.Customers;
using Pulse.Models.Misc;
using Pulse.Web.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
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
            catch (Exception ex) when (ex is TaskCanceledException)
            {
                _logger.LogError(ex, "Ollama request cancelled.");
                dResult["Status"] = "Cancelled";
                dResult["Comments"] = "Request was cancelled by user.";
                return dResult;
            }
            catch (Exception ex) when (ex is TimeoutException)
            {
                _logger.LogError(ex, "Ollama request timed out.");
                dResult["Status"] = "Error";
                dResult["Comments"] = "Ollama timed out — try a simpler query.";
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
        public async Task<string> GetDetailedSchemaAsync(CancellationToken ct = default)
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

                var examples = aiQueries.Any() ? "\nHere are some examples of queries and their correct SQL Statements to guide you:\n" + string.Join("\n", aiQueries.Select(ex => $"Question: {ex.Question}\nSQL: {ex.SqlQuery}\n")) : "";

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

                    If you need information from the database, create a SQL Statement based on the provided Database Schema using these guidelines:
                    - Generate a valid, efficient SQL query (SELECT only, no DDL/DML) based on the Schema and user question.
                    - Ensure that you use the exact table and column names from the schema.
                    - Respect relationships for joins
                    - Handle nullability and data types (e.g., string filters with LIKE, dates with CAST).
                    - Optimize: Use WHERE for filters, JOINs only if needed, TOP if limiting results.
                    
                    - Use the 'execute_sql' tool using SQL Query as parameter to run your queries and retrieve results.
                    - If no results are returned from the database, use the 'web_search' and 'web_fetch' tools to find relevant information on the web
                    

                    The user question is:
                    {historyMessages.Last(m => m.Role == "user")?.Content}

                    - Check the following schema to see if you need information from the database:
                    Schema:
                    {schemaText}

                    {examples}

                    
                    - If you require more information to answer a question, you can ask clarifying questions to the user.
                    - Do not return a SQL Query as the answer. Use these results to formulate accurate and relevant responses to the user's questions.
                    
                        **Rules**:
                    - One tool call per type; stop after results.
                    - If tool results (e.g., web_search) are provided in the conversation, use them to refine your answer.
                    - After tool results, if result starts with 'Invalid Tool Name', provide the final answer without further tool calls. Do not re-search or re-query the same info.
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

            //var canThink = sModel is "gpt-oss:latest" or "qwen3:32b" or "deepseek-r1:14b";

            var ollamaRequest = new
            {
                model = sModel,
                messages = messages.ToArray(),
                tools = tools.ToArray(),
                think = true,
                keep_alive = "10m",
                stream = false,
                options = new { temperature = 0.0, num_ctx = 32000 }
            };

            try
            {
                _logger.LogInformation("Starting Ollama chat for query: {Query}", historyMessages.LastOrDefault()?.Content);
                var ollamaResponse = await _ollamaservice.PostAsync<OllamaChatResponse>("/api/chat", ollamaRequest, ct);
                //if (string.IsNullOrEmpty(ollamaResponse?.Message?.Content) && ollamaResponse?.Message?.ToolCalls?.Any() == false)
                //{
                //    return historyMessages.Last(m => m.Role != "user")?.Content.Trim() ?? "No response from Ollama.";
                //}
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
        public async Task HandleToolCalls(List<OllamaToolCall> toolCalls, List<OllamaMessage> messages, string model, CancellationToken ct)
        {
            var knownTools = new HashSet<string> { "execute_sql", "web_search", "web_fetch" , "execute_action_sql" };
            
            foreach (var toolCall in toolCalls)
            {
                var args = toolCall.Function.Arguments; // JsonElement
                var toolName = toolCall.Function.Name ?? "unknown"; // Null-check for safety

                string toolResult;

                if (_toolAttemptName == toolName && _toolAttempts >= MAX_TOOL_ATTEMPTS)
                {
                    //toolResult = $"Error: Max attempts ({MAX_TOOL_ATTEMPTS}) reached for tool '{toolName}'. Skipping.";
                    _logger.LogWarning($"Error: Max attempts ({MAX_TOOL_ATTEMPTS}) reached for tool '{toolName}'.");
                }
                foreach(var tn in knownTools)
                {
                    if (toolName.IndexOf(tn) > -1)
                    {
                        toolName = tn;
                    }
                }
                if (!knownTools.Contains(toolName))
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
                                _logger.LogInformation("Fetching from: {url}",url);
                                toolResult = await WebFetchAsync(url, ct);
                                _logger.LogInformation("Result: {res}",toolResult);
                                break;

                            case "execute_action_sql":
                                if (!args.TryGetProperty("sql", out var sqlActProp))
                                {
                                    toolResult = "Error: Missing 'sql' argument for execute_sql.";
                                    break;
                                }
                                var sqlAct = sqlActProp.GetString() ?? "";
                                toolResult = await ExecuteActionSqlToolAsync(args, ct);
                                break;

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
                if(sql.StartsWith("SELECT"))
                {
                    var response = await _pulseApiClient.GetAsync($"/AI/execute:{Uri.EscapeDataString(sql)}", ct);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync(ct);
                }
                else
                {
                    return "Error: Statement is not a SELECT statement";
                }
                
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
                return contentText.Length > 12000 ? contentText[..12000] + "..." : contentText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Web fetch error.");
                return $"Error in web fetch: {ex.Message}";
            }
        }
        public async Task<List<ClientSale>> GetClientSalesAsync(string clientID, CancellationToken ct = default)
        {
            try
            {
                var response = await _pulseApiClient.GetAsync($"/Customers/Details/{Uri.EscapeDataString(clientID)}/Sales", ct);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<List<ClientSale>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ClientSale>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching client sales.");
                return new List<ClientSale>();
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
        }//public async Task<string> AskPulseAIAsync(OllamaRequest ollamaRequest,  CancellationToken ct = default) // string prompt, string sModel, int NoOfContextTokens = 4096, int NoOfResponseTokens = 4000
        public async Task<string> AskPulseAIAsync(string prompt, string sModel, CancellationToken ct = default)
        {
            var ollamaRequest = new { model = sModel, prompt, stream = false };

            try
            {
                //string ollamaResult = await _ollamaservice.PostStreamAsync("/api/generate", ollamaRequest, ct);
                //return ollamaResult ?? "No analysis from Ollama.";
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

        public async IAsyncEnumerable<string> StreamChatWithOllamaAsync(List<OllamaMessage> historyMessages, string sModel, bool enableSearch = true, [EnumeratorCancellation] CancellationToken ct = default, int qryLoop = 0, bool inThinking = false)
        {
            if (_CancelQuery)
            {
                _CancelQuery = false;
                _toolAttempts = 0;
                _toolAttemptName = "";
                yield return "Ok, I cancelled the query.";
                yield break;
            }
            if (qryLoop >= MAX_TOTAL_QRY_ATTEMPTS)
            {
                _toolAttempts = 0;
                _toolAttemptName = "";
                yield return "My apologies, I seem to be caught in a loop. Maybe try and rephrase the question.";
                yield break;
            }

            string schemaText = string.Empty;
            string examples = string.Empty;
            string yMsg = string.Empty;
            try
            {
                schemaText = await GetDetailedSchemaAsync(ct);
                if (string.IsNullOrEmpty(schemaText))
                {
                    yMsg = "Error: Unable to retrieve database schema information.";
                }
                else
                {
                    examples = await GetAiExamplesAsync(ct);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during setup in ChatWithOllamaAsync.");
                _toolAttempts = 0;
                _toolAttemptName = "";
                yMsg = $"Error during setup: {ex.Message}";
                
            }
            if (!string.IsNullOrEmpty(yMsg))
            {
                yield return yMsg;
                yield break;
            }
            var systemMessage = new OllamaMessage
            {
                Role = "system",
                Content = $"""
            You are PulseAI, a helpful AI performing services for H&M Rollers.
            
            H&M Rollers primarily covers industrial rollers with Rubber, Polyurethane, and other specialized materials for
            various industries including paper, steel, food processing, and more.
            
            You are conversing with a colleague, and they have asked if you could provide meaningful information regarding
            the following:

            {historyMessages.Last(m => m.Role == "user")?.Content}

            You have been provided with the entire conversation so far in a list of messages. Go through them as they may provide context
            for the current query.

            Along with your vast General Knowledge, you will have the following tools to help you provide the best response for your fellow
            worker. Select whichever tool you think will help you the most.

            **Tools**
            1. The SQL Server 2022 database schema for the company database. You can examine this to see if the database contains any information
               that may help you. If it does have relevant data, you can write a SQL Statement and run it with the 'execute_sql' tool to get what
               need. The schema and guidelines for creating these statements can be found later in this message.
            2. You can search the web by using the 'web_search' tool. This will enable you to retrieve links and short snippets of the websites
               contents from websites that may help you. Examine the results returned by this tool to see if any of them can help you. Only
               use the web_search tool once per query.
            3. You can use the 'web_fetch' tool to retrieve more information from any sites of interest returned by web_search.

            If results are returned from the tool call, incorporate these results in your response and use them to determine either what the next step
            should be or if you now have enough information to provide a final response.

            You can use the tools as many times as you need to provide the best response. However, be careful to not re-use a tool to requery 
            information that has already been searched for. Also take care to not get caught in a loop repeating the same action. If this happens
            for more than 4 loops, exit and provide a response on the data you have. 

            If a tool call comes back with no results, first check if the tool name is a valid tool name. If it isn't do not incorporate the
            empty result in your reasoning.

            If you are unsure of anything, or require more information, ask clarifying questions to the user.

            Check the messages list for any messages that come after the users query, these are actions that have already been taken to respond
            to this query. Continue from the last message, do not start from the user query again, for e.g. if you previously received three
            links from the 'web_search' tool, but have only run the 'web_fetch' tool on the first one, move on to the second - don't start a
            new action.

            Here is the company database schema, detailing the tables, entities and how they relate to each other:
            
            {schemaText}

            If you need information from the database, create SQL Statements based on this Schema, just follow the guidelines listed below:
                
            **SQL Statement Creation Guidelines**
            - Generate a valid, efficient SQL query (SELECT only, no DDL/DML) based on the Schema and user question.
            - Ensure that you use the exact table and column names from the schema.
            - Respect relationships for joins
            - Handle nullability and data types (e.g., string filters with LIKE, dates with CAST).
            - Optimize: Use WHERE for filters, JOINs only if needed, TOP if limiting results.

            {examples}

            Once you have created the SQL Statement, pass it as a parameter to the 'execute_sql' tool to run it and retrieve the result.
            If the database does not return any results then you can try again with a different statement, but only up to 3 attempts,
            then move on to a different tool or provide your response.

            **Thinking Process**:
            - Always think step-by-step before responding or calling tools.
            - Stream your thoughts progressively if possible, let the user know what is happening as often as possible.
            - Only provide the reasoning since the last message, do not duplicate.
            - After thinking, either call a tool or provide the final answer.
            - If calling a tool, double-check toolname is execute_sql, web_search or web_fetch. No other toolname is permitted.

            **Formatting Rules For Your Final Response**:
            - Always aim to provide clear, concise, and accurate information in your responses.
            - Remember to cite sources when using web_search results to support your answers.
            - If using results from the database in your answer, state this in your answer.
            - Do not return a SQL Query as the answer.
            - If you do not know the answer to a question, then say you do not know. Do not make up an answer.
            - All amounts are South African Rands (ZAR)
            - End your answers with a friendly remark or offer further assistance. A suggestion of what may be of interest to look at next would be helpful.
            """
            };
            //var systemMessage = new OllamaMessage
            //{
            //    Role = "system",
            //    Content = $"""
            //You are PulseAI, a helpful AI performing services for H&M Rollers, a company that primarily covers industrial rollers with Rubber, Polyurethane, and other specialized materials.
            //We cover rollers for various industries including paper, steel, food processing, and more. You will be assisting your colleagues by answering
            //questions related to our business operations, products, and services as well as general topics.
            //To enable you to do this effectively, you will rely on your general knowledge, access to web search tools, and querying our internal SQL Server 2022 database. You will decide when
            //to use each resource to best answer the user's questions. Sometimes, you may need to use multiple resources in sequence to gather the necessary information.

            //**Thinking Process**:
            //- Always think step-by-step before responding or calling tools.
            //- Output your reasoning in <thinking> tags, e.g., <thinking>Step 1: Analyze query. Step 2: Check schema relevance.</thinking>
            //- Stream your thoughts progressively if possible.
            //- After thinking, either call tools or provide the final answer.

            //If you need information from the database, create a SQL Statement based on the provided Database Schema using these guidelines:
            //- Generate a valid, efficient SQL query (SELECT only, no DDL/DML) based on the Schema and user question.
            //- Ensure that you use the exact table and column names from the schema.
            //- Respect relationships for joins
            //- Handle nullability and data types (e.g., string filters with LIKE, dates with CAST).
            //- Optimize: Use WHERE for filters, JOINs only if needed, TOP if limiting results.

            //- Use the 'execute_sql' tool using SQL Query as parameter to run your queries and retrieve results.
            //- If no results are returned from the database, use the 'web_search' and 'web_fetch' tools to find relevant information on the web

            //The user question is:
            //{historyMessages.Last(m => m.Role == "user")?.Content}
            //- Check the following schema to see if you need information from the database:
            //Schema:
            //{schemaText}
            //{examples}

            //- If you require more information to answer a question, you can ask clarifying questions to the user.
            //- Do not return a SQL Query as the answer. Use these results to formulate accurate and relevant responses to the user's questions.

            //    **Rules**:
            //- Only one tool call allowed for the 'web_search' and 'web_fetch' tools. Do not re-search or re-query the same info.
            //- If tool results (e.g., web_search) are provided in the conversation, use them to refine your answer.
            //- After tool results, if result starts with 'Invalid Tool Name', provide the final answer without further tool calls. Do not re-search or re-query the same info.
            //- Always aim to provide clear, concise, and accurate information in your responses.
            //- Remember to cite sources when using web search results to support your answers.
            //- If using results from the database in your answer, state this in your answer.
            //- If you do not know the answer to a question, then say you do not know. Do not make up an answer.
            //- All amounts are South African Rands (ZAR)
            //- End your answers with a friendly remark or offer further assistance.
            //"""
            //};
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



            var ollamaRequest = new
            {
                model = sModel,
                messages = messages.ToArray(),
                tools = tools.ToArray(),
                think = "high",
                keep_alive = "30m",
                stream = true,  // Enable streaming
                options = new { temperature = 0.0, num_ctx = 32000, repeat_penalty = 1.2, frequency_penalty = 2, presence_penalty = 2 }
            };
            //var ollamaRequest = new
            //{
            //    model = flapper.Model,
            //    messages = messages.ToArray(),
            //    tools = tools.ToArray(),
            //    think = flapper.Think,
            //    keep_alive = "10m",
            //    stream = true,  // Enable streaming
            //    options = new { temperature = flapper.Options.Temperature, num_ctx = flapper.Options.NumCtx, num_predict = flapper.Options.NumPredict, repeat_penalty = flapper.Options.RepeatPenalty, frequency_penalty = flapper.Options.FrequencyPenalty, presence_penalty = flapper.Options.PresencePenalty }
            //};
            _logger.LogInformation("Starting Ollama chat stream for query: {Query}", historyMessages.LastOrDefault()?.Content);

                // Accumulate full message for tool call check
                var accumulatedMessage = new OllamaMessage { Role = "assistant" };
                
                bool hasYieldedContent = false;
                

                await foreach (var chunk in _ollamaservice.ChatStreamAsync("/api/chat", ollamaRequest, ct))
                {
                    if (!string.IsNullOrEmpty(chunk.Message.Thinking) && !inThinking)
                    {
                        inThinking = true;
                        yield return "Thinking:\n";
                    }
                // Append deltas
                    if (!string.IsNullOrEmpty(chunk.Message.Thinking)){
                        hasYieldedContent = true;
                        //accumulatedMessage.Content += chunk.Message.Thinking;
                        //_logger.LogInformation("Ollama chat stream thinking chunk: {ch}", chunk.Message.Thinking);
                        yield return chunk.Message.Thinking;
                    }
                //else if (!string.IsNullOrEmpty(chunk.Message.Content))
                
                // TESTING !!!
                    if (!string.IsNullOrEmpty(chunk.Message.Content))
                    {
                        if (inThinking)
                        {
                            inThinking = false;
                            yield return "Answer:\n";
                        }

                        //_logger.LogInformation("Ollama chat stream answer chunk: {ch}", chunk.Message.Content);
                        //accumulatedMessage.Content += chunk.Message.Content;
                        yield return chunk.Message.Content;  // Stream to UI
                        hasYieldedContent = true;
                    }

                    if (chunk.Message.ToolCalls != null)
                    {
                        accumulatedMessage.ToolCalls ??= new List<OllamaToolCall>();
                        accumulatedMessage.ToolCalls.AddRange(chunk.Message.ToolCalls);
                    }

                    if (chunk.Done)
                    {
                        if (accumulatedMessage.ToolCalls?.Any() == true)
                        {
                            // Yield progress before handling
                            if (!hasYieldedContent)
                            {
                                yield return "<thinking>Analyzing query and preparing tools...</thinking>\n";
                            }
                            else
                            {
                                yield return "\n<thinking>Tool calls detected. Processing...</thinking>\n";
                            }

                            // Handle each tool with specific progress yields
                            foreach (var toolCall in accumulatedMessage.ToolCalls)
                            {
                                var toolName = toolCall.Function.Name;
                                yield return $"<thinking>Invoking tool: {toolName}...</thinking>\n";

                                // Existing HandleToolCalls logic, but yield results summary if visible
                                await HandleToolCalls(new List<OllamaToolCall> { toolCall }, messages, sModel, ct);
                                yield return $"<thinking>Tool {toolName} complete. Incorporating results...</thinking>\n";
                            }

                            // TESTING TAKING OUT
                            //await HandleToolCalls(accumulatedMessage.ToolCalls, messages, sModel, ct);

                            // Recurse and yield from next iteration
                            await foreach (var recursiveChunk in StreamChatWithOllamaAsync(messages, sModel, enableSearch, ct, qryLoop + 1, inThinking))
                            {
                                //_logger.LogInformation("Ollama chat stream recursivechunk: {ch}", recursiveChunk);
                                yield return recursiveChunk;
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Ollama chat stream completed.");
                        }
                        yield break;
                    }
                }

                if (!hasYieldedContent)
                {
                    yield return "No response from Ollama.";
                }
        }
    
    }
}