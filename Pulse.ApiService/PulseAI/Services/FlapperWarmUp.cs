namespace Pulse.ApiService.PulseAI.Services
{
    // OllamaWarmupService.cs
    public class FlapperWarmUp : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<FlapperWarmUp> _logger;

        public FlapperWarmUp(HttpClient httpClient, IConfiguration config, ILogger<FlapperWarmUp> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var model = _config["PulseAI:Model"] ?? "Flapper:latest"; // or your sqlcoder / fine-tuned model
            var baseUrl = _config["PulseAI:Url"] ?? "http://localhost:11434";

            try
            {
                // Simple preload with keep_alive forever
                var payload = new
                {
                    model = model,
                    keep_alive = -1
                    
                                               // You can also set options here if needed: num_ctx, temperature, etc.
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/generate", payload, stoppingToken);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Ollama model {Model} preloaded and kept alive indefinitely.", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to preload Ollama model.");
            }
        }
    }
}
