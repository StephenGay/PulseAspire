// BlazorUI/Services/JwtAuthenticationStateProvider.cs
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Pulse.Web.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IDistributedCache _cache;
    private readonly ICircuitState _circuitState;

    public JwtAuthenticationStateProvider(IDistributedCache cache, ICircuitState circuitState)
    {
        _cache = cache;
        _circuitState = circuitState;
    }

    private string GetCacheKey() => $"jwt:{_circuitState.CurrentCircuitId ?? "Unknown"}"; // Guid.NewGuid().ToString()}";

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var cacheKey = GetCacheKey();
        var tokenBytes = await _cache.GetAsync(cacheKey);
        if (tokenBytes == null || tokenBytes.Length == 0)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var token = Encoding.UTF8.GetString(tokenBytes);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        var cacheKey = GetCacheKey();
        var bytes = Encoding.UTF8.GetBytes(token);
        var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
        await _cache.SetAsync(cacheKey, bytes, options);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsLoggedOut()
    {
        var cacheKey = GetCacheKey();
        await _cache.RemoveAsync(cacheKey);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}

// BlazorUI/Services/JwtAuthenticationStateProvider.cs
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.AspNetCore.Http; // For IHttpContextAccessor
//using Microsoft.Extensions.Caching.Distributed;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//public class JwtAuthenticationStateProvider : AuthenticationStateProvider
//{
//    private readonly IDistributedCache _cache;
//    private readonly IHttpContextAccessor _httpContextAccessor;

//    public JwtAuthenticationStateProvider(IDistributedCache cache, IHttpContextAccessor httpContextAccessor)
//    {
//        _cache = cache;
//        _httpContextAccessor = httpContextAccessor;
//    }

//    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
//    {
//        var sessionId = _httpContextAccessor.HttpContext?.Session.Id ?? Guid.NewGuid().ToString(); // Fallback for no session
//        var cacheKey = $"jwt:{sessionId}";

//        var tokenBytes = await _cache.GetAsync(cacheKey);
//        if (tokenBytes == null || tokenBytes.Length == 0)
//        {
//            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())); // Unauthenticated
//        }

//        var token = Encoding.UTF8.GetString(tokenBytes);
//        var handler = new JwtSecurityTokenHandler();
//        var jwtToken = handler.ReadJwtToken(token);
//        var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
//        return new AuthenticationState(new ClaimsPrincipal(identity));
//    }

//    public async Task MarkUserAsAuthenticated(string token)
//    {
//        var sessionId = _httpContextAccessor.HttpContext?.Session.Id ?? Guid.NewGuid().ToString();
//        var cacheKey = $"jwt:{sessionId}";
//        var bytes = Encoding.UTF8.GetBytes(token);
//        var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
//        await _cache.SetAsync(cacheKey, bytes, options);
//        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
//    }

//    public async Task MarkUserAsLoggedOut()
//    {
//        var sessionId = _httpContextAccessor.HttpContext?.Session.Id ?? Guid.NewGuid().ToString();
//        var cacheKey = $"jwt:{sessionId}";
//        await _cache.RemoveAsync(cacheKey);
//        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
//    }
//}

//// BlazorUI/Services/JwtAuthenticationStateProvider.cs
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.Extensions.Caching.Distributed;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//public class JwtAuthenticationStateProvider : AuthenticationStateProvider
//{
//    private readonly IDistributedCache _cache;
//    //private readonly string _cacheKey; // e.g., "jwt:{userId}" or circuit-specific

//    //public JwtAuthenticationStateProvider(IDistributedCache cache, IHttpContextAccessor accessor) // Or custom key provider
//    //{
//    //    _cache = cache;
//    //    _cacheKey = $"jwt:{accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous"}";
//    //}

//    //public override async Task<AuthenticationState> GetAuthenticationStateAsync()
//    //{
//    //    var tokenBytes = await _cache.GetAsync(_cacheKey);
//    //    if (tokenBytes == null) return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

//    //    var token = Encoding.UTF8.GetString(tokenBytes);
//    //    var handler = new JwtSecurityTokenHandler();
//    //    var jwtToken = handler.ReadJwtToken(token);
//    //    var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
//    //    return new AuthenticationState(new ClaimsPrincipal(identity));
//    //}
//    public JwtAuthenticationStateProvider(IDistributedCache cache)
//    {
//        _cache = cache;
//    }

//    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
//    {
//        var state = await base.GetAuthenticationStateAsync(); // Or inject another way
//        var userId = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
//        var cacheKey = $"jwt:{userId}";

//        var tokenBytes = await _cache.GetAsync(cacheKey);
//        if (tokenBytes == null) return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

//        var token = Encoding.UTF8.GetString(tokenBytes);
//        var handler = new JwtSecurityTokenHandler();
//        var jwtToken = handler.ReadJwtToken(token);
//        var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
//        return new AuthenticationState(new ClaimsPrincipal(identity));
//    }
//    public async Task MarkUserAsAuthenticated(string token, string userId)
//    {
//        var key = $"jwt:{userId}";
//        var bytes = Encoding.UTF8.GetBytes(token);
//        var options = new DistributedCacheEntryOptions()
//            .SetAbsoluteExpiration(TimeSpan.FromMinutes(60)); // Match JWT expiry or sliding
//        await _cache.SetAsync(key, bytes, options);
//        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
//    }

//    public async Task MarkUserAsLoggedOut(string userId)
//    {
//        await _cache.RemoveAsync($"jwt:{userId}");
//        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
//    }
//}