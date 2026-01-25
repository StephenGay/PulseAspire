using Pulse.Models.CustomComponents;
using Pulse.Models.Misc;
using Pulse.Web.Tools;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Runtime.CompilerServices;
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
        private readonly Uri _localBaseAddress;
        private readonly Uri _cloudBaseAddress;
        private readonly JsonSerializerOptions _jsonOptions;

        // Cache model availability to avoid repeated API calls
        private static readonly ConcurrentDictionary<string, bool> ModelCache = new();

        public OllamaService(HttpClient httpOllama, ILogger<OllamaService> logger, IConfiguration configuration)
        {
            _ollamaApiKey = configuration["OllamaApi:Key"] ?? string.Empty;
            _localBaseAddress = new Uri(configuration["OllamaApi:EndpointHttp"]
                ?? throw new InvalidOperationException("Missing OllamaApi:EndpointHttp configuration"));
            _cloudBaseAddress = new Uri("https://ollama.com");

            _httpOllama = httpOllama ?? throw new ArgumentNullException(nameof(httpOllama));
            _httpOllama.BaseAddress = _localBaseAddress; // Default to local

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            _logger = logger;
        }

        /// <summary>
        /// Validates that the Ollama service is reachable and a model is available.
        /// Call this at startup to fail fast if Ollama isn't running.
        /// </summary>
        public async Task<bool> ValidateConnectionAsync(string modelName, CancellationToken ct = default)
        {
            try
            {
                var response = await GetAsync<OllamaTagsResponse>("/api/tags", ct);
                var modelExists = response?.Models?.Any(m => m.Name == modelName) ?? false;

                if (!modelExists)
                {
                    _logger.LogWarning("Model '{ModelName}' not found in Ollama. Available models: {Models}",
                        modelName, string.Join(", ", response?.Models?.Select(m => m.Name) ?? Enumerable.Empty<string>()));
                    return false;
                }

                _logger.LogInformation("Ollama connection validated. Model '{ModelName}' available.", modelName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate Ollama connection");
                return false;
            }
        }

        private void ConfigureRequestHeaders(HttpRequestMessage request)
        {
            // Convert relative URI to absolute using HttpClient's BaseAddress
            var absoluteUri = request.RequestUri?.IsAbsoluteUri == true
                ? request.RequestUri
                : new Uri(_httpOllama.BaseAddress, request.RequestUri);

            var isCloudEndpoint = absoluteUri?.Host.Contains("ollama.com", StringComparison.OrdinalIgnoreCase) ?? false;

            if (isCloudEndpoint && !string.IsNullOrEmpty(_ollamaApiKey))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ollamaApiKey);
                _logger.LogDebug("Added API key header for cloud endpoint");
            }
            else if (isCloudEndpoint)
            {
                _logger.LogWarning("Cloud endpoint requested but no API key configured");
            }
        }
        public async Task<T?> PostAsync<T>(string requestUri, object payload, CancellationToken ct = default)
    where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(requestUri);
            ArgumentNullException.ThrowIfNull(payload);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
                var jsonPayload = JsonSerializer.Serialize(payload, _jsonOptions);
                request.Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                // Add API key for cloud endpoints
                ConfigureRequestHeaders(request);

                _logger.LogDebug("POST request to {RequestUri}", requestUri);
                using var response = await _httpOllama.SendAsync(request, ct);

                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync(ct);

                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for {RequestUri}: {StatusCode}",
                    requestUri, ex.StatusCode);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from {RequestUri}", requestUri);
                throw;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request to {RequestUri} was cancelled", requestUri);
                throw;
            }
        }

        public async Task<T?> GetAsync<T>(string requestUri, CancellationToken ct = default)
        where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(requestUri);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                // Add API key for cloud endpoints
                ConfigureRequestHeaders(request);

                _logger.LogDebug("GET request to {RequestUri}", requestUri);
                using var response = await _httpOllama.SendAsync(request, ct);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(ct);

                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for {RequestUri}: {StatusCode}",
                    requestUri, ex.StatusCode);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from {RequestUri}", requestUri);
                throw;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request to {RequestUri} was cancelled", requestUri);
                throw;
            }
        }


        public async IAsyncEnumerable<string> PostStreamAsync(
            string requestUri,
            object payload,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(requestUri);
            ArgumentNullException.ThrowIfNull(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            var jsonPayload = JsonSerializer.Serialize(payload, _jsonOptions);
            request.Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            // Configure headers for cloud endpoints
            ConfigureRequestHeaders(request);

            var response = await _httpOllama.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            string? line;
            while ((line = await reader.ReadLineAsync(ct)) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                OllamaGenerateResponse? chunk;
                try
                {
                    chunk = JsonSerializer.Deserialize<OllamaGenerateResponse>(line, _jsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize Ollama chunk");
                    throw;
                }

                if (chunk != null)
                {
                    yield return chunk.Response;

                    if (chunk.Done)
                    {
                        _logger.LogInformation("Stream complete: {EvalCount} tokens in {TotalDuration}ms",
                            chunk.EvalCount, chunk.TotalDuration);
                        yield break;
                    }
                }
            }
        }

        public async IAsyncEnumerable<OllamaChatChunk> ChatStreamAsync(
            string requestUri,
            object payload,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(requestUri);
            ArgumentNullException.ThrowIfNull(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            var jsonPayload = JsonSerializer.Serialize(payload, _jsonOptions);
            request.Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            // Configure headers for cloud endpoints
            ConfigureRequestHeaders(request);

            var response = await _httpOllama.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            string? line;
            while ((line = await reader.ReadLineAsync(ct)) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                OllamaChatChunk? chunk;
                try
                {
                    chunk = JsonSerializer.Deserialize<OllamaChatChunk>(line, _jsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize chat chunk");
                    throw;
                }

                if (chunk != null)
                {
                    yield return chunk;

                    if (chunk.Done)
                    {
                        _logger.LogInformation("Chat stream complete: {TotalDuration}ms", chunk.TotalDuration);
                        yield break;
                    }
                }
            }
        }

        public async Task<string> GenerateSQLQueryAsync(
            string model,
            string schema,
            string examples,
            string naturalLanguageQuery,
            CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(model);
            ArgumentException.ThrowIfNullOrEmpty(schema);
            ArgumentException.ThrowIfNullOrEmpty(naturalLanguageQuery);

            var prompt = BuildSQLPrompt(schema, examples, naturalLanguageQuery);
            var requestBody = new
            {
                model,
                prompt,
                stream = false,
                options = new { temperature = 0.0, num_predict = 8000 }
            };

            var ollamaResponse = await PostAsync<OllamaGenerateResponse>("/api/generate", requestBody, ct);

            if (ollamaResponse?.Done != true || string.IsNullOrEmpty(ollamaResponse.Response))
            {
                throw new InvalidOperationException("Ollama generation incomplete or empty.");
            }

            var sql = ExtractSQLFromResponse(ollamaResponse.Response);

            if (string.IsNullOrEmpty(sql))
            {
                throw new InvalidOperationException($"No valid SQL found in Ollama response.");
            }

            _logger.LogInformation("Generated SQL query (tokens: {Tokens})", ollamaResponse.PromptEvalCount);
            return sql;
        }

        private static string BuildSQLPrompt(string schema, string examples, string query)
        {
            return $$"""
                You are a SQL expert for a SQL Server 2022 database.
                - Generate a valid, efficient SQL query (SELECT only, no DDL/DML).
                - Use table/column names exactly as they appear in the schema.
                - Respect relationships and nullability.
                - Format DateTime as "dd-MM-yy".
                - Optimize: WHERE for filters, JOINs only if needed.
                
                Schema:
                {{schema}}
                {{examples}}
                
                User Question:
                {{query}}
                
                Return ONLY the SQL query, no explanation.
                """;
        }

        private static string ExtractSQLFromResponse(string response)
        {
            var trimmed = response.Trim();

            // Try [SQL Start]/[SQL End] markers
            var sqlStart = trimmed.IndexOf("[SQL Start]", StringComparison.Ordinal);
            if (sqlStart >= 0)
            {
                sqlStart += "[SQL Start]".Length;
                var sqlEnd = trimmed.IndexOf("[SQL End]", sqlStart, StringComparison.Ordinal);
                if (sqlEnd > sqlStart)
                {
                    return trimmed.Substring(sqlStart, sqlEnd - sqlStart).Trim();
                }
            }

            // Try markdown ```sql blocks
            var codeBlockStart = trimmed.IndexOf("```sql", StringComparison.OrdinalIgnoreCase);
            if (codeBlockStart >= 0)
            {
                codeBlockStart += 6;
                var codeBlockEnd = trimmed.IndexOf("```", codeBlockStart, StringComparison.Ordinal);
                if (codeBlockEnd > codeBlockStart)
                {
                    return trimmed.Substring(codeBlockStart, codeBlockEnd - codeBlockStart).Trim();
                }
            }

            // Fallback: Extract first SELECT...FROM... query
            var selectMatch = Regex.Match(
                trimmed,
                @"SELECT\s+(?:.*?\s+)?FROM\s+.*?(?:;|WHERE|GROUP|ORDER|$)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (selectMatch.Success)
            {
                return selectMatch.Value.TrimEnd(';', ' ').Trim();
            }

            return null;
        }
    }

    // Add these response models if not already present
    //public class OllamaTagsResponse
    //{
    //    [JsonPropertyName("models")]
    //    public List<OllamaModel> Models { get; set; } = new();
    //}

    //public class OllamaModel
    //{
    //    [JsonPropertyName("name")]
    //    public string Name { get; set; } = string.Empty;

    //    [JsonPropertyName("size")]
    //    public long Size { get; set; }
    //}
}