using OllamaSharp;

namespace Pulse.ApiService.PulseAI.Services;

public interface IPulseAiClientFactory
{
    OllamaApiClient GetClient(string name);
}

public class PulseAiClientFactory : IPulseAiClientFactory
{
    private readonly Dictionary<string, OllamaApiClient> _clients = new();
    private readonly Dictionary<string, TimeSpan> _clientTimeouts = new();
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(10);

    public PulseAiClientFactory(Dictionary<string, string> clientUrls)
    {
        foreach (var (name, url) in clientUrls)
        {
            var httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(20),
                BaseAddress = new Uri(url)
            };
            _clients[name] = new OllamaApiClient(httpClient);
            //_clientTimeouts[name] = _defaultTimeout;
        }
    }

    public OllamaApiClient GetClient(string name)
    {
        if (_clients.TryGetValue(name, out var client))
        {
            return client;
        }

        throw new ArgumentException($"No PulseAiClient registered with name '{name}'");
    }

}
