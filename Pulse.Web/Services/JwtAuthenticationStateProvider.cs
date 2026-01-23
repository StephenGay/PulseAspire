//// BlazorUI/Services/JwtAuthenticationStateProvider.cs
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.Extensions.Caching.Distributed;
//using Pulse.Web.Services;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//public class JwtAuthenticationStateProvider : AuthenticationStateProvider
//{
//    private readonly IDistributedCache _cache;
//    private readonly ICircuitState _circuitState;

//    public JwtAuthenticationStateProvider(IDistributedCache cache, ICircuitState circuitState)
//    {
//        _cache = cache;
//        _circuitState = circuitState;
//    }

//    private string GetCacheKey() => $"jwt:{_circuitState.CurrentCircuitId ?? "Unknown"}"; // Guid.NewGuid().ToString()}";

//    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
//    {
//        var cacheKey = GetCacheKey();
//        var tokenBytes = await _cache.GetAsync(cacheKey);
//        if (tokenBytes == null || tokenBytes.Length == 0)
//        {
//            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
//        }

//        var token = Encoding.UTF8.GetString(tokenBytes);
//        var handler = new JwtSecurityTokenHandler();
//        var jwtToken = handler.ReadJwtToken(token);
//        var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
//        return new AuthenticationState(new ClaimsPrincipal(identity));
//    }

//    public async Task MarkUserAsAuthenticated(string token)
//    {
//        var cacheKey = GetCacheKey();
//        var bytes = Encoding.UTF8.GetBytes(token);
//        var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
//        await _cache.SetAsync(cacheKey, bytes, options);
//        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
//    }

//    public async Task MarkUserAsLoggedOut()
//    {
//        var cacheKey = GetCacheKey();
//        await _cache.RemoveAsync(cacheKey);
//        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
//    }
//}

