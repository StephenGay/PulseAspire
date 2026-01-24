using Pulse.Models.Users;
using Pulse.Web.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Http;
using Microsoft.Extensions.Caching.Distributed;

public class AuthService
{
    private readonly TokenStorageService _tokenStorage;
    private readonly TokenHolderService _tokenHolder;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AuthService> _logger;

    public string? JwtToken { get; private set; }
    public ClaimsPrincipal aspireUser { get; private set; } = new ClaimsPrincipal(new ClaimsIdentity());
    public string? aspireUserId => aspireUser.FindFirstValue(ClaimTypes.NameIdentifier) ?? aspireUser.FindFirstValue(JwtRegisteredClaimNames.Sub);
    public string? aspireUsername => aspireUser.FindFirstValue(ClaimTypes.Name) ?? aspireUser.FindFirstValue(JwtRegisteredClaimNames.UniqueName);
    public string? aspireEmail => aspireUser.FindFirstValue(ClaimTypes.Email) ?? aspireUser.FindFirstValue(JwtRegisteredClaimNames.Email);
    public IEnumerable<string> Roles => aspireUser.FindAll(ClaimTypes.Role).Select(c => c.Value);
    public bool IsAuthenticated => aspireUser.Identity?.IsAuthenticated ?? false;

    public AuthService(
        TokenStorageService tokenStorage,
        TokenHolderService tokenHolder,
        IDistributedCache cache,
        ILogger<AuthService> logger)
    {
        _tokenStorage = tokenStorage;
        _tokenHolder = tokenHolder;
        _cache = cache;
        _logger = logger;
    }

    public async Task LoadTokenAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            await SetAuthenticatedAsync(token);
        }
    }

    public async Task SetAuthenticatedAsync(string jwtToken)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
        {
            throw new ArgumentException("JWT token cannot be null or empty", nameof(jwtToken));
        }

        JwtToken = jwtToken;
        await _tokenStorage.SetTokenAsync(jwtToken);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(jwtToken);

        // Check expiry on client-side
        if (jwt.ValidTo < DateTime.UtcNow)
        {
            _logger.LogWarning("Token already expired at {ExpireTime}", jwt.ValidTo);
            await ClearAsync();
            throw new InvalidOperationException("Token has already expired");
        }

        // Store token in memory holder for AuthHeaderHandler
        _tokenHolder.SetToken(jwtToken);

        _logger.LogInformation("Token authenticated successfully, expires at {ExpiryTime}", jwt.ValidTo);

        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        aspireUser = new ClaimsPrincipal(identity);
    }

    public async Task ClearAsync()
    {
        JwtToken = null;
        aspireUser = new ClaimsPrincipal(new ClaimsIdentity());
        await _tokenStorage.RemoveTokenAsync();

        // Clear from memory holder
        _tokenHolder.ClearToken();

        _logger.LogInformation("Authentication cleared");
    }
}