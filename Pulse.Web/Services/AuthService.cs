using Pulse.Models.Users;
using Pulse.Web.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Http;
public class AuthService
{
    private readonly TokenStorageService _tokenStorage;

    public string? JwtToken { get; private set; }
    public ClaimsPrincipal aspireUser { get; private set; } = new ClaimsPrincipal(new ClaimsIdentity());
    public string? aspireUserId => aspireUser.FindFirstValue(ClaimTypes.NameIdentifier) ?? aspireUser.FindFirstValue(JwtRegisteredClaimNames.Sub);
    public string? aspireUsername => aspireUser.FindFirstValue(ClaimTypes.Name) ?? aspireUser.FindFirstValue(JwtRegisteredClaimNames.UniqueName);
    public string? aspireEmail => aspireUser.FindFirstValue(ClaimTypes.Email) ?? aspireUser.FindFirstValue(JwtRegisteredClaimNames.Email);
    public IEnumerable<string> Roles => aspireUser.FindAll(ClaimTypes.Role).Select(c => c.Value);
    public bool IsAuthenticated => aspireUser.Identity?.IsAuthenticated ?? false;

    public AuthService(TokenStorageService tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public async Task LoadTokenAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            SetAuthenticatedAsync(token);
        }
    }

    
    public async Task SetAuthenticatedAsync(string jwtToken)
    {
        JwtToken = jwtToken;
        await _tokenStorage.SetTokenAsync(jwtToken);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(jwtToken);

        // Optional client-side expiry check
        if (jwt.ValidTo < DateTime.UtcNow)
        {
            await ClearAsync();
            return;
        }

        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        aspireUser = new ClaimsPrincipal(identity);
    }

    public async Task ClearAsync()
    {
        JwtToken = null;
        aspireUser = new ClaimsPrincipal(new ClaimsIdentity());
        await _tokenStorage.RemoveTokenAsync();
    }
}

//// BlazorUI/Services/AuthService.cs
//using System.Net.Http;
//using System.Net.Http.Json;
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.Extensions.Logging;
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
//    private readonly ILogger<AuthService> _logger;

//    public AuthService(IHttpClientFactory factory, AuthenticationStateProvider authStateProvider, ILogger<AuthService> logger)
//    {
//        _httpClient = factory.CreateClient("PulseApiClient");
//        _authStateProvider = authStateProvider;
//        _logger = logger;
//    }

//    public async Task<string> Register(RegisterModel model)
//    {
//        var response = await _httpClient.PostAsJsonAsync("/Security/register", model);
//        return response.IsSuccessStatusCode ? "Success" : "Error";
//    }

//    public async Task<string> Login(LoginModel model)
//    {
//        var response = await _httpClient.PostAsJsonAsync("/Security/login", model);
//        if (!response.IsSuccessStatusCode)
//        {
//            _logger.LogWarning("Login failed with status code: {StatusCode}", response.StatusCode);
//            return "Error";
//        }

//        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
//        if (string.IsNullOrEmpty(result?.Token))
//        {
//            _logger.LogWarning("Login returned no token");
//            return "Error";
//        }

//        if (_authStateProvider is JwtAuthenticationStateProvider jwtProvider)
//        {
//            try
//            {
//                await jwtProvider.MarkUserAsAuthenticated(result.Token);
//                _logger.LogInformation("User marked as authenticated via JwtAuthenticationStateProvider");
//                return "Success";
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error marking user as authenticated");
//                return "Error";
//            }
//        }
//        else
//        {
//            _logger.LogError("AuthenticationStateProvider is not JwtAuthenticationStateProvider - cannot mark user as authenticated");
//            return "Error";
//        }
//    }

//    public async Task Logout()
//    {
//        if (_authStateProvider is JwtAuthenticationStateProvider jwtProvider)
//        {
//            await jwtProvider.MarkUserAsLoggedOut();
//            _logger.LogInformation("User logged out via JwtAuthenticationStateProvider");
//        }
//        else
//        {
//            _logger.LogWarning("Logout called but AuthenticationStateProvider is not JwtAuthenticationStateProvider");
//        }
//    }
//}

////// AuthResponse.cs
//public class AuthResponse { public string Token { get; set; } }

