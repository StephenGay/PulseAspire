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
    sqlDbResource = builder.AddConnectionString("dbPulse");
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

var PulseAI = builder.AddConnectionString("PulseAI");

var TablesAI = builder.AddConnectionString("TablesAI");

var PulseApi = builder.AddProject<Projects.Pulse_ApiService>("PulseApi")
    .WithReference(cache)
    .WithReference(sqlDbResource)
    .WithHttpHealthCheck("/alive")
    .WithReference(PulseAI)
    .WithReference(TablesAI)
    .WaitFor(cache);

var PulseWebUI = builder.AddProject<Projects.Pulse_Web>("PulseWebUI")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/alive")
    .WithReference(PulseApi)
    .WaitFor(PulseApi)
    .WithReference(cache)
    .WaitFor(cache);


//builder.AddProject<Projects.Pulse_DesktopApp>("PulseDesktop")
//    .WithReference(PulseApi);

// Note: Pulse.MobileApp is a .NET MAUI project and is run independently.
// It connects to the PulseApi endpoint at runtime via configuration.

builder.Build().Run();
