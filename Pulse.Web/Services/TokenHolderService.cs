using System.Text;

namespace Pulse.Web.Services
{
    /// <summary>
    /// Singleton service that holds the JWT token in memory.
    /// Thread-safe storage for use across the application.
    /// </summary>
    public class TokenHolderService
    {
        private byte[] _tokenBytes = Array.Empty<byte>();
        private readonly object _lockObj = new object();
        private readonly ILogger<TokenHolderService> _logger;

        public TokenHolderService(ILogger<TokenHolderService> logger)
        {
            _logger = logger;
        }

        public void SetToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Attempted to set empty token");
                return;
            }

            lock (_lockObj)
            {
                _tokenBytes = Encoding.UTF8.GetBytes(token);
                _logger.LogInformation("Token stored in singleton holder, length: {Length}", _tokenBytes.Length);
            }
        }

        public string? GetToken()
        {
            lock (_lockObj)
            {
                if (_tokenBytes.Length == 0)
                {
                    _logger.LogDebug("No token available in holder");
                    return null;
                }

                var token = Encoding.UTF8.GetString(_tokenBytes);
                _logger.LogDebug("Token retrieved from singleton holder, length: {Length}", token.Length);
                return token;
            }
        }

        public void ClearToken()
        {
            lock (_lockObj)
            {
                _tokenBytes = Array.Empty<byte>();
                _logger.LogInformation("Token cleared from singleton holder");
            }
        }
    }
}