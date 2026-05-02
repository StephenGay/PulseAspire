using CommunityToolkit.Aspire.Hosting.Ollama;
using Microsoft.Extensions.Hosting;


var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

var cache = builder.AddRedis("cache")
                    .WithRedisCommander()
                    .WithDataVolume();

IResourceBuilder<IResourceWithConnectionString> sqlDbResource;

if (builder.Environment.IsDevelopment())
{
    // Use local SQL Server 2022 (no container spun up)
    sqlDbResource =  builder.AddConnectionString("dbPulse");
}
else
{
    var sqlPassword = builder.AddParameter("sql-password", secret: true);
    var sql = builder.AddSqlServer("PulseSqlServer", password: sqlPassword, port: 1433)
                     .WithDataVolume("Pulse-Aspire-Sql-Data")
                     .WithEnvironment("ACCEPT_EULA", "Y")
                     .WithEnvironment("MSSQL_PID", "Developer");

    sqlDbResource = sql.AddDatabase("dbPulse");
}

var PulseAI = builder.AddOllama("PulseAI")
                    
                    .WithDataVolume("Pulse-Aspire-AI-Models")
                    .WithEnvironment("OLLAMA_KEEP_ALIVE", "-1")

                    .WithEnvironment("OLLAMA_LLM_LIBRARY", "cuda_v12")  // ← ADD: Force CUDA library
                    .WithEnvironment("CUDA_DEVICE_ORDER", "PCI_BUS_ID")

                    .WithEnvironment("NVIDIA_VISIBLE_DEVICES", "all")      // ← ADD: Enable GPU
                    .WithEnvironment("ENABLE_GPU", "true")
                    .WithEnvironment("NVIDIA_DRIVER_CAPABILITIES", "compute,utility")
                    .WithEnvironment("OLLAMA_NUM_GPU", "1")              // ← ADD: Force use 1 GPU
                    .WithEnvironment("OLLAMA_GPU_LAYERS", "25")

                    //.WithLifetime(ContainerLifetime.Persistent)
                    .WithGPUSupport()
                    .WithOpenWebUI();

//.WithEnvironment("OLLAMA_FLASH_ATTENTION", "1")       // Optional: faster on modern GPUs
//.WithEnvironment("OLLAMA_NUM_PARALLEL", "1")          // Adjust based on your VRAM
//.WithEnvironment("ENABLE_GPU", "true")              // ← ADD: Enable GPU support in Ollama

var flapperAI = PulseAI.AddModel("Flapper:latest");

var TablesAI = builder.AddConnectionString("TablesAI");

var PulseApi = builder.AddProject<Projects.Pulse_ApiService>("PulseApi")
    .WithReference(cache)
    .WithReference(sqlDbResource)
    .WithHttpHealthCheck("/alive")
    .WithReference(PulseAI)
    .WithReference(flapperAI)
    .WithReference(TablesAI)
    .WaitFor(cache)
    .WaitFor(flapperAI)
    .WaitFor(PulseAI);

var PulseWebUI = builder.AddProject<Projects.Pulse_Web>("PulseWebUI")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/alive")
    .WithReference(PulseApi)
    .WaitFor(PulseApi)
    .WithReference(PulseAI)
    .WithReference(cache)
    .WaitFor(cache)
    .WaitFor(PulseAI);
    

builder.AddProject<Projects.Pulse_DesktopApp>("PulseDesktop")
    .WithReference(PulseApi);

// Note: Pulse.MobileApp is a .NET MAUI project and is run independently.
// It connects to the PulseApi endpoint at runtime via configuration.

builder.Build().Run();
