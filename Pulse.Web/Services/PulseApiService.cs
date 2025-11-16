using Pulse.Models.CustomComponents;
using Pulse.Models.Misc;
using Pulse.Models.Production;
using Pulse.Models.Users;
using System.Data;
using System.Text;
using System.Text.Json;

namespace Pulse.Web.Services
{
    internal sealed class PulseApiService(HttpClient PulseApiClient, ILogger<PulseApiService> logger)
    {
        private readonly HttpClient _httpClient = PulseApiClient;
        private readonly ILogger<PulseApiService> _logger = logger;
        //private static string _cachedSchema;
        //private static DateTime _cacheExpiry = DateTime.MinValue;

        //public async Task<string> GetDetailedSchemaAsync()
        //{
        //    //if (DateTime.UtcNow < _cacheExpiry && _cachedSchema != null) return _cachedSchema;

        //    var schemas = await GetSchemaAsync(); // Use your PulseApiService
        //    //var schemaBuilder = new StringBuilder();

        //    //foreach (var schema in schemas)
        //    //{
        //    //    schemaBuilder.AppendLine($"Entity: {schema.EntityType} (Table: {schema.TableName})");
        //    //    schemaBuilder.AppendLine($"Primary Keys: {string.Join(", ", schema.PrimaryKeys)}");

        //    //    schemaBuilder.AppendLine("Columns:");
        //    //    foreach (var col in schema.Columns)
        //    //    {
        //    //        schemaBuilder.AppendLine($"- {col.Name} (Type: {col.DataType}, Nullable: {col.IsNullable}, PK: {col.IsPrimaryKey})");
        //    //    }

        //    //    schemaBuilder.AppendLine("Relationships:");
        //    //    foreach (var rel in schema.Relationships)
        //    //    {
        //    //        schemaBuilder.AppendLine($"- To {rel.RelatedEntityType} (Table: {rel.RelatedTableName}), Navigation: {rel.NavigationName}, FK Columns: {string.Join(", ", rel.ForeignKeyColumns)}, Cardinality: {rel.Cardinality}");
        //    //    }
        //    //    schemaBuilder.AppendLine(); // Separator
        //    //}

        //    //_cachedSchema = schemaBuilder.ToString();
        //    //_cacheExpiry = DateTime.UtcNow.AddMinutes(30); // Refresh interval
        //    //return _cachedSchema;
            
        //}
        public async Task<List<SchemaDto>> GetSchemaAsync()
        {
            string requestUri = "/AI/schema";

            try
            {
                var response = await _httpClient.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<List<SchemaDto>>(json);

                //var content = await response.Content.ReadAsStringAsync();

                //return JsonSerializer.Deserialize<List<SchemaDto>>(content, new JsonSerializerOptions
                //{
                //    PropertyNameCaseInsensitive = true
                //});
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
                var response = await _httpClient.GetAsync(requestUri);
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

        public async Task<string> GetAiSamples()
        {
            var requestUri = "/AI/examples";
            try
            {
                var aiQueries = await GetAsync<List<AiQuery>>(requestUri);
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

        public async Task<UserFavouriteQry?> SaveFavouriteQueryAsync(UserFavouriteQry query)
        {
            var response = await _httpClient.PostAsJsonAsync("/User/Favourites/SavedQueries/Add/", query);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserFavouriteQry>();
            }
            return null;
        }
        public async Task<AiQuery?> SaveAiQueryAsync(AiQuery query)
        {
            var response = await _httpClient.PostAsJsonAsync("/AI/aiquery", query);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AiQuery>();
            }
            return null;
        }
        public async Task<string?> DeleteFavouriteQueryAsync(int ID)
        {
            var response = await _httpClient.DeleteAsync($"/User/Favourites/SavedQueries/Delete/{ID}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<string>();
            }
            return null;
        }

        public async Task<ProductionPlanItem> UpdatePlanItem(ProductionPlanItem pI) 
        {
            var response = await _httpClient.PutAsJsonAsync($"/Divisions/WIP/UpdateProductionPlanItem/{pI.ProductionPlanItemID}", pI);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProductionPlanItem>();
            }
            return null;
        }
        public async Task<bool> RecordQueryVote(int qID, string Vote)
        {
            var response = await _httpClient.PutAsJsonAsync($"/AI/savedqueries/vote/{qID}/{Vote}", Vote);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }

}


