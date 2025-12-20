using Microsoft.AspNetCore.Authentication;
using Pulse.Models.AI;
using Pulse.Models.CustomComponents;
using Pulse.Models.Customers;
using Pulse.Models.Misc;
using Pulse.Models.Production;
using Pulse.Models.Users;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pulse.Web.Services
{
    public sealed class PulseApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PulseApiService> _logger;

        public PulseApiService(HttpClient httpClient, ILogger<PulseApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string requestUri, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(requestUri, ct);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string requestUri, TRequest payload, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, payload, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                // Parse JSON errors if present
                try
                {
                    var errors = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(errorContent);
                    var messages = errors?.Select(e => e["ErrorMessage"] ?? e.ToString());
                    throw new HttpRequestException($"API error: {response.StatusCode} - {string.Join(", ", messages)}");
                }
                catch
                {
                    throw new HttpRequestException($"API error: {response.StatusCode} - {errorContent}");
                }
            }
            return await response.Content.ReadFromJsonAsync<TResponse>(ct);
        }

        public async Task<bool> PutAsync<TRequest>(string requestUri, TRequest payload, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(requestUri, payload, ct);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while putting to {RequestUri}", requestUri);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while putting to {RequestUri}", requestUri);
                throw;
            }
        }

        public async Task<string?> DeleteAsync(string requestUri, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(requestUri, ct);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync(ct);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while deleting {RequestUri}", requestUri);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting {RequestUri}", requestUri);
                throw;
            }
        }

        public async Task<List<SchemaDto>> GetSchemaAsync(CancellationToken ct = default)
        {
            try
            {
                return await GetAsync<List<SchemaDto>>("/AI/schema", ct) ?? new List<SchemaDto>();
            }
            catch
            {
                return new List<SchemaDto>();
            }
        }

        public async Task<string> GetAiSamples(CancellationToken ct = default)
        {
            try
            {
                var aiQueries = await GetAsync<List<AiQuery>>("/AI/examples", ct) ?? new List<AiQuery>();
                if (!aiQueries.Any()) return "";

                var sb = new StringBuilder("\nExamples of correct queries:\n");
                foreach (var ex in aiQueries)
                {
                    sb.AppendLine($"Question: {ex.Question}");
                    sb.AppendLine($"SQL: {ex.SqlQuery}");
                }
                return sb.ToString();
            }
            catch
            {
                return "";
            }
        }

        public List<Dictionary<string, object>> StringToDictionary(string result)
        {
            var results = new List<Dictionary<string, object>>();
            if (string.IsNullOrWhiteSpace(result))
            {
                _logger.LogWarning("StringToDictionary: Result is empty.");
                return results;
            }

            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var row = JsonSerializer.Deserialize<Dictionary<string, object>>(line, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (row != null) results.Add(row);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse JSON row: {Line}", line);
                }
            }
            return results;
        }

        public DataTable CreateDataTableFromDictionary(List<Dictionary<string, object>> results)
        {
            var dataTable = new DataTable();
            if (results == null || results.Count == 0)
            {
                _logger.LogWarning("CreateDataTableFromDictionary: No results to display.");
                return dataTable;
            }

            var firstRow = results.First();
            foreach (var key in firstRow.Keys)
            {
                dataTable.Columns.Add(key, typeof(object));  // Use object to handle various types
            }

            foreach (var row in results)
            {
                var dataRow = dataTable.NewRow();
                foreach (var kvp in row)
                {
                    dataRow[kvp.Key] = kvp.Value ?? DBNull.Value;
                }
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

        public DataTable StringToDataTable(string str)
        {
            return CreateDataTableFromDictionary(StringToDictionary(str));
        }

        public async Task<string?> AddNewRoleAsync(string newRoleName, CancellationToken ct = default)
        {
            return await PostAsync<object, string>("/Security/roles", new { Name = newRoleName }, ct);
        }

        public async Task AssignRoleToUserAsync(string userId, string roleName, CancellationToken ct = default)
        {
            var payload = new { RoleName = roleName };
            await PostAsync<object, object>($"/Security/users/{userId}/roles", payload, ct);
        }

        public async Task<UserFavouriteQry?> SaveFavouriteQueryAsync(UserFavouriteQry query, CancellationToken ct = default)
        {
            return await PostAsync<UserFavouriteQry, UserFavouriteQry>("/User/Favourites/SavedQueries/Add/", query, ct);
        }

        public async Task<AiQuery?> SaveAiQueryAsync(AiQuery query, CancellationToken ct = default)
        {
            return await PostAsync<AiQuery, AiQuery>("/AI/aiquery", query, ct);
        }

        public async Task<ContextualPrompt?> SaveCustomPromptAsync(ContextualPrompt prompt, CancellationToken ct = default)
        {
            return await PostAsync<ContextualPrompt, ContextualPrompt>("/AI/ContextualPrompt", prompt, ct);
        }
        public async Task<AuthenticationToken?> UserLoginAsync(LoginModel model, CancellationToken ct = default)
        {
            return await PostAsync<LoginModel, AuthenticationToken>("/Security/login", model, ct);
            // Caller should set headers: _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", token);
        }

        public async Task<AuthenticationToken?> UserRegisterAsync(RegisterModel model, CancellationToken ct = default)
        {
            return await PostAsync<RegisterModel, AuthenticationToken>("/Security/register", model, ct);
            // Caller handles token
        }

        public async Task<EquipmentCapability?> AddEquipmentCapabilityAsync(EquipmentCapability eqCap, CancellationToken ct = default)
        {
            return await PostAsync<EquipmentCapability, EquipmentCapability>("Divisions/Equipment/Capabilities/Add/", eqCap, ct);
        }

        public async Task<WorkCentreFunctions?> AddWorkCentreFunctionAsync(WorkCentreFunctions wcFunc, CancellationToken ct = default)
        {
            return await PostAsync<WorkCentreFunctions, WorkCentreFunctions>("Divisions/WorkCentre/Functions/Add/", wcFunc, ct);
        }

        public async Task<string?> DeleteFavouriteQueryAsync(int ID, CancellationToken ct = default)
        {
            return await DeleteAsync($"/User/Favourites/SavedQueries/Delete/{ID}", ct);
        }

        public async Task<bool> UpdatePlanItemAsync(ProductionPlanItem pI, CancellationToken ct = default)
        {
            return await PutAsync($"/Divisions/WIP/UpdateProductionPlanItem/{pI.ProductionPlanItemID}", pI, ct);
        }

        public async Task<bool> RecordQueryVoteAsync(int qID, string vote, CancellationToken ct = default)
        {
            return await PutAsync($"/AI/savedqueries/vote/{qID}/{Uri.EscapeDataString(vote)}", vote, ct);
        }
        public async Task<bool> UpdateClientMasterAsync(Customer client, CustomerUpdateDto custUpdate, CancellationToken ct = default)
        {
            var response = await _httpClient.PatchAsJsonAsync($"/Customers/Details/{client.FullClientID}/Update/MasterFile", custUpdate, ct);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
        public async Task<List<ProductionPlanItem>> GetWOProductionPlan(int WONo, CancellationToken ct = default)
        {
            List<ProductionPlanItem>? pp = await GetAsync<List<ProductionPlanItem>>($"/Divisions/Production/WorkOrder/{WONo}/GetProductionPlan", ct);
            if(pp == null)
            {
                pp = await GetAsync<List<ProductionPlanItem>>($"/Divisions/Production/WorkOrder/{WONo}/CreateProductionPlan", ct);
            }
            return pp;
        }
    }
}