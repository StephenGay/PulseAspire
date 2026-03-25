
var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
                    .WithRedisCommander();

var PulseApi = builder.AddProject<Projects.Pulse_ApiService>("PulseApi")
    //.WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache);

builder.AddProject<Projects.Pulse_Web>("PulseWebUI")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(PulseApi)
    .WaitFor(PulseApi)
    .WithReference(cache)
    .WaitFor(cache);

builder.AddProject<Projects.Pulse_DesktopApp>("PulseDesktop")
    .WithReference(PulseApi);

// Note: Pulse.MobileApp is a .NET MAUI project and is run independently.
// It connects to the PulseApi endpoint at runtime via configuration.

//var ollama = builder.AddContainer("ollama", "ollama/ollama").WithHttpEndpoint(11434).WithHealthCheck("/api/tags");

builder.Build().Run();
