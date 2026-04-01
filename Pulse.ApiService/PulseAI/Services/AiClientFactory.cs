using OllamaSharp;

namespace Pulse.ApiService.PulseAI.Services;

public interface IPulseAiClientFactory
{
    OllamaApiClient GetClient(string name);
}

public class PulseAiClientFactory : IPulseAiClientFactory
{
    private readonly Dictionary<string, OllamaApiClient> _clients = new();

    public PulseAiClientFactory(Dictionary<string, string> clientUrls)
    {
        foreach (var (name, url) in clientUrls)
        {
            _clients[name] = new OllamaApiClient(new Uri(url));
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
