using Pulse.Models.CustomComponents;
using Pulse.Models.Misc;
using Pulse.Web.Tools;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;


namespace Pulse.Web.Services
{
    internal sealed class OllamaService(HttpClient httpOllama, ILogger<OllamaService> logger)
    {
        private readonly HttpClient _httpOllama = httpOllama;
        private readonly ILogger<OllamaService> _logger = logger;
        public async Task<T?> PostAsync<T>(string requestUri, object payload)
        {
            try
            {
                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpOllama.PostAsync(requestUri, content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while accessing {RequestUri}", requestUri);
                return default;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while processing response from {RequestUri}", requestUri);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while accessing {RequestUri}", requestUri);
                return default;
            }
        }
        public async Task<T?> GetAsync<T>(string requestUri)
        {
            try
            {
                var response = await _httpOllama.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while accessing {RequestUri}", requestUri);
                return default;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while processing response from {RequestUri}", requestUri);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while accessing {RequestUri}", requestUri);
                return default;
            }
        }
        public async Task<string> GenerateSQLQueryAsync(string strModel, string schema, string examples, string naturalLanguageQuery)
        {

            //var prompt = $@"
            //    You are an expert in SQL Server 2022 T-SQL. Given this schema:
            //    {schema}

            //    {examples}

            //    User question: {naturalLanguageQuery}

            //    Generate a valid SQL Server 2022 SELECT query in plain text. Return ONLY the T-SQL query.
            //    Use schema for accurate joins/columns.
            //    ";

            var strPrompt = $"""
                    You are a SQL expert for a SQL Server 2022 database. Generate a valid, efficient SQL query (SELECT only, no DDL/DML) based on the following schema and user question.
                    - Use exact table/column names.
                    - Respect relationships for joins (e.g., use FK columns).
                    - Handle nullability and data types (e.g., string filters with LIKE, dates with CAST).
                    - Optimize: Use WHERE for filters, JOINs only if needed, TOP if limiting results.
                    - Prefix the SQL statement with [SQL Start] and End with [SQL End].

                    Database Schema:
                    {schema}

                    {examples}

                    User Question: {naturalLanguageQuery}
                    """;

            var requestBody = new
            {
                //model = ai.ModelName,
                //prompt = ai.Prompt,
                model = strModel,
                prompt = strPrompt,
                stream = false,
                max_tokens = 512,
                temperature = 0.0
            };

            try
            {
                var jsonRequest = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");
                //var response = await _httpOllama.PostAsJsonAsync("/api/generate", requestBody);
                var response = await _httpOllama.PostAsync("/api/generate", httpContent);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                var ollamaResponse = JsonSerializer.Deserialize<OllamaGenerateResponse>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (ollamaResponse == null || !ollamaResponse.Done)
                {
                    throw new Exception("Ollama generation incomplete or failed.");
                }

                // Extract and trim the generated SQL
                return ollamaResponse.Response?.Trim() ?? "Error: Empty response from Ollama.";
            }
            //if (!response.IsSuccessStatusCode)
            //{
            //    var errorContent = await response.Content.ReadAsStringAsync();
            //    return $"Error: Ollama returned status {response.StatusCode}. Details: {errorContent}";
            //}

            //var responseContent = await response.Content.ReadFromJsonAsync<JsonElement>();
            //var rawResponse = responseContent.GetProperty("response").GetString()?.Trim();

            //if (string.IsNullOrWhiteSpace(rawResponse))
            //{
            //    return "Error: Generated SQL query is empty.";
            //}

            ////var match = Regex.Match(rawResponse, @"^\s*SELECT\s+.*?(?:;|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            ////var sqlQuery = match.Success ? match.Value.Trim() : null;
            //var sqlQuery = rawResponse;
            //if (sqlQuery == null)
            //    return $"Error: Generated query is not a valid SELECT statement.\nGenerated: {rawResponse}";
            //if (!sqlQuery.EndsWith(";")) sqlQuery += ";";

            //return $"Generated SQL: {sqlQuery}";
            //}
            
            catch (HttpRequestException httpEx)
            {
                // Handle Ollama offline, network issues
                throw new Exception($"Ollama connection error: {httpEx.Message}. Ensure Ollama is running on localhost:11434.");
            }
            catch (JsonException jsonEx)
            {
                // Handle malformed response
                throw new Exception($"Ollama response parsing error: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                return $"Error: General: {ex.Message}";
            }
        }
    }
}
