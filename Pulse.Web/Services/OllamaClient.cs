using Pulse.Models.CustomComponents;
using Pulse.Models.Misc;
using Pulse.Web.Tools;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;


namespace Pulse.Web.Services
{
    public sealed class OllamaService
    {
        private readonly HttpClient _httpOllama;
        private readonly ILogger<OllamaService> _logger;
        private readonly string _ollamaApiKey;
        private readonly string _localBaseAddress = "http://localhost:11434"; // Your local Ollama instance
        private readonly string _cloudBaseAddress = "https://ollama.com"; // For web_search, etc.

        public OllamaService(HttpClient httpOllama, ILogger<OllamaService> logger, IConfiguration configuration)
        {
            _ollamaApiKey = configuration["OllamaApi:Key"] ?? string.Empty; // Already in your config
            httpOllama.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ollamaApiKey);
            _httpOllama = httpOllama;
            // _httpOllama.BaseAddress = new Uri(_localBaseAddress); // Default to local
            _logger = logger;
            
        }
        public async Task<T?> PostAsync<T>(string requestUri, object payload, CancellationToken ct = default)
        {
            //ConfigureClientForEndpoint(requestUri);
            try
            {
                var jsonPayload = JsonSerializer.Serialize(payload);
                using var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                using var response = await _httpOllama.PostAsync(requestUri, content, ct);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while accessing {RequestUri}", requestUri);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while processing response from {RequestUri}", requestUri);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while accessing {RequestUri}", requestUri);
                throw;
            }
        }
        public async Task<T?> GetAsync<T>(string requestUri, CancellationToken ct = default)
        {
            //ConfigureClientForEndpoint(requestUri);
            try
            {
                using var response = await _httpOllama.GetAsync(requestUri, ct);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while accessing {RequestUri}", requestUri);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while processing response from {RequestUri}", requestUri);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while accessing {RequestUri}", requestUri);
                throw;
            }
        }

        private void ConfigureClientForEndpoint(string endpoint)
        {
            if (endpoint.StartsWith("https://ollama.com", StringComparison.OrdinalIgnoreCase))
            {
                _httpOllama.BaseAddress = new Uri(_cloudBaseAddress);
                if (!string.IsNullOrEmpty(_ollamaApiKey))
                {
                    _httpOllama.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ollamaApiKey);
                }
                else
                {
                    _logger.LogWarning("Ollama API key missing for cloud endpoint {Endpoint}", endpoint);
                }
            }
            else
            {
                _httpOllama.BaseAddress = new Uri(_localBaseAddress);
                _httpOllama.DefaultRequestHeaders.Authorization = null; // Clear for local
            }
        }
        public async Task<string> GenerateSQLQueryAsync(string strModel, string schema, string examples, string naturalLanguageQuery, CancellationToken ct = default)
        {
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
                {schema}
                {examples}
                User Question:
                {naturalLanguageQuery}
                """;

            var requestBody = new
            {
                model = strModel,
                prompt = strPrompt,
                stream = false,
                options = new { temperature = 0.0, num_predict = 8000 }
            };

            var ollamaResponse = await PostAsync<OllamaGenerateResponse>("/api/generate", requestBody, ct);
            if (ollamaResponse == null || !ollamaResponse.Done || string.IsNullOrEmpty(ollamaResponse.Response))
            {
                throw new InvalidOperationException("Ollama generation incomplete or empty.");
            }

            var fullResponse = ollamaResponse.Response.Trim();

            // Extract SQL: First try [SQL Start]/[SQL End], then fallback to ```sql
            var sqlStart = fullResponse.IndexOf("[SQL Start]");
            if (sqlStart >= 0)
            {
                sqlStart += "[SQL Start]".Length;
                var sqlEnd = fullResponse.IndexOf("[SQL End]", sqlStart);
                if (sqlEnd > sqlStart)
                {
                    return fullResponse.Substring(sqlStart, sqlEnd - sqlStart).Trim();
                }
            }

            // Fallback to markdown or plain SELECT
            sqlStart = fullResponse.IndexOf("```sql");
            if (sqlStart >= 0)
            {
                sqlStart += 6; // Skip ```sql
                var sqlEnd = fullResponse.IndexOf("```", sqlStart);
                if (sqlEnd > sqlStart)
                {
                    return fullResponse.Substring(sqlStart, sqlEnd - sqlStart).Trim();
                }
            }

            // Regex fallback for plain SELECT
            var match = Regex.Match(fullResponse, @"SELECT\s+.*?(?:;|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);
            if (match.Success)
            {
                return match.Value.Trim();
            }

            throw new InvalidOperationException($"No valid SQL found in Ollama response: {fullResponse}");
        }
    }
}