using BlazorAnimation;
using Pulse.Web;
using Pulse.Web.Components;
using Pulse.Web.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

var pulseApiEndpoint = builder.Configuration["PulseApi:Endpoint"] 
    ?? throw new InvalidOperationException("Missing configuration for PulseApi:Endpoint");

builder.Services.AddSingleton<PulseApiService>();
builder.Services.AddHttpClient<PulseApiService>(client =>
{
    client.BaseAddress = new Uri(pulseApiEndpoint);
});

var ollamaEndpoint = builder.Configuration["OllamaApi:EndpointHttp"] 
    ?? throw new InvalidOperationException("Missing configuration for Ollama:Endpoint");
builder.Services.AddSingleton<OllamaService>();
builder.Services.AddHttpClient<OllamaService>(client =>
{
    client.BaseAddress = new Uri(ollamaEndpoint);
});

builder.Services.AddSingleton<DataTransferService>();
builder.Services.AddSingleton<Pulse_AI>();

builder.Services.AddMudServices();
builder.Services.AddHttpForwarderWithServiceDiscovery();

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
