using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Pulse.Models.Api;
using Pulse.Models.Users;

namespace Pulse.MobileApp.Services;

public class PulseApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private const string TokenStorageKey = "auth_token";
    private const string UserStorageKey = "current_user";
    private const string UserIdStorageKey = "user_id";

    public PulseApiService()
    {
        // Load configuration from appsettings.json
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        _apiBaseUrl = config["ApiSettings:BaseUrl"] ?? "https://192.168.0.6:5475";

        var handler = new HttpClientHandler();
#if DEBUG
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif
        _httpClient = new HttpClient(handler);
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
    }

    /// <summary>
    /// Logs in user and stores token - matches web app API contract
    /// </summary>
    public async Task<ApiResponse<AuthenticationToken>?> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new { email, password };
            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Use the same endpoint as web app
            var response = await _httpClient.PostAsync("/Security/login", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<AuthenticationToken>>(jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Extract token from wrapped response (same as web app)
            if (apiResponse?.Success == true && apiResponse.Data?.Token != null)
            {
                // Store token securely
                await SecureStorage.SetAsync(TokenStorageKey, apiResponse.Data.Token);
                await SecureStorage.SetAsync(UserStorageKey, apiResponse.Data.Username ?? apiResponse.Data.Email ?? email);
                await SecureStorage.SetAsync(UserIdStorageKey, apiResponse.Data.UserId ?? string.Empty);

                // Set default auth header
                SetAuthToken(apiResponse.Data.Token);
                return apiResponse;
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Login error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Sets the authorization token for future requests
    /// </summary>
    private void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Checks if user is logged in
    /// </summary>
    public async Task<bool> IsLoggedInAsync()
    {
        var token = await SecureStorage.GetAsync(TokenStorageKey);
        if (!string.IsNullOrEmpty(token))
        {
            SetAuthToken(token);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Logs out the user
    /// </summary>
    public async Task LogoutAsync()
    {
        SecureStorage.Remove(TokenStorageKey);
        SecureStorage.Remove(UserStorageKey);
        SecureStorage.Remove(UserIdStorageKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Get current logged in user
    /// </summary>
    public async Task<string?> GetCurrentUserAsync()
    {
        return await SecureStorage.GetAsync(UserStorageKey);
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json);
    }
}
