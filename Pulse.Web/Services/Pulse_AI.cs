using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Polly.Timeout;
using Pulse.Models.CustomComponents;
using Pulse.Models.Customers;
using Pulse.Models.Misc;
using Pulse.Models.Organizational;
using Pulse.Web.Services;
using Pulse.Web.Tools;
using StackExchange.Redis;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Pulse.Web.Services
{
    public class Pulse_AI
    {
        private readonly HttpClient _ollamaClient;
        private readonly HttpClient _pulseApiClient;
        private readonly ILogger<Pulse_AI> _logger; // Optional for logging
        //private readonly OllamaOptions _options;
        private readonly string _ollamaApiKey = "76c15b3643ba413aac7429bbb64e119a._BO65S6KcVBYl9PpsPwSc5WI";
        private static string? _cachedSchema;
        private static string? _cachedExamples;
        private static DateTime _cacheSchemaExpiry = DateTime.MinValue;
        private static DateTime _cacheExamplesExpiry = DateTime.MinValue;

        private static string _toolAttemptName = "";
        private static int _toolAttempts = 0;
        private static bool _CancelQuery = false;
        const int MAX_TOOL_ATTEMPTS = 5;
        const int MAX_TOTAL_QRY_ATTEMPTS = 10;
        const string DEFAULT_MODEL = "gpt-oss:latest";

        public Pulse_AI(IHttpClientFactory httpClientFactory, ILogger<Pulse_AI> logger)
        {
            _ollamaClient = httpClientFactory.CreateClient("OllamaClient");
            if (!string.IsNullOrEmpty(_ollamaApiKey))
            {
                _ollamaClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ollamaApiKey);
            }
            _pulseApiClient = httpClientFactory.CreateClient("PulseApiClient");

            _logger = logger;
        }

        public async Task CancelQueryAsync() { _CancelQuery = true; await Task.CompletedTask; }
        public async Task<Dictionary<string, string>> GetSQLFromOllamaAsync(string userQuery, string strModel, CancellationToken ct = default)
        {
            Dictionary<string, string> dResult = new()
            {
                { "Status", "" },
                { "Comments", "" },
                { "SQL", "" }
            };

            //var httpClient = _httpClientFactory.CreateClient("OllamaClient");

            string schemaText = await GetDetailedSchemaAsync(userQuery, ct);
            if (string.IsNullOrEmpty(schemaText))
            {
                dResult["Status"] = "Error";
                dResult["Comments"] = "Unable to retrieve database schema information.";
                return dResult;
            }

            string examples = await GetAiExamplesAsync();
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

            var jsonContent = JsonSerializer.Serialize(ollamaRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _ollamaClient.PostAsync("/api/generate", content, ct);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync(ct);
                var ollamaResponse = JsonSerializer.Deserialize<OllamaGenerateResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
                var sqlStartAt = fullResponse.IndexOf("```sql");
                if (sqlStartAt < 0)
                {
                    sqlStartAt = fullResponse.IndexOf("SELECT");
                }
                else
                {
                    sqlStartAt += 6; // Move past '''sql
                }
                if (sqlStartAt < 0)
                {
                    dResult["Status"] = "Error";
                    dResult["Comments"] = "Ollama did not return a SQL statement.";
                    return dResult;
                }
                var sqlEndAt = fullResponse.IndexOf("```", sqlStartAt);
                if (sqlEndAt < 0) sqlEndAt = fullResponse.Length;
                var sql = fullResponse.Substring(sqlStartAt, sqlEndAt - sqlStartAt).Trim();
                dResult["Status"] = "Success";
                fullResponse = fullResponse.Substring(0, sqlStartAt).Trim();
                if (fullResponse.EndsWith("```sql")) { fullResponse = fullResponse.Substring(0, fullResponse.Length - 6).Trim(); }
                dResult["Comments"] = fullResponse;
                dResult["SQL"] = sql;
                return dResult;
            }
            catch (TimeoutRejectedException tre)
            {
                _logger.LogError(tre, "Ollama timed out after policy timeout.");
                dResult["Status"] = "Error";
                dResult["Comments"] = "Ollama generation timed out—prompt too complex or model overload. Try simpler query or optimize schema.";
                return dResult;
            }
            catch (TaskCanceledException tce) when (!ct.IsCancellationRequested)
            {
                // This catches actual timeouts (HttpClient.Timeout exceeded)
                _logger.LogError(tce, "Ollama request timed out after {TimeoutSeconds}s", _ollamaClient.Timeout.TotalSeconds);
                dResult["Status"] = "Error";
                dResult["Comments"] = "Ollama timed out—prompt too complex, service slow, or network issue. Try a simpler query.";
                return dResult;
            }
            catch (TaskCanceledException) when (ct.IsCancellationRequested)
            {
                // Optional: Handle explicit cancellation (e.g., via CancellationToken.Pass to PostAsync)
                _logger.LogWarning("Ollama request was canceled explicitly.");
                dResult["Status"] = "Error";
                dResult["Comments"] = "Ollama request was cancelled.";
                return dResult;
            }
        }
        private async Task<string> GetDetailedSchemaAsync(string userQuery, CancellationToken ct = default)
        {
            try
            {

                if (DateTime.UtcNow < _cacheSchemaExpiry && _cachedSchema != null) return _cachedSchema;

                //var apiClient = _httpClientFactory.CreateClient("PulseApiClient");
                var response = await _pulseApiClient.GetAsync("/AI/schema", ct);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                _logger.LogDebug("Schema JSON: {Json}", json);

                // Deserialize with fallback
                List<SchemaDto> schemas;
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        AllowTrailingCommas = true // Extra safety for malformed JSON
                    };
                    schemas = JsonSerializer.Deserialize<List<SchemaDto>>(json, options) ?? new List<SchemaDto>();
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Deserialization failed - Fallback parsing.");
                    schemas = ParseSchemaFallback(json);
                }

                if (!schemas.Any())
                {
                    _logger.LogWarning("No schemas deserialized.");
                    return "";
                }

                var schemaBuilder = new StringBuilder();
                foreach (var schema in schemas)
                {
                    schemaBuilder.AppendLine($"Entity: {schema.EntityType} (Table: {schema.TableName})");
                    schemaBuilder.AppendLine($"Primary Keys: {(schema.PrimaryKeys != null ? string.Join(", ", schema.PrimaryKeys) : "None")}");

                    schemaBuilder.AppendLine("Columns:");
                    foreach (var col in schema.Columns ?? new List<ColumnDto>())
                    {
                        schemaBuilder.AppendLine($"- {col.Name} (Type: {col.DataType}, Nullable: {col.IsNullable}, PK: {col.IsPrimaryKey})");
                    }

                    schemaBuilder.AppendLine("Relationships:");
                    if (schema.Relationships == null || !schema.Relationships.Any())
                    {
                        schemaBuilder.AppendLine("- None");
                    }
                    else
                    {
                        foreach (var rel in schema.Relationships)
                        {
                            schemaBuilder.AppendLine($"- To {rel.RelatedEntityType} (Table: {rel.RelatedTableName}), Navigation: {rel.NavigationName}, FK Columns: {(rel.ForeignKeyColumns != null ? string.Join(", ", rel.ForeignKeyColumns) : "None")}, Cardinality: {rel.Cardinality}");
                        }
                    }
                    schemaBuilder.AppendLine(); // Separator
                }

                _cachedSchema = schemaBuilder.ToString();
                _cacheSchemaExpiry = DateTime.UtcNow.AddMinutes(120);
                return _cachedSchema;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch or process schema.");
                return "";
            }
        }
        public async Task<string> GetAiExamplesAsync()
        {
            if (DateTime.UtcNow < _cacheExamplesExpiry && _cachedExamples != null) return _cachedExamples;

            //var apiClient = _httpClientFactory.CreateClient("PulseApiClient");
            var response = await _pulseApiClient.GetAsync("/AI/examples");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var aiQueries = JsonSerializer.Deserialize<List<AiQuery>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            string examples = "";
            if (aiQueries != null && aiQueries.Count > 0)
            {
                examples = "\nExamples of correct queries:\n";
                foreach (var ex in aiQueries)
                {
                    examples += $"Question: {ex.Question}\nSQL: {ex.SqlQuery}\n";
                }
            }
            _cachedExamples = examples;
            _cacheExamplesExpiry = DateTime.UtcNow.AddMinutes(60);
            return _cachedExamples;
        }
        public async Task<List<string>> GetOllamaModelsAsync(CancellationToken ct = default)
        {
            //var httpClient = _httpClientFactory.CreateClient("OllamaClient");
            try
            {
                var response = await _ollamaClient.GetAsync("/api/tags", ct);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                var tags = JsonSerializer.Deserialize<OllamaTagsResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return tags?.Models?.Select(m => m.Name).ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch Ollama models - Using fallback.");
                return new List<string> { "llama3.1:latest", "gemma3:27b", "sqlcoder:15b" }; // Fallback static
            }
        }
        public async Task<string> ChatWithOllamaAsync(List<OllamaMessage> historyMessages, string sModel, bool enableSearch = true, CancellationToken ct = default, int qryLoop = 0)
        {
            //var httpClient = _httpClientFactory.CreateClient("OllamaClient");
            //if (!string.IsNullOrEmpty(_ollamaApiKey))
            //{
            //    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ollamaApiKey); // From config/env
            //}
            //else
            //{
            //    throw new InvalidOperationException("Ollama API key is missing in configuration.");
            //}

            if (_CancelQuery)
            {
                _CancelQuery = false;
                return "Ok, I cancelled the query.";
            }

            if (qryLoop >= MAX_TOTAL_QRY_ATTEMPTS)
            {

                return "My apologies, I seem to be caught in a loop. Maybe try and rephrase the question."; //"Error: Maximum query attempts reached. Unable to process the request further.";
            }

            string schemaText = await GetDetailedSchemaAsync("NotNeeded", ct);
            if (string.IsNullOrEmpty(schemaText))
            {
                return "Error : Unable to retrieve database schema information.";
            }

            string examples = await GetAiExamplesAsync();
            // (SELECT only unless {user.UserID} is 1)
            var systemMessage = new OllamaMessage
            {
                Role = "system",
                Content = $"""
                    You are PulseAI, assistant for H&M Rollers[](https://www.hmrollers.com/), covering rollers with Rubber, Polyurethane, etc., for paper, steel, food processing industries. Answer queries on operations, products, services, or general topics using:
                    - Database schema for internal data (e.g., customer counts, roller materials).
                    - 'web_search' for trends/external info; 'web_fetch' for URL content.
                    - 'execute_sql' to run SQL queries .

                    Schema:
                    {schemaText}

                    Examples:
                    {examples}

                    User Question: {historyMessages.Last(m => m.Role == "user")?.Content}

                    **Steps**:
                    1. Check schema for relevant tables (e.g., ClientMaster for customers).
                    2. If DB needed, generate concise SELECT query (use exact table/column names, WHERE for filters, JOIN only if required).
                    3. Run query via 'execute_sql' for SELECT queries 'execute_action_sql' for others.
                    4. If external info needed, use 'web_search' once, then 'web_fetch' if specific URL required.
                    5. After one tool call per type, deliver final answer — no further calls.
                    
                    (Do not include the SQL Query in your response)

                    **Output Format** (always use this structure):
                    
                    **Web Search Results**(if used):
                            1. [Title](URL): Snippet...
                            2. ...

                    Answer: concise response with citations.
                    End with offer to help further.

                    **Rules**:
                    - Use schema for SQL; respect joins, nullability, data types (e.g., LIKE for strings).
                    - Cite web sources as [1], [2]. If no answer, say "I don't know."
                    - One tool call per type; stop after results.
                    """
            };

            //5. **Extract**: If web_search has results, call 'web_fetch' to retrieve more info is needed
            //**Web Search Results**(if used):
            //        1. [Title](URL): / n
            //                Snippet...
            //        2. ...
            // **Output Format** (always use this structure):
            // Final Answer: concise response with citations.
            //        **SQL Query * *(if executed):
            //        Results: **summary of data, e.g., "42 customers start with A." * *
            //```sql
            //        your_sql
            // Final Answer: concise response with citations. End with offer to help further.
            // Results: summary of data, e.g., "42 customers start with A."
            //Content = $"""
            //    You are PulseAI, an AI assistant performing services for H&M Rollers, a company that primarily covers industrial rollers with Rubber, Polyurethane, and other specialized materials.
            //    Website: https://www.hmrollers.com/ . We cover rollers for various industries including paper, steel, food processing, and more. You will be assisting your colleagues by answering 
            //    questions related to our business operations, products, and services as well as general topics.

            //    To enable you to do this effectively, you will rely on your general knowledge, access to web search tools, and querying our internal SQL Server 2022 database. You will decide when 
            //    to use each resource to best answer the user's questions. Sometimes, you may need to use multiple resources in sequence to gather the necessary information. 

            //    -If you need to look up recent information, trends or other information not contained in the database, you can use the 'web_search' tool
            //        to perform web searches and the 'web_fetch' tool to retrieve full content from specific URLs

            //    The user question is:
            //    {historyMessages.Last(m => m.Role == "user")?.Content}

            //    - Check the following schema to see if you need information from the database:
            //    Schema:
            //    {schemaText}

            //    {examples}

            //    If you need information from the database, create a SQL Statement based on the Schema using these guidelines:
            //    - Generate a valid, efficient SQL query (SELECT only, no DDL/DML) based on the Schema and user question.
            //    - Ensure that you use the exact table and column names from the schema.
            //    - Respect relationships for joins
            //    - Handle nullability and data types (e.g., string filters with LIKE, dates with CAST).
            //    - Optimize: Use WHERE for filters, JOINs only if needed, TOP if limiting results.

            //    - Use the 'execute_sql' tool using SQL Query as parameter to run your queries and retrieve results.
            //    - If you require more information to answer a question, you can ask clarifying questions to the user.
            //    - Do not return a SQL Query as the answer. Use these results to formulate accurate and relevant responses to the user's questions.

            //    - If tool results (e.g., web_search) are provided in the conversation, use them to refine your answer.
            //    - **CRITICAL: After tool results, provide the final answer without further tool calls. Do not re-search or re-query the same info.**
            //    - Always aim to provide clear, concise, and accurate information in your responses.
            //    - Remember to cite sources when using web search results to support your answers.
            //    - If you do not know the answer to a question, then say you do not know. Do not make up an answer.
            //    - End your answers with a friendly remark or offer further assistance.
            //    """
            //Content = $"""
            //    You are a knowledgeable AI assistant, designed to have friendly chats with users on a large range of topics.
            //    Sometimes, the user will ask you questions that require querying a SQL Server 2022 database to get accurate answers, if the information is not in your training data,
            //    especially for recent or company-specific data. For these questions, you must generate and execute SQL queries against the database to retrieve the necessary information.
            //    The database to query is named dbPulse and the schema is as follows:
            //    {schemaText}
            //    Go through the schema thoroughly and understand the table relationships (use FK columns) and how the different entities are
            //    structured.You can gain access to the database with the following connection string:
            //    Server=localhost\Dev;Database=dbPulse;Trusted_Connection=True;TrustServerCertificate=True;
            //    Query the database if needed in order to answer the question below. If you need to ask clarifying questions, then do so.
            //    If the question does not pertain to the database or its contents, then use your general knowledge to answer. If needed, use web_search to fetch latest info (e.g., trends)
            //    If you do not know the answer to a question, then say you do not know. Do not make up an answer.
            //    {examples}
            //    When you need to query the database, generate a valid SQL Server 2022 T-SQL query to get the data you need.
            //    Always use SELECT statements only. Do not use DDL or DML statements.
            //    Ensure that you use the exact table and column names from the schema.
            //    When filtering on string columns, use the LIKE operator. When filtering on date columns, use CAST to ensure correct format.
            //    When creating ratios, cast the numerator as float to avoid integer division.
            //    Always optimize your queries to return only the data you need. Use WHERE clauses to filter data, JOINs only when necessary, and TOP to limit results.
            //    If your response includes a SQL Query, ensure it is a valid SELECT string (no \n) then use the 'execute_sql' tool with the SQL Query as the argument
            //    - Do not return a SQL Query as the answer. Use these results to formulate accurate and relevant responses to the user's questions.
            //    The user question is below. Go through it carefully, and then formulate your response
            //    User question: {historyMessages.Last(m => m.Role == "user")?.Content}
            //    """
            //};

            // Combine system + history
            var messages = new List<OllamaMessage> { systemMessage };
            messages.AddRange(historyMessages);

            // Define tools (web_search and web_fetch)
            var tools = new List<object>();


            if (enableSearch)
            {
                tools.Add(new
                {
                    type = "function",
                    function = new
                    {
                        name = "web_search",
                        description = "Search the web for current information.",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                query = new { type = "string", description = "Search query" }
                            },
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
                        description = "Fetch full content from a URL.",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                url = new { type = "string", description = "URL to fetch" }
                            },
                            required = new[] { "url" }
                        }
                    }
                });
            }
            tools.Add(new
            {
                type = "function",
                function = new
                {
                    name = "execute_sql",
                    description = "Execute a SQL query against the dbPulse database and return results.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            sql = new { type = "string", description = "The SQL query to execute" }
                        },
                        required = new[] { "sql" }
                    }
                }
            });
            tools.Add(new
            {
                type = "function",
                function = new
                {
                    name = "execute_action_sql",
                    description = "Execute a SQL query against the dbPulse database and return results.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            sql = new { type = "string", description = "The SQL query to execute" }
                        },
                        required = new[] { "sql" }
                    }
                }
            });
            var canThink = false;
            if (sModel == "gpt-oss:latest" || sModel == "qwen3:32b" || sModel == "deepseek-r1:14b") { canThink = true; }
            var ollamaRequest = new
            {
                model = sModel,
                messages = messages.ToArray(),
                tools = tools.ToArray(),
                think = canThink, // Enable reasoning/tool calls
                stream = false,
                options = new { temperature = 0.0, num_ctx = 32000 } // num_predict = 8000 }
            };

            var jsonContent = JsonSerializer.Serialize(ollamaRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                _logger.LogInformation("Starting Ollama chat for query: {Query}", historyMessages.Last().Content);
                var response = await _ollamaClient.PostAsync("/api/chat", content, ct);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync(ct);

                // Deserialization with fallback
                OllamaChatResponse ollamaResponse;
                try
                {
                    ollamaResponse = JsonSerializer.Deserialize<OllamaChatResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException)
                {
                    // Fallback: Parse with JsonDocument for robust handling
                    using var doc = JsonDocument.Parse(responseJson);
                    var root = doc.RootElement;

                    ollamaResponse = new OllamaChatResponse
                    {
                        Model = root.TryGetProperty("model", out var modelProp) ? modelProp.GetString() : null,
                        Done = root.TryGetProperty("done", out var doneProp) ? doneProp.GetBoolean() : false,
                        Message = root.TryGetProperty("message", out var msgProp) ? new OllamaMessage
                        {
                            Role = msgProp.TryGetProperty("role", out var roleProp) ? roleProp.GetString() : null,
                            Content = msgProp.TryGetProperty("content", out var contentProp) ? contentProp.GetString() : null,
                            ToolCalls = msgProp.TryGetProperty("tool_calls", out var toolProp) && toolProp.ValueKind == JsonValueKind.Array ?
                                toolProp.EnumerateArray().Select(t => new OllamaToolCall
                                {
                                    Function = new OllamaFunctionCall
                                    {
                                        Name = t.TryGetProperty("function", out var func) ? (func.TryGetProperty("name", out var name) ? name.GetString() : null) : null,
                                        Arguments = func.TryGetProperty("arguments", out var args) ? args : default(JsonElement)
                                    }
                                }).ToList() : null
                        } : null
                    };

                    if (ollamaResponse == null || string.IsNullOrEmpty(ollamaResponse.Message?.Content))
                    {
                        throw new Exception("Fallback deserialization failed - Ollama response malformed.");
                    }
                }




                // Handle tool calls if any (recursive for multi-turn)
                if (ollamaResponse.Message.ToolCalls != null && ollamaResponse.Message.ToolCalls.Any())
                {
                    await HandleToolCalls(ollamaResponse.Message.ToolCalls, messages, sModel, ct);
                    // Re-call chat with updated messages for final response
                    return await ChatWithOllamaAsync(messages, sModel, enableSearch, ct, qryLoop + 1);
                }
                _logger.LogInformation("Ollama chat completed.");
                return ollamaResponse.Message.Content.Trim();
            }
            catch (TaskCanceledException tce) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(tce, "Ollama chat timed out.");
                throw new Exception("Ollama chat timed out—prompt too complex or service slow.");
            }
        }
        private async Task HandleToolCalls(List<OllamaToolCall> toolCalls, List<OllamaMessage> messages, string model, CancellationToken ct)
        {
            foreach (var toolCall in toolCalls)
            {
                var arguments = toolCall.Function.Arguments; // JsonElement
                var toolResult = toolCall.Function.Name switch
                {
                    "execute_sql" => await ExecuteSqlToolAsync(arguments, ct),
                    "execute_action_sql" => await ExecuteActionSqlToolAsync(arguments, ct),
                    "web_search" => await WebSearchAsync(arguments, ct),
                    "web_fetch" => await WebFetchAsync(arguments, ct),
                    _ => "Unknown tool"
                };

                if (string.IsNullOrEmpty(toolResult) || toolResult == "Unknown tool")
                {
                    _toolAttempts++;
                    _toolAttemptName = toolCall.Function.Name;
                }
                else
                {
                    // Append tool result as message
                    _toolAttempts = 0; // Reset on success
                    _toolAttemptName = "";
                }
                messages.Add(new OllamaMessage
                {
                    Role = "tool",
                    Content = toolResult,
                    ToolName = toolCall.Function.Name
                });
            }
        }

        private async Task<string> ExecuteSqlToolAsync(JsonElement args, CancellationToken ct)
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
                _logger.LogInformation("Executing SQL tool: {Sql}", sql);
                // TESTING PUTTING THIS BACK
                //sql = sql.Replace("/n", " ");
                var response = await _pulseApiClient.GetAsync($"/AI/execute:{sql}", ct);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                return json; // Return raw JSON data for Ollama to use in next turn
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
                //sql = sql.Replace("/n", " ");
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
        private async Task<string> WebSearchAsync(JsonElement args, CancellationToken ct)
        {
            string query = args.TryGetProperty("query", out var queryProp) ? queryProp.GetString() ?? "" : "";
            if (string.IsNullOrEmpty(query))
            {
                return "Error: Missing 'query' argument for web_search.";
            }

            //var httpClient = _httpClientFactory.CreateClient("OllamaClient");
            //httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ollamaApiKey); // Ensure key sent

            var searchRequest = new { query };
            var jsonContent = JsonSerializer.Serialize(searchRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                _logger.LogInformation("Calling Ollama web_search with query: {Query}", query);
                var response = await _ollamaClient.PostAsync("https://ollama.com/api/web_search", content, ct); // Cloud API
                response.EnsureSuccessStatusCode();
                var searchJson = await response.Content.ReadAsStringAsync(ct);

                // Deserialization with case-insensitive options
                OllamaWebSearchResponse searchResponse;
                try
                {
                    searchResponse = JsonSerializer.Deserialize<OllamaWebSearchResponse>(searchJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException)
                {
                    // Fallback: Parse with JsonDocument if standard fails
                    using var doc = JsonDocument.Parse(searchJson);
                    var root = doc.RootElement;

                    searchResponse = new OllamaWebSearchResponse
                    {
                        Results = root.TryGetProperty("results", out var resultsProp) && resultsProp.ValueKind == JsonValueKind.Array
                            ? resultsProp.EnumerateArray().Select(r => new OllamaWebSearchResult
                            {
                                Title = r.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null,
                                Url = r.TryGetProperty("url", out var urlProp) ? urlProp.GetString() : null,
                                Snippet = r.TryGetProperty("snippet", out var snippetProp) ? snippetProp.GetString() : null
                            }).ToList()
                            : null
                    };
                }

                if (searchResponse?.Results == null || !searchResponse.Results.Any())
                {
                    _logger.LogWarning("Web search returned null or empty results for query: {Query}", query);
                    return "No web search results found.";
                }

                // Format results text
                //var resultText = string.Join("\n", searchResponse.Results.Select(r => $"{r.Title}: {r.Snippet} ({r.Url})"));
                var resultText = string.Join("|", searchResponse.Results.Select(r => $"[{r.Title}]({r.Url}): {r.Snippet}"));
                return resultText.Length > 8000 ? resultText.Substring(0, 8000) + "..." : resultText;
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Web search HTTP error - Check API key or network.");
                return $"Error in web search: {hre.Message}. Ensure Ollama API key is valid.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in web search.");
                return $"Error in web search: {ex.Message}";
            }
        }

        private async Task<string> WebFetchAsync(JsonElement args, CancellationToken ct)
        {
            string url = args.TryGetProperty("url", out var urlProp) ? urlProp.GetString() ?? "" : "";
            if (string.IsNullOrEmpty(url))
            {
                return "Error: Missing 'url' argument for web_fetch.";
            }
            //var httpClient = _httpClientFactory.CreateClient("OllamaClient");
            //httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ollamaApiKey);

            var fetchRequest = new { url };
            var jsonContent = JsonSerializer.Serialize(fetchRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _ollamaClient.PostAsync("https://ollama.com/api/web_fetch", content, ct);
            response.EnsureSuccessStatusCode();
            var fetchJson = await response.Content.ReadAsStringAsync(ct);
            var fetchResponse = JsonSerializer.Deserialize<OllamaWebFetchResponse>(fetchJson);

            var contentText = fetchResponse.Content ?? "";
            return contentText.Length > 8000 ? contentText.Substring(0, 8000) + "..." : contentText;
        }

        public async Task<List<ClientSales>> GetClientSalesAsync(string clientID, CancellationToken ct = default)
        {
            List<ClientSales> fallback = new();

            try
            {
                // Example endpoint call - Adjust to your actual API (e.g., /api/sales/{clientID})
                var response = await _pulseApiClient.GetAsync($"/Customers/Details/{Uri.EscapeDataString(clientID)}/Sales", ct);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("API request failed: Status {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    return fallback;
                }

                var json = await response.Content.ReadAsStringAsync(ct);
                var sales = JsonSerializer.Deserialize<List<ClientSales>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (sales == null || !sales.Any())
                {
                    return fallback;
                }
                return sales;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetClientSalesAsync.");
                return fallback;
            }
        }

        public async Task<ClientCurrentStats> GetClientStatsAsync(string fullclientID, CancellationToken ct = default)
        {
            ClientCurrentStats fallback = new();

            try
            {
                // Example endpoint call - Adjust to your actual API (e.g., /api/sales/{clientID})
                var response = await _pulseApiClient.GetAsync($"/Customers/Details/{Uri.EscapeDataString(fullclientID)}/CurrentStats", ct);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("API request failed: Status {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    return fallback;
                }

                var json = await response.Content.ReadAsStringAsync(ct);
                var stats = JsonSerializer.Deserialize<ClientCurrentStats>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (stats == null)
                {
                    return fallback;
                }
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetClientStatsAsync.");
                return fallback;
            }
        }
        public async Task<List<ClientRollerSpecification>> GetClientRollerSpecificationsAsync(string clientID, CancellationToken ct = default)
        {
            List<ClientRollerSpecification> fallback = new();

            try
            {
                // Example endpoint call - Adjust to your actual API (e.g., /api/sales/{clientID})
                var response = await _pulseApiClient.GetAsync($"/Customers/Details/{Uri.EscapeDataString(clientID)}/RollerSpecifications", ct);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("API request failed: Status {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    return fallback;
                }

                var json = await response.Content.ReadAsStringAsync(ct);
                var specs = JsonSerializer.Deserialize<List<ClientRollerSpecification>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (specs == null || !specs.Any())
                {
                    return fallback;
                }
                return specs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetClientRollerSpecificationsAsync.");
                return fallback;
            }
        }
        public async Task<string> AskPulseAIAsync(string prompt, string sModel, CancellationToken ct = default)
        {
            //var httpClient = _httpClientFactory.CreateClient("PulseApiClient");

            try
            {
                //var dataText = JsonSerializer.Serialize(sales, new JsonSerializerOptions { WriteIndented = true });

                //var prompt = $"""
                //You will be given the sales history for a customer. You are a financial analysist and will examine the data and provide
                //meaninful responses to the user query provided.

                //The query will include the customers name which is to be used when referencing the customer, not the client id.
                //The data also includes a sub-object providing details on the period. When referencing periods use the month-calenderyear
                //not the PeriodID. If the user seems to be Refering to Financial Year then use the Financial Year, state in your response which year
                //field you are using.
            
                //Unless it is specifically asked for in the query, do not include the period sales amounts, just the analysis.
                //Note: All Sales Amounts are in ZAR (South African Rands)

                //Data:
                //{dataText}

                //Query: {userQuery}
                //""";

                var model = sModel;
                var ollamaRequest = new { model, prompt, stream = false };
                var jsonContent = JsonSerializer.Serialize(ollamaRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var ollamaResponse = await _ollamaClient.PostAsync("http://localhost:11434/api/generate", content, ct);
                if (!ollamaResponse.IsSuccessStatusCode)
                {
                    var ollamaError = await ollamaResponse.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Ollama request failed: {Error}", ollamaError);
                    return $"Error from Ollama: {ollamaError}";
                }

                var responseJson = await ollamaResponse.Content.ReadAsStringAsync(ct);
                var ollamaResult = JsonSerializer.Deserialize<OllamaGenerateResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return ollamaResult?.Response?.Trim() ?? "No analysis from Ollama.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetClientSalesAnalysisAsync.");
                return $"Error: {ex.Message}";
            }
        }

        private async Task<List<SchemaDto>> GetSchemaFromApiAsync(CancellationToken ct = default)
        {
            //var apiClient = _httpClientFactory.CreateClient("PulseApiClient");
            try
            {
                var response = await _pulseApiClient.GetAsync("/AI/schema", ct);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(ct);
                _logger.LogDebug("Fetched schema JSON: {Json}", json);

                List<SchemaDto> schemas;
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        AllowTrailingCommas = true // Extra safety for malformed JSON
                    };
                    schemas = JsonSerializer.Deserialize<List<SchemaDto>>(json, options) ?? new List<SchemaDto>();
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Deserialization failed - Fallback parsing.");
                    schemas = ParseSchemaFallback(json);
                }

                if (!schemas.Any())
                {
                    _logger.LogWarning("Deserialized schema is empty - Check API endpoint or JSON format.");
                }

                return schemas;
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Schema API call failed.");
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