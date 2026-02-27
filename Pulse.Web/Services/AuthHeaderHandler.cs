// BlazorUI/Services/AuthHeaderHandler.cs
using System.Net.Http.Headers;

namespace Pulse.Web.Services
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly TokenHolderService _tokenHolder;
        private readonly ILogger<AuthHeaderHandler> _logger;

        public AuthHeaderHandler(TokenHolderService tokenHolder, ILogger<AuthHeaderHandler> logger)
        {
            _tokenHolder = tokenHolder;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _tokenHolder.GetToken();
            
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation("✓ JWT token attached to {Method} {Path} - Token length: {Length}", 
                    request.Method, request.RequestUri?.PathAndQuery, token.Length);
            }
            else
            {
                _logger.LogWarning("✗ NO JWT token in holder for {Method} {Path}", 
                    request.Method, request.RequestUri?.PathAndQuery);
            }

            var response = await base.SendAsync(request, cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogError("✗ 401 Unauthorized on {Method} {Path} - Token was: {TokenStatus}", 
                    request.Method, request.RequestUri?.PathAndQuery, 
                    string.IsNullOrEmpty(token) ? "MISSING" : "PRESENT");
            }

            return response;
        }
    }
}

