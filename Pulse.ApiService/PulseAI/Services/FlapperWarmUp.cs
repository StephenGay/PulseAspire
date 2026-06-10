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
            var model = _config["PulseAI:Model"] ?? "Flapper:latest";
            var baseUrl = _config["PulseAI:Url"] ?? "http://localhost:11434";

            try
            {
                // Add a small delay to ensure Ollama container is fully ready
                //await Task.Delay(2000, stoppingToken);

                // Preload with keep_alive forever and GPU options to preserve layer offloading
                var payload = new
                {
                    model = model,
                    keep_alive = -1,
                    stream = false,
                    options = new
                    {
                        num_gpu = 25
                    }
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/generate", payload, stoppingToken);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Ollama model {Model} preloaded and kept alive indefinitely with GPU support.", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to preload Ollama model.");
            }
        }
    }
}
