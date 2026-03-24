using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Pulse.MobileApp.Services;

public class PulseApiService
{
    private readonly HttpClient _httpClient;
    private const string ApiBaseUrl = "https://your-api-endpoint:5000"; // Change for production

    public PulseApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(ApiBaseUrl);
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json);
    }
}