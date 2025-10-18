using Microsoft.Extensions.Options;
using Polly.Timeout;
using Pulse.Models.CustomComponents;
using Pulse.Models.Misc;
using Pulse.Web.Tools;
using StackExchange.Redis;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Pulse.Web.Services
{
    public class Pulse_AI
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Pulse_AI> _logger; // Optional for logging
        private static string? _cachedSchema;
        private static DateTime _cacheExpiry = DateTime.MinValue;

        public Pulse_AI(IHttpClientFactory httpClientFactory, ILogger<Pulse_AI> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

        }
        public async Task<Dictionary<string, string>> GetSQLFromOllamaAsync(string userQuery, string model, CancellationToken ct = default)
        {
            Dictionary<string, string> dResult = new()
            {
                { "Status", "" },
                { "Comments", "" },
                { "SQL", "" }
            };

            var httpClient = _httpClientFactory.CreateClient("OllamaClient");

            string schemaText = await GetDetailedSchemaAsync(ct);
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
            var prompt = $"""
            You are a SQL expert for a SQL Server 2022 database. Generate a valid, efficient SQL query (SELECT only, no DDL/DML) based on the following schema and user question.
            - Use exact table/column names.
            - Respect relationships for joins (e.g., use FK columns).
            - Handle nullability and data types (e.g., string filters with LIKE, dates with CAST).
            - Optimize: Use WHERE for filters, JOINs only if needed, TOP if limiting results.
            - Format DateTime literals as 'DD-MM-YY'.
            - Format All Numberic Values as ###,###.##
            - Return only one SQL Statement as the final answer.
            - All comments, explanations, reasoning must come before the SQL Statement in the response.
            - The Query must start with the SELECT keyword only
            - Do not include any other text after the SQL Statement.
            - The SQL Statement must be valid T-SQL for SQL Server 2022.
            - The SQL Statement must be the last part of the response.

            Database Schema:
            {schemaText}

            {examples}

            {isRetry}
            User Question: {userQuery}
            """;

            var ollamaRequest = new
            {
                model = model,
                prompt = prompt,
                stream = false,
                options = new { temperature = 0.0, num_predict = 2000 }
            };

            var jsonContent = JsonSerializer.Serialize(ollamaRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync("/api/generate", content, ct);
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
                if( sqlStartAt < 0)
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
                _logger.LogError(tce, "Ollama request timed out after {TimeoutSeconds}s", httpClient.Timeout.TotalSeconds);
                dResult["Status"] = "Error";
                dResult["Comments"] = "Ollama generation timed out—prompt too complex or model overload. Try simpler query or optimize schema.";
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
        private async Task<string> GetDetailedSchemaAsync(CancellationToken ct)
        {
            try
            {


                if (DateTime.UtcNow < _cacheExpiry && _cachedSchema != null) return _cachedSchema;

                var apiClient = _httpClientFactory.CreateClient("PulseApiClient");
                var response = await apiClient.GetAsync("/AI/schema");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var schemas = JsonSerializer.Deserialize<List<SchemaDto>>(json);

                var schemaBuilder = new StringBuilder();

                foreach (var schema in schemas)
                {
                    schemaBuilder.AppendLine($"Entity: {schema.EntityType} (Table: {schema.TableName})");
                    schemaBuilder.AppendLine($"Primary Keys: {string.Join(", ", schema.PrimaryKeys)}");

                    schemaBuilder.AppendLine("Columns:");
                    foreach (var col in schema.Columns)
                    {
                        schemaBuilder.AppendLine($"- {col.Name} (Type: {col.DataType}, Nullable: {col.IsNullable}, PK: {col.IsPrimaryKey})");
                    }

                    schemaBuilder.AppendLine("Relationships:");
                    foreach (var rel in schema.Relationships)
                    {
                        schemaBuilder.AppendLine($"- To {rel.RelatedEntityType} (Table: {rel.RelatedTableName}), Navigation: {rel.NavigationName}, FK Columns: {string.Join(", ", rel.ForeignKeyColumns)}, Cardinality: {rel.Cardinality}");
                    }
                    schemaBuilder.AppendLine(); // Separator
                }

                _cachedSchema = schemaBuilder.ToString();
                _cacheExpiry = DateTime.UtcNow.AddMinutes(30); // Refresh interval
                return _cachedSchema;
            }
            catch (TimeoutRejectedException tre)
            {
                _logger.LogError(tre, "Ollama timed out after policy timeout.");
                throw new Exception("Ollama generation timed out—prompt too complex or model overload. Try simpler query or optimize schema.");
            }
            catch (TaskCanceledException tce) when (!ct.IsCancellationRequested)
            {
                // This catches actual timeouts (HttpClient.Timeout exceeded)
                _logger.LogError(tce, "Ollama request timed out ");
                throw new Exception("Ollama timed out—prompt too complex, service slow, or network issue. Try a simpler query.");
            }
            catch (TaskCanceledException) when (ct.IsCancellationRequested)
            {
                // Optional: Handle explicit cancellation (e.g., via CancellationToken.Pass to PostAsync)
                _logger.LogWarning("Ollama request was canceled explicitly.");
                throw new Exception("Ollama request canceled.");
            }
        }
        public async Task<string> GetAiExamplesAsync()
        {
            var apiClient = _httpClientFactory.CreateClient("PulseApiClient");
            var response = await apiClient.GetAsync("/AI/examples");
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
            return examples;
        }
        public async Task<List<string>> GetOllamaModelsAsync(CancellationToken ct = default)
        {
            var httpClient = _httpClientFactory.CreateClient("OllamaClient");
            try
            {
                var response = await httpClient.GetAsync("/api/tags", ct);
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
        public async Task<string> ChatWithOllamaAsync(List<OllamaMessage> historyMessages, string model, CancellationToken ct = default)
        {
            var httpClient = _httpClientFactory.CreateClient("OllamaClient");
            var webSearchApiKey = "76c15b3643ba413aac7429bbb64e119a._BO65S6KcVBYl9PpsPwSc5WI";

            string schemaText = await GetDetailedSchemaAsync(ct);
            if (string.IsNullOrEmpty(schemaText))
            {
                return "Error : Unable to retrieve database schema information.";
            }

            string examples = await GetAiExamplesAsync();

            var systemMessage = new OllamaMessage
            {
                Role = "system",
                Content = $"""
                    You are a knowledgeable AI assistant, designed to have friendly chats with users on a large range of topics.
                    Sometimes, the user will ask you questions that require querying a SQL Server 2022 database to get accurate answers, if the information is not in your training data,
                    especially for recent or company-specific data. For these questions, you must generate and execute SQL queries against the database to retrieve the necessary information.
                    The database to query is named dbPulse and the schema is as follows:
                    {schemaText}
                    Go through the schema thoroughly and understand the table relationships (use FK columns) and how the different entities are
                    structured.You can gain access to the database with the following connection string:
                    Server=localhost\Dev;Database=dbPulse;Trusted_Connection=True;TrustServerCertificate=True;
                    Query the database if needed in order to answer the question below. If you need to ask clarifying questions, then do so.
                    If the question does not pertain to the database or its contents, then use your general knowledge to answer.
                    If you do not know the answer to a question, then say you do not know. Do not make up an answer.
                    {examples}
                    When you need to query the database, generate a valid SQL Server 2022 T-SQL query to get the data you need.
                    Always use SELECT statements only. Do not use DDL or DML statements.
                    Ensure that you use the exact table and column names from the schema.
                    When filtering on string columns, use the LIKE operator. When filtering on date columns, use CAST to ensure correct format.
                    When creating ratios, cast the numerator as float to avoid integer division.
                    Always optimize your queries to return only the data you need. Use WHERE clauses to filter data, JOINs only when necessary, and TOP to limit results.
                    The user question is below. Go through it carefully, and then formulate your response.
                    User question: {historyMessages.Last().Content}
                    """
            };

            // Combine system + history
            var messages = new List<OllamaMessage> { systemMessage };
            messages.AddRange(historyMessages);

            var ollamaRequest = new
            {
                model,
                messages = messages.ToArray(),
                stream = false,
                options = new { temperature = 0.0, num_predict = 512 }
            };

            var jsonContent = JsonSerializer.Serialize(ollamaRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                _logger.LogInformation("Starting Ollama chat for query: {Query}", historyMessages.Last().Content);
                var response = await httpClient.PostAsync("/api/chat", content, ct);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync(ct);
                var ollamaResponse = JsonSerializer.Deserialize<OllamaChatResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (ollamaResponse == null || string.IsNullOrEmpty(ollamaResponse.Message?.Content))
                {
                    throw new Exception("Ollama chat incomplete or empty.");
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
    }
}
