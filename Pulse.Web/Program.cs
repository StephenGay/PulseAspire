// Cleaned & Optimized Pulse.Web/Program.cs
// Key Fixes:
// - Removed duplicates (TokenStorageService)
// - Simplified circuit handler registration (single line, scoped)
// - Removed unnecessary AddControllers() (Blazor Server doesn't need MVC controllers)
// - Ensured antiforgery skips SignalR paths (assuming /messagehub or similar)
// - Kept custom JWT auth (no server-side JwtBearer — handled manually via AuthService)
// - Added CascadingAuthenticationState (already present — required for custom provider)
// - Uncommented/added hub mapping placeholder (adjust to your actual hub, e.g., MessageHub)
// - Minor cleanups: removed redundant comments, ensured logical service order

using Aspire.StackExchange.Redis;
using BlazorAnimation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.FluentUI.AspNetCore.Components;
using Pulse.ApiService.Extensions;
using Pulse.Models.Misc;
using Pulse.Models.Users;
using Pulse.Web.Components;
using Pulse.Web.Services;
using Pulse.Web.Extensions;
using Radzen;
using System.Net.Http.Headers;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

#region Aspire Scaffolding
builder.Services.ConfigureHttpClientDefaults(http =>
{
    // Don't add resilience - we'll control it per client
    http.UseSocketsHttpHandler((handler, sp) =>
    {
        handler.PooledConnectionLifetime = TimeSpan.FromMinutes(15);
    });
});


builder.AddServiceDefaults();
builder.AddRedisDistributedCache("cache");
builder.AddRedisOutputCache("cache");
#endregion

#region Services (AI, API Clients, Data Transfer)
builder.Services.AddScoped<Pulse_AI>();
builder.Services.AddScoped<PulseApiService>();
builder.Services.AddScoped<OllamaService>();
builder.Services.AddScoped<DataTransferService>();
builder.Services.AddScoped<RollerModelGenerationService>();
builder.Services.AddScoped<RollerDataTransferService>();
builder.Services.AddScoped<Global_AI_Functions>();
builder.Services.AddScoped<GlobalFunctions>();
builder.Services.AddScoped<ApiErrorHandler>();
builder.Services.AddScoped<AiPromptService>();  // ? Centralized AI prompts
builder.Services.AddSingleton<IMouseService, MouseService>();
builder.Services.AddScoped<PulseThemeService>();
builder.Services.AddScoped<IHtmlCaptureService, HtmlCaptureService>();
#endregion

#region HttpClients (Consolidated)
var ollamaEndpoint = builder.Configuration["Aspire:Services:PulseAI:Endpoint:0"]
    ?? builder.Configuration["OllamaApi:EndpointHttp"]
    ?? throw new InvalidOperationException("Missing OllamaApi endpoint");

var pulseApiEndpoint = builder.Configuration["Aspire:Services:PulseApi:Http:0"]
    ?? builder.Configuration["PulseApi:Endpoint"]
    ?? throw new InvalidOperationException("Missing PulseApi endpoint");

builder.Services.AddScoped<OllamaService>();

// Ollama HttpClient (infinite timeout for long-running model inference)
builder.Services.AddHttpClient<OllamaService>("OllamaClient", client =>
{
    client.BaseAddress = new Uri(ollamaEndpoint);
    client.Timeout = Timeout.InfiniteTimeSpan;
})
.StopPollyTimeouts();


// Pulse API HttpClient (with auth header handler for JWT)
builder.Services.AddHttpClient<PulseApiService>("PulseApiClient", client =>
{
    client.BaseAddress = new Uri(pulseApiEndpoint);
    client.Timeout = Timeout.InfiniteTimeSpan;
})
.StopPollyTimeouts()
.AddHttpMessageHandler<AuthHeaderHandler>();

// Flapper API HttpClient (for long-running AI model inference)
builder.Services.AddHttpClient("FlapperApiClient", client =>
{
    client.Timeout = Timeout.InfiniteTimeSpan;
})
.StopPollyTimeouts();

#endregion

#region Authentication & Authorization (Custom JWT via AuthService)
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Admin"));
    
    options.AddPolicy("User", policy =>
        policy.RequireRole("User", "Admin"));
});

builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddSingleton<TokenHolderService>();  // Changed from AddScoped to AddSingleton
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthenticationStateProvider>());

builder.Services.AddScoped<AuthHeaderHandler>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

#endregion

#region Circuit Handling
builder.Services.AddScoped<ICircuitState, CircuitState>();
builder.Services.AddScoped<CircuitHandler, CircuitIdService>();
#endregion

// In Pulse.Web/Program.cs
builder.Services.AddSingleton<HubConnection>(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HubConnectionBuilder()
        .WithUrl(navigationManager.ToAbsoluteUri("/messagehub"))
        .WithAutomaticReconnect()
        .Build();
});

#region UI Frameworks & Tools
builder.Services.AddScoped<IPulseToastService, PulseToastService>();

//builder.Services.AddBootstrapBlazor(options =>
//{
//    options.ToastDelay = 6000;
//    options.ToastPlacement = BootstrapBlazor.Components.Placement.BottomEnd;
//});
builder.Services.AddSpeechSynthesis();
builder.Services.AddSpeechRecognition();
builder.Services.AddFluentUIComponents();
builder.Services.AddDataGridEntityFrameworkAdapter();
builder.Services.AddScoped<Microsoft.FluentUI.AspNetCore.Components.DialogService>();
builder.Services.AddScoped<AppState>();
builder.Services.Configure<AnimationOptions>(Guid.NewGuid().ToString(), _ => { });
builder.Services.AddRadzenComponents();
#endregion

#region Blazor Server Setup
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(5);
        options.DisconnectedCircuitMaxRetained = 100;
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(60);
    });

builder.Services.AddHttpForwarderWithServiceDiscovery();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<MessageHubService>();
#endregion

// Add IMemoryCache if not already registered
builder.Services.AddMemoryCache();

var app = builder.Build();

#region Middleware Pipeline (Correct Order)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}


// At startup, validate Ollama is reachable
using (var scope = app.Services.CreateScope())
{
    var ollamaService = scope.ServiceProvider.GetRequiredService<OllamaService>();
    var isValid = await ollamaService.ValidateConnectionAsync("Flapper:latest");

    if (!isValid)
    {
        app.Logger.LogWarning("Ollama service unavailable at startup");
        // Optionally fail startup or run in degraded mode
    }
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseOutputCache();

// Skip antiforgery validation for SignalR hub negotiate/connections
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/messagehub"), appBuilder =>
{
    appBuilder.UseAntiforgery();
});

app.MapStaticAssets();
app.MapDefaultEndpoints();
#endregion

// Map SignalR hubs (uncomment and update to actual Hub class if using MessageHub)
// app.MapHub<MessageHub>("/messagehub");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();