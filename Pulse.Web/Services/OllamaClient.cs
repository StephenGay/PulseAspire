using Pulse.Models.CustomComponents;
using Pulse.Models.Misc;
using Pulse.Web.Tools;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace Pulse.Web.Services
{
    internal sealed class OllamaService(HttpClient httpOllama, ILogger<OllamaService> logger)
    {
        //private readonly PulseApiService _pulseApiClient;
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

        public async Task<string> GenerateSQLQueryAsync(string schema, string examples, string naturalLanguageQuery)
        {

            var prompt = $@"
                You are an expert in SQL Server 2022 T-SQL. Given this schema:
                {schema}

                {examples}

                User question: {naturalLanguageQuery}

                Generate a valid SQL Server 2022 SELECT query in plain text. Return ONLY the T-SQL query.
                Use schema for accurate joins/columns.
                ";

            var requestBody = new
            {
                //model = ai.ModelName,
                //prompt = ai.Prompt,
                model = "llama3.1:latest",
                prompt = prompt,
                stream = false,
                max_tokens = 200,
                temperature = 0.0
            };

            try
            {
                var response = await _httpOllama.PostAsJsonAsync("/api/generate", requestBody);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error: Ollama returned status {response.StatusCode}. Details: {errorContent}";
                }

                var responseContent = await response.Content.ReadFromJsonAsync<JsonElement>();
                var rawResponse = responseContent.GetProperty("response").GetString()?.Trim();

                if (string.IsNullOrWhiteSpace(rawResponse))
                {
                    return "Error: Generated SQL query is empty.";
                }

                var match = Regex.Match(rawResponse, @"^\s*SELECT\s+.*?(?:;|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var sqlQuery = match.Success ? match.Value.Trim() : null;
                if (sqlQuery == null)
                    return $"Error: Generated query is not a valid SELECT statement.\nGenerated: {rawResponse}";
                if (!sqlQuery.EndsWith(";")) sqlQuery += ";";

                return $"Generated SQL: {sqlQuery}";
            }
            catch (Exception ex)
            {
                return $"Error: General: {ex.Message}";
            }
        }
    }
}
