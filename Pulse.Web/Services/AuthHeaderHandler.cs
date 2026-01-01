// BlazorUI/Services/AuthHeaderHandler.cs
using Microsoft.Extensions.Caching.Distributed;
using Pulse.Web.Services;
using System.Net.Http.Headers;
using System.Text;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IDistributedCache _cache;
    private readonly ICircuitState _circuitState;

    public AuthHeaderHandler(IDistributedCache cache, ICircuitState circuitState)
    {
        _cache = cache;
        _circuitState = circuitState;
    }

    private string GetCacheKey() => $"jwt:{_circuitState.CurrentCircuitId ?? "Unknown"}"; // Guid.NewGuid().ToString()}";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokenBytes = await _cache.GetAsync(GetCacheKey());
        if (tokenBytes != null && tokenBytes.Length > 0)
        {
            var token = Encoding.UTF8.GetString(tokenBytes);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

// BlazorUI/Services/AuthHeaderHandler.cs
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Caching.Distributed;
//using System.Net.Http.Headers;
//using System.Text;

//public class AuthHeaderHandler : DelegatingHandler
//{
//    private readonly IDistributedCache _cache;
//    private readonly IHttpContextAccessor _httpContextAccessor;

//    public AuthHeaderHandler(IDistributedCache cache, IHttpContextAccessor httpContextAccessor)
//    {
//        _cache = cache;
//        _httpContextAccessor = httpContextAccessor;
//    }

//    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//    {
//        var sessionId = _httpContextAccessor.HttpContext?.Session.Id ?? Guid.NewGuid().ToString();
//        var cacheKey = $"jwt:{sessionId}";

//        var tokenBytes = await _cache.GetAsync(cacheKey);
//        if (tokenBytes != null && tokenBytes.Length > 0)
//        {
//            var token = Encoding.UTF8.GetString(tokenBytes);
//            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
//        }

//        return await base.SendAsync(request, cancellationToken);
//    }
//}

//// BlazorUI/Services/AuthHeaderHandler.cs
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.Extensions.Caching.Distributed;
//using System.Net.Http.Headers;
//using System.Security.Claims;
//using System.Text;

////public class AuthHeaderHandler : DelegatingHandler
////{
////    private readonly IDistributedCache _cache;
////    private readonly string _cacheKey; // Same logic as provider

////    public AuthHeaderHandler(IDistributedCache cache /*, key provider */)
////    {
////        _cache = cache;
////        // Resolve _cacheKey similarly
////    }

////    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
////    {
////        var tokenBytes = await _cache.GetAsync(_cacheKey);
////        if (tokenBytes != null)
////        {
////            var token = Encoding.UTF8.GetString(tokenBytes);
////            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
////        }
////        return await base.SendAsync(request, ct);
////    }
////}
//// BlazorUI/Services/AuthHeaderHandler.cs
//public class AuthHeaderHandler : DelegatingHandler
//{
//    private readonly IDistributedCache _cache;
//    private readonly AuthenticationStateProvider _authStateProvider;

//    public AuthHeaderHandler(IDistributedCache cache, AuthenticationStateProvider authStateProvider)
//    {
//        _cache = cache;
//        _authStateProvider = authStateProvider;
//    }

//    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
//    {
//        var state = await _authStateProvider.GetAuthenticationStateAsync();
//        var userId = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
//        var cacheKey = $"jwt:{userId}";

//        var tokenBytes = await _cache.GetAsync(cacheKey);
//        if (tokenBytes != null)
//        {
//            var token = Encoding.UTF8.GetString(tokenBytes);
//            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
//        }
//        return await base.SendAsync(request, ct);
//    }
//}