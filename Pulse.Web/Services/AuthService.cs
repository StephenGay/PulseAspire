
// BlazorUI/Services/AuthService.cs
using System.Net.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Pulse.Models.Users;
using System.Security.Claims;

public interface IAuthService
{
    Task<string> Register(RegisterModel model);
    Task<string> Login(LoginModel model);
    Task Logout();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(IHttpClientFactory factory, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = factory.CreateClient("PulseApiClient");
        _authStateProvider = authStateProvider;
    }

    public async Task<string> Register(RegisterModel model)
    {
        var response = await _httpClient.PostAsJsonAsync("/Security/register", model);
        return response.IsSuccessStatusCode ? "Success" : "Error";
    }
    public async Task<string> Login(LoginModel model)
{
    var response = await _httpClient.PostAsJsonAsync("/Security/login", model);
    if (!response.IsSuccessStatusCode) return "Error";

    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
    await ((JwtAuthenticationStateProvider)_authStateProvider).MarkUserAsAuthenticated(result.Token);
    return "Success";
}

public async Task Logout()
{
    await ((JwtAuthenticationStateProvider)_authStateProvider).MarkUserAsLoggedOut();
}
}

//// AuthResponse.cs
public class AuthResponse { public string Token { get; set; } }
//// BlazorUI/Services/AuthService.cs
//using Microsoft.AspNetCore.Components.Authorization;
//using Pulse.Models.Users;
//using System.Security.Claims;

//public interface IAuthService
//{
//    Task<string> Register(RegisterModel model);
//    Task<string> Login(LoginModel model);
//    Task Logout();
//}

//public class AuthService : IAuthService
//{
//    private readonly HttpClient _httpClient;
//    private readonly AuthenticationStateProvider _authStateProvider;

//    public AuthService(IHttpClientFactory factory, AuthenticationStateProvider authStateProvider)
//    {
//        _httpClient = factory.CreateClient("ApiClient");
//        _authStateProvider = authStateProvider;
//    }

//    public async Task<string> Register(RegisterModel model)
//    {
//        var response = await _httpClient.PostAsJsonAsync("/Security/register", model);
//        return response.IsSuccessStatusCode ? "Success" : "Error";
//    }

//    public async Task<string> Login(LoginModel model)
//    {
//        var response = await _httpClient.PostAsJsonAsync("/Security/login", model);
//        if (!response.IsSuccessStatusCode) return "Error";

//        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
//        ((JwtAuthenticationStateProvider)_authStateProvider).MarkUserAsAuthenticated(result.Token, result.UserId);
//        return "Success";
//    }

//    public async Task Logout()
//    {
//        var state = await _authStateProvider.GetAuthenticationStateAsync();
//        var userId = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Assuming Id in claims
//        await ((JwtAuthenticationStateProvider)_authStateProvider).MarkUserAsLoggedOut(userId ?? "anonymous");
//    }
//}

//// AuthResponse.cs
//public class AuthResponse { public string Token { get; set; } public string UserId { get; set; } }