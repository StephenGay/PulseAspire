
var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
                    .WithRedisCommander();

var PulseApi = builder.AddProject<Projects.Pulse_ApiService>("PulseApi")
    .WithHttpHealthCheck("/health")
    .WithReference(cache);

builder.AddProject<Projects.Pulse_Web>("PulseWebUI")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(PulseApi)
    .WaitFor(PulseApi)
    .WithReference(cache)
    .WaitFor(cache);

//var ollama = builder.AddContainer("ollama", "ollama/ollama").WithHttpEndpoint(11434).WithHealthCheck("/api/tags");

builder.Build().Run();
