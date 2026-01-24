// BlazorUI/Services/AuthHeaderHandler.cs
using System.Text;
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
                _logger.LogDebug("JWT token attached to {Method} {RequestUri}", request.Method, request.RequestUri?.PathAndQuery);
            }
            else
            {
                _logger.LogWarning("No JWT token found in token holder for {RequestUri}", request.RequestUri?.PathAndQuery);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

