using Pulse.Models.Api;
using Pulse.Models.AI;
using Pulse.Models.CustomComponents;
using Pulse.Models.Customers;
using Pulse.Models.Misc;
using Pulse.Models.Production;
using Pulse.Models.Users;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pulse.Web.Services
{
    /// <summary>
    /// Service for communicating with Pulse.ApiService.
    /// Uses ApiEndpoints for centralized endpoint management.
    /// </summary>
    public sealed class PulseApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PulseApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public PulseApiService(HttpClient httpClient, ILogger<PulseApiService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        #region Core HTTP Methods

        public async Task<T?> GetAsync<T>(string requestUri, CancellationToken ct = default)
            where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(requestUri);

            try
            {
                _logger.LogDebug("GET request to {RequestUri}", requestUri);
                var response = await _httpClient.GetAsync(requestUri, ct);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(ct);

                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new PulseApiException(
                    $"Failed to fetch from {requestUri}",
                    requestUri,
                    ex.StatusCode,
                    innerException: ex);
            }
            catch (JsonException ex)
            {
                throw new PulseApiException(
                    $"Invalid JSON response from {requestUri}",
                    requestUri,
                    innerException: ex);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request to {RequestUri} was cancelled", requestUri);
                throw;
            }
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string requestUri,
            TRequest payload,
            CancellationToken ct = default)
            where TResponse : class
        {
            ArgumentException.ThrowIfNullOrEmpty(requestUri);
            ArgumentNullException.ThrowIfNull(payload);

            try
            {
                _logger.LogDebug("POST request to {RequestUri}", requestUri);
                var response = await _httpClient.PostAsJsonAsync(requestUri, payload, ct);

                if (!response.IsSuccessStatusCode)
                {
                    await HandleApiErrorAsync(response, requestUri, ct);
                }

                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, ct);
            }
            catch (HttpRequestException ex)
            {
                throw new PulseApiException(
                    $"POST request failed to {requestUri}",
                    requestUri,
                    ex.StatusCode,
                    innerException: ex);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request to {RequestUri} was cancelled", requestUri);
                throw;
            }
        }

        public async Task<bool> PutAsync<TRequest>(
            string requestUri,
            TRequest payload,
            CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(requestUri);
            ArgumentNullException.ThrowIfNull(payload);

            try
            {
                _logger.LogDebug("PUT request to {RequestUri}", requestUri);
                var response = await _httpClient.PutAsJsonAsync(requestUri, payload, ct);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new PulseApiException(
                    $"PUT request failed to {requestUri}",
                    requestUri,
                    ex.StatusCode,
                    innerException: ex);
            }
        }

        public async Task<bool> DeleteAsync(string requestUri, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(requestUri);

            try
            {
                _logger.LogDebug("DELETE request to {RequestUri}", requestUri);
                var response = await _httpClient.DeleteAsync(requestUri, ct);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new PulseApiException(
                    $"DELETE request failed to {requestUri}",
                    requestUri,
                    ex.StatusCode,
                    innerException: ex);
            }
        }

        public async Task<bool> PatchAsync<TRequest>(
            string requestUri,
            TRequest payload,
            CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(requestUri);
            ArgumentNullException.ThrowIfNull(payload);

            try
            {
                _logger.LogDebug("PATCH request to {RequestUri}", requestUri);
                var response = await _httpClient.PatchAsJsonAsync(requestUri, payload, ct);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new PulseApiException(
                    $"PATCH request failed to {requestUri}",
                    requestUri,
                    ex.StatusCode,
                    innerException: ex);
            }
        }

        #endregion

        #region Error Handling

        private async Task HandleApiErrorAsync(
            HttpResponseMessage response,
            string requestUri,
            CancellationToken ct)
        {
            try
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);

                // Try to parse as structured error
                var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(errorContent, _jsonOptions);

                if (errorResponse != null)
                {
                    var details = errorResponse.Errors != null
                        ? string.Join("; ", errorResponse.Errors.SelectMany(e => e.Value))
                        : errorResponse.Detail;

                    _logger.LogError("API error {StatusCode} from {RequestUri}: {Details}",
                        response.StatusCode, requestUri, details);

                    throw new PulseApiException(
                        errorResponse.Title ?? "API request failed",
                        requestUri,
                        response.StatusCode,
                        details);
                }

                // Fallback for plain text
                throw new PulseApiException(
                    "API request failed",
                    requestUri,
                    response.StatusCode,
                    errorContent[..Math.Min(200, errorContent.Length)]);
            }
            catch (JsonException)
            {
                throw new PulseApiException(
                    $"API request failed with status {response.StatusCode}",
                    requestUri,
                    response.StatusCode);
            }
        }

        #endregion

        #region AI Endpoints

        public async Task<List<SchemaDto>> GetSchemaAsync(CancellationToken ct = default)
        {
            try
            {
                var schema = await GetAsync<List<SchemaDto>>(ApiEndpoints.Ai.Schema, ct);
                _logger.LogInformation("Schema loaded: {Count} entities", schema?.Count ?? 0);
                return schema ?? new();
            }
            catch (PulseApiException ex)
            {
                _logger.LogError(ex, "Failed to load schema");
                return new();
            }
        }

        public async Task<string> GetAiExamplesAsync(CancellationToken ct = default)
        {
            try
            {
                var aiQueries = await GetAsync<List<AiQuery>>(ApiEndpoints.Ai.Examples, ct) ?? new();

                if (!aiQueries.Any()) return "";

                var sb = new StringBuilder("\nExamples of correct queries:\n");
                foreach (var example in aiQueries)
                {
                    sb.AppendLine($"Question: {example.Question}");
                    sb.AppendLine($"SQL: {example.SqlQuery}");
                    sb.AppendLine();
                }

                return sb.ToString();
            }
            catch (PulseApiException ex)
            {
                _logger.LogWarning(ex, "Failed to load AI examples");
                return "";
            }
        }

        public async Task<AiQuery?> SaveAiQueryAsync(AiQuery query, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            return await PostAsync<AiQuery, AiQuery>(ApiEndpoints.Ai.AiQuery, query, ct);
        }

        public async Task<ContextualPrompt?> SaveCustomPromptAsync(
            ContextualPrompt prompt,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(prompt);
            return await PostAsync<ContextualPrompt, ContextualPrompt>(
                ApiEndpoints.Ai.ContextualPrompt, prompt, ct);
        }

        public async Task<bool> RecordQueryVoteAsync(
            int queryId,
            string vote,
            CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(vote);
            return await PutAsync(ApiEndpoints.Ai.Vote(queryId, vote), vote, ct);
        }

        #endregion

        #region Security Endpoints

        public async Task<AuthenticationToken?> UserLoginAsync(
            LoginModel model,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(model);
            return await PostAsync<LoginModel, AuthenticationToken>(
                ApiEndpoints.Security.Login, model, ct);
        }

        public async Task<AuthenticationToken?> UserRegisterAsync(
            RegisterModel model,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(model);
            return await PostAsync<RegisterModel, AuthenticationToken>(
                ApiEndpoints.Security.Register, model, ct);
        }

        public async Task<string?> AddNewRoleAsync(
            string roleName,
            CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(roleName);
            return await PostAsync<string, string>(
                ApiEndpoints.Security.CreateRole, roleName, ct);
        }

        public async Task<bool> AssignRoleToUserAsync(
            string userId,
            string roleName,
            CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(userId);
            ArgumentException.ThrowIfNullOrEmpty(roleName);

            var payload = new { userId, roleName };
            await PostAsync<object, object>(ApiEndpoints.Security.AssignRole, payload, ct);
            return true;
        }

        #endregion

        #region User Endpoints

        public async Task<UserFavouriteQry?> SaveFavouriteQueryAsync(
            UserFavouriteQry query,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            return await PostAsync<UserFavouriteQry, UserFavouriteQry>(
                ApiEndpoints.User.Favourites.SavedQueries.Add, query, ct);
        }

        public async Task<string?> DeleteFavouriteQueryAsync(
            int queryId,
            CancellationToken ct = default)
        {
            if (queryId <= 0)
                throw new ArgumentException("Query ID must be positive", nameof(queryId));

            return await DeleteAsync(ApiEndpoints.User.Favourites.SavedQueries.Delete(queryId), ct)
                ? "Deleted" : null;
        }

        #endregion

        #region Division Endpoints

        public async Task<EquipmentCapability?> AddEquipmentCapabilityAsync(
            EquipmentCapability capability,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(capability);
            return await PostAsync<EquipmentCapability, EquipmentCapability>(
                ApiEndpoints.Divisions.Equipment.Capabilities.Add, capability, ct);
        }

        public async Task<WorkCentreFunctions?> AddWorkCentreFunctionAsync(
            WorkCentreFunctions function,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(function);
            return await PostAsync<WorkCentreFunctions, WorkCentreFunctions>(
                ApiEndpoints.Divisions.WorkCentre.Functions.Add, function, ct);
        }

        public async Task<bool> UpdatePlanItemAsync(
            ProductionPlanItem planItem,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(planItem);
            return await PutAsync(
                ApiEndpoints.Divisions.WIP.UpdateProductionPlanItem(planItem.ProductionPlanItemID),
                planItem, ct);
        }

        public async Task<List<ProductionPlanItem>> GetWorkOrderProductionPlanAsync(
            int workOrderNumber,
            CancellationToken ct = default)
        {
            if (workOrderNumber <= 0)
                throw new ArgumentException("Work order number must be positive", nameof(workOrderNumber));

            try
            {
                var plan = await GetAsync<List<ProductionPlanItem>>(
                    ApiEndpoints.Divisions.Production.WorkOrder.GetProductionPlan(workOrderNumber), ct);

                if (plan == null)
                {
                    _logger.LogInformation("Production plan not found for WO {WONo}, creating...", workOrderNumber);
                    plan = await GetAsync<List<ProductionPlanItem>>(
                        ApiEndpoints.Divisions.Production.WorkOrder.CreateProductionPlan(workOrderNumber), ct);
                }

                return plan ?? new();
            }
            catch (PulseApiException ex)
            {
                _logger.LogError(ex, "Failed to get production plan for WO {WONo}", workOrderNumber);
                throw;
            }
        }

        #endregion

        #region Customer Endpoints

        public async Task<bool> UpdateClientMasterAsync(
            Customer client,
            CustomerUpdateDto updateData,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(updateData);

            return await PatchAsync(
                ApiEndpoints.Customers.Details.UpdateMasterFile(client.FullClientID),
                updateData, ct);
        }

        #endregion

        #region Data Transformation

        public List<Dictionary<string, object>> StringToDictionary(string result)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(result));

            var results = new List<Dictionary<string, object>>();
            if (string.IsNullOrWhiteSpace(result)) return results;

            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var row = JsonSerializer.Deserialize<Dictionary<string, object>>(line, _jsonOptions);
                    if (row?.Any() == true) results.Add(row);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse JSON row");
                }
            }

            return results;
        }

        public DataTable CreateDataTableFromDictionary(List<Dictionary<string, object>> results)
        {
            ArgumentNullException.ThrowIfNull(results);

            var dataTable = new DataTable();
            if (!results.Any()) return dataTable;

            var firstRow = results.First();
            foreach (var key in firstRow.Keys)
            {
                dataTable.Columns.Add(key, typeof(object));
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
            => CreateDataTableFromDictionary(StringToDictionary(str));

        #endregion
    }
}