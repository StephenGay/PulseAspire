using BlazorAnimation;
using Microsoft.FluentUI.AspNetCore.Components;

using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Microsoft.Extensions.Options;


using Pulse.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pulse.Web.Tools;
using Pulse.Models.CustomComponents;


var builder = WebApplication.CreateBuilder(args);

// Determine environment (Development, Production, etc.)
bool isDevelopment = builder.Environment.IsDevelopment();

// Helper for retry policy (only add in non-dev)
IAsyncPolicy<HttpResponseMessage> retryPolicy = isDevelopment
    ? Policy.NoOpAsync<HttpResponseMessage>() // Disable retries/timeouts in dev
    : HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TimeoutRejectedException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                Console.WriteLine($"Ollama retry {retryAttempt} after {timespan}");
            });

// Helper for timeout policy
IAsyncPolicy<HttpResponseMessage> timeoutPolicy = isDevelopment
    ? Policy.NoOpAsync<HttpResponseMessage>()
    : Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(620), TimeoutStrategy.Pessimistic);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

var ollamaEndpoint = builder.Configuration["OllamaApi:EndpointHttp"]
    ?? throw new InvalidOperationException("Missing configuration for Ollama:Endpoint");

//builder.Services.AddHttpClient().ClearResilienceHandlers();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<Pulse_AI>("OllamaClient", client =>
{
    client.BaseAddress = new Uri(ollamaEndpoint);
    client.Timeout = isDevelopment ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(600); // Infinite in dev to debug hangs
}
).ClearResilienceHandlers();

//builder.Services.AddHttpClient<Pulse_AI>("OllamaClient", client =>
//{
//    client.BaseAddress = new Uri(ollamaEndpoint);
//    client.Timeout = isDevelopment ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(600); // Infinite in dev to debug hangs
//})
//.AddPolicyHandler(retryPolicy)
//.AddPolicyHandler(timeoutPolicy);

var pulseApiEndpoint = builder.Configuration["PulseApi:Endpoint"] 
    ?? throw new InvalidOperationException("Missing configuration for PulseApi:Endpoint");

builder.Services.AddHttpClient<Pulse_AI>("PulseApiClient", client =>
{
    client.BaseAddress = new Uri(pulseApiEndpoint); // API base
    client.Timeout = isDevelopment ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(30);
})
    .ClearResilienceHandlers();
//.AddPolicyHandler(retryPolicy)
//.AddPolicyHandler(timeoutPolicy);

// Register Pulse_AI as Scoped (shares clients per Blazor circuit/request)
builder.Services.AddScoped<Pulse_AI>();

builder.Services.AddSingleton<PulseApiService>();
builder.Services.AddHttpClient<PulseApiService>(client =>
{
    client.BaseAddress = new Uri(pulseApiEndpoint);
    client.Timeout = TimeSpan.FromSeconds(300);
})
    .ClearResilienceHandlers();


builder.Services.AddSingleton<OllamaService>();
builder.Services.AddHttpClient<OllamaService>(client =>
{
    client.BaseAddress = new Uri(ollamaEndpoint);
    client.Timeout = TimeSpan.FromMinutes(2);
})
    .ClearResilienceHandlers();

builder.Services.AddSingleton<DataTransferService>();
builder.Services.AddSingleton<Pulse_AI>();

builder.Services.AddFluentUIComponents();

builder.Services.AddHttpForwarderWithServiceDiscovery();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new TypeConverter());
    });
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<AnimationOptions>(Guid.NewGuid().ToString(), c => { });
//builder.Services.AddHttpClient<WeatherApiClient>(client =>
//    {
//        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
//        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
//        client.BaseAddress = new("https+http://apiservice");
//    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<Pulse.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();


