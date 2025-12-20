using BlazorAnimation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Pulse.Models;
using Pulse.Models.Misc;
using Pulse.Models.Users;
using Pulse.Web.Services;
using Pulse.Web.Tools;
using Toolbelt.Blazor.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

#region Helpers to stop Ollama timing out - turned out to be Polly causing the problem so can look at removing.

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
#endregion

#region Add Aspire Scaffolding
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");
#endregion

#region Register Pulse.Api, Pulse.AI & Ollama as Singleton services to stay alive throughout app lifetime
var ollamaEndpoint = builder.Configuration["OllamaApi:EndpointHttp"]
    ?? throw new InvalidOperationException("Missing configuration for Ollama:Endpoint");

var pulseApiEndpoint = builder.Configuration["PulseApi:Endpoint"]
    ?? throw new InvalidOperationException("Missing configuration for PulseApi:Endpoint");

builder.Services.AddSingleton<Pulse_AI>();
builder.Services.AddSingleton<PulseApiService>();
builder.Services.AddHttpClient<PulseApiService>("PulseApiClient",client =>
{
    client.BaseAddress = new Uri(pulseApiEndpoint);
    client.Timeout = isDevelopment ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
})
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = true, UseDefaultCredentials = true })
    .ClearResilienceHandlers(); // Remove Polly handlers as they were causing issues with long requests

builder.Services.AddSingleton<OllamaService>();
builder.Services.AddHttpClient<OllamaService>("OllamaClient",client =>
{
    client.BaseAddress = new Uri(ollamaEndpoint);
    client.Timeout = isDevelopment ? Timeout.InfiniteTimeSpan : TimeSpan.FromMinutes(2);
}).ClearResilienceHandlers();

// Default HttpClient placed last to avoid interfering with custom ones above
builder.Services.AddHttpClient();
// DataTransferService used to store global vars until I understand passing params better
builder.Services.AddSingleton<DataTransferService>();
#endregion

#region Tools & Frameworks for UI
builder.Services.AddBootstrapBlazor(options =>
{
    options.ToastDelay = 8000;
    options.ToastPlacement = BootstrapBlazor.Components.Placement.TopCenter;
});
//builder.Services.AddBootstrapBlazorBaiduSpeech();
builder.Services.AddSpeechSynthesis();
builder.Services.AddSpeechRecognition();

builder.Services.AddFluentUIComponents();
builder.Services.AddDataGridEntityFrameworkAdapter();

// Used to tell the master AI Nav menu when a page has loaded
// that has AI functions so it shows icons
builder.Services.AddScoped<AppState>();

builder.Services.Configure<AnimationOptions>(Guid.NewGuid().ToString(), c => { });
#endregion

// Makes JSON Serializer safer & more robust
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new TypeConverter());
    });

// More Aspire Scaffolding
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpForwarderWithServiceDiscovery();

#region Add Security
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("PulseDbConn")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{     options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home";  // Redirect to login page
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);  // Cookie lifetime
    });

builder.Services.AddAuthorization();
#endregion
// Build App
var app = builder.Build();

// Post Build Configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();
app.MapStaticAssets();
// Seed Roles
await DatabaseSeeder.SeedRolesAsync(app.Services);
app.MapRazorComponents<Pulse.Web.Components.App>()
    .AddInteractiveServerRenderMode();
app.MapDefaultEndpoints();

// Let's Go!
app.Run();


