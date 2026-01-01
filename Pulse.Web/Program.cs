using Aspire.StackExchange.Redis;
using BlazorAnimation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.FluentUI.AspNetCore.Components;
using Pulse.Models.Misc;
using Pulse.Models.Users;
using Pulse.Web.Components;
using Pulse.Web.Services;
using Pulse.Web.Tools;
using System.Net.Http.Headers;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Aspire Scaffolding
builder.AddServiceDefaults();
builder.AddRedisDistributedCache("cache");
builder.AddRedisOutputCache("cache");
#endregion

#region Singleton Services (Pulse.AI, PulseApiService, OllamaService, DataTransferService)
builder.Services.AddScoped<Pulse_AI>();
builder.Services.AddScoped<PulseApiService>();
builder.Services.AddScoped<OllamaService>();
builder.Services.AddScoped<DataTransferService>();
#endregion

#region HttpClients (Consolidated)
var ollamaEndpoint = builder.Configuration["OllamaApi:EndpointHttp"] ?? throw new InvalidOperationException("Missing configuration for Ollama:Endpoint");
var pulseApiEndpoint = builder.Configuration["PulseApi:Endpoint"]
    ?? throw new InvalidOperationException("Missing configuration for PulseApi:Endpoint");
// OllamaClient (no Polly; timeout set directly)
builder.Services.AddHttpClient<OllamaService>("OllamaClient", client =>
{
    client.BaseAddress = new Uri(ollamaEndpoint);
    client.Timeout = Timeout.InfiniteTimeSpan; // For long-running Ollama requests
}).ClearResilienceHandlers(); 

// ApiClient (with service discovery for Aspire; no hardcoded port)
builder.Services.AddHttpClient<PulseApiService>("PulseApiClient", client =>
{
    client.BaseAddress = new Uri(pulseApiEndpoint);
    client.Timeout = Timeout.InfiniteTimeSpan; // For long-running Ollama requests
})
    .ClearResilienceHandlers()
    .AddHttpMessageHandler<AuthHeaderHandler>();
#endregion

#region Auth and Circuit Services
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "PulseApp",
//        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PulseApp",
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKey-Min32CharsLong"))
//    };
//});
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthHeaderHandler>();

builder.Services.AddScoped<ICircuitState, CircuitState>();
builder.Services.AddScoped<CircuitHandler>(sp =>
    new CircuitIdService(
        sp.GetRequiredService<ICircuitState>(),
        sp.GetRequiredService<ILogger<CircuitIdService>>()
    ));
//builder.Services.AddScoped<CircuitIdService>();
//builder.Services.AddScoped<CircuitHandler>(sp => new CircuitIdService(sp.GetRequiredService<ILogger<CircuitIdService>>()));

//builder.Services.AddScoped<CircuitHandler, CircuitIdService>(); // Shared instance for lifecycle and injection
builder.Services.AddCascadingAuthenticationState();
#endregion



#region UI Tools and Frameworks
builder.Services.AddBootstrapBlazor(options =>
{
    options.ToastDelay = 6000;
    options.ToastPlacement = BootstrapBlazor.Components.Placement.BottomEnd;
});
builder.Services.AddSpeechSynthesis();
builder.Services.AddSpeechRecognition();
builder.Services.AddFluentUIComponents();
builder.Services.AddDataGridEntityFrameworkAdapter();
builder.Services.AddScoped<AppState>();
builder.Services.Configure<AnimationOptions>(Guid.NewGuid().ToString(), c => { });
#endregion

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new TypeConverter());
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;  // Dev only - shows full stack in browser console
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(5);  // Retain longer for reconnects
    });


builder.Services.AddHttpForwarderWithServiceDiscovery();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<MessageHubService>();


// Build App
var app = builder.Build();

// Post-Build Configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseOutputCache();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

// Seed Roles (use a scoped service)
//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    string[] roles = ["Admin", "User"];
//    foreach (var role in roles)
//    {
//        if (!await roleManager.RoleExistsAsync(role))
//            await roleManager.CreateAsync(new IdentityRole(role));
//    }
//}

//app.MapBlazorHub();
//app.MapHub<NotificationHub>("/notificationHub");
app.MapDefaultEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();