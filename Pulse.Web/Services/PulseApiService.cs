using System.Data;
using System.Text.Json;

namespace Pulse.Web.Services
{
    internal sealed class PulseApiService(HttpClient PulseApiClient, ILogger<PulseApiService> logger)
    {
        private readonly HttpClient _httpClient = PulseApiClient;
        private readonly ILogger<PulseApiService> _logger = logger;

        public async Task<string> GetSchemaAsync()
        {
            var sch = await _httpClient.GetAsync("/AI/schema");
            return sch.ToString();
        }

        public async Task<T?> GetAsync<T>(string requestUri)
        {
            try
            {
                var response = await _httpClient.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
               
                return System.Text.Json.JsonSerializer.Deserialize<T>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while accessing {RequestUri}", requestUri);
                return default;
            }
            catch (System.Text.Json.JsonException ex)
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

        public async Task<List<Dictionary<string, object>>> StringToDictionary(string result)
        {
            var results = new List<Dictionary<string, object>>();
            if (string.IsNullOrWhiteSpace(result))
            {
                Console.WriteLine("ExtractResultsFromResult: Result is empty.");
                return results;
            }

            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    
                    try
                    {
                        var row = JsonSerializer.Deserialize<Dictionary<string, object>>(line);
                        results.Add(row);

                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"ExtractResultsFromResult: Failed to parse JSON row: {line}, Error: {ex.Message}");
                    }
                }
            }
            return results;
        }

        public async Task<DataTable> CreateDataTableFromDictionary(List<Dictionary<string, object>> results)
        {
            var dataTable = new DataTable();
            if (results == null || results.Count == 0)
            {
                Console.WriteLine("CreateDataTableFromResults: No results to display.");
                return dataTable;
            }

            // Use the first row to define columns
            var firstRow = results.First();
            foreach (var key in firstRow.Keys)
            {
                dataTable.Columns.Add(key, typeof(string)); // Use string for simplicity; adjust if needed
            }

            // Add rows
            foreach (var row in results)
            {
                var dataRow = dataTable.NewRow();
                foreach (var kvp in row)
                {
                    // Convert values to string to handle nulls and different types
                    dataRow[kvp.Key] = kvp.Value?.ToString() ?? "NULL";
                }
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        public async Task<DataTable> StringToDataTable(string str)
        {
            return await CreateDataTableFromDictionary(await StringToDictionary(str));
        }

    }

}
