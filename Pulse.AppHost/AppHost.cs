
var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.Pulse_ApiService>("PulseApi")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Pulse_Web>("PulseWebUI")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

//var ollama = builder.AddContainer("ollama", "ollama/ollama").WithHttpEndpoint(11434).WithHealthCheck("/api/tags");

builder.Build().Run();
