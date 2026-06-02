using Aspire.StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using Pulse.ApiService;
using Pulse.ApiService.Endpoints;
using Pulse.ApiService.Endpoints.Production;
using Pulse.ApiService.Endpoints.Security;
using Pulse.ApiService.Extensions;
using Pulse.ApiService.Hubs;
using Pulse.ApiService.Middleware;
using Pulse.ApiService.PulseAI.Characters;
using Pulse.ApiService.PulseAI.Endpoints;

using Pulse.ApiService.PulseAI.Services;
using Pulse.ApiService.Security;
using Pulse.ApiService.Services;
using Pulse.Models;
using Pulse.Models.Communication;
using Pulse.Models.PulseContext;
using Pulse.Models.Users;
using Scalar.AspNetCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

#region Aspire & Infrastructure

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.AddRedisDistributedCache("cache");

#endregion

#region Database

builder.AddSqlServerDbContext<PulseDbContext>("dbPulse",
    configureDbContextOptions: options =>
    {
        options.UseSqlServer(
        sqlOptions => sqlOptions.MigrationsAssembly("Pulse.ApiService")
        );
    });
builder.Services.AddDbContextFactory<PulseDbContext>(options =>
    options.UseSqlServer());

#endregion

#region Authentication & Authorization

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = Encoding.UTF8.GetBytes(
    jwtSettings["Key"] ?? throw new InvalidOperationException("Missing Jwt:Key"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
    .AddEntityFrameworkStores<PulseDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "Pulse.ApiService",
            ValidAudience = jwtSettings["Audience"] ?? "Pulse.Clients",
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ClockSkew = TimeSpan.FromSeconds(5),
            NameClaimType = JwtRegisteredClaimNames.UniqueName,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role  // ADD THIS LINE
        };

        // Critical for SignalR: Extract JWT from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/messagehub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("User", policy =>
        policy.RequireRole("User", "Admin"));
});

#endregion

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policyBuilder =>
    {
        var blazorOrigin = builder.Configuration["Aspire:Services:PulseWebUI:Http:0"] ?? "https://localhost:7219";
        //?? throw new InvalidOperationException("Missing Blazor UI endpoint configuration");

        policyBuilder
            .WithOrigins(blazorOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

#endregion


builder.AddOllamaApiClient("PulseAI").AddChatClient();

#region Pulse AI Ollama Client Integration

//builder.Services.ConfigureHttpClientDefaults(http =>
//{
//    // Don't add resilience - we'll control it per client
//    http.T .UseSocketsHttpHandler((handler, sp) =>
//    {
//        handler.PooledConnectionLifetime = TimeSpan.FromMinutes(15);
//    });
//});
builder.Services.AddScoped<FlapperOllamaAPI>(sp =>
{
    var flapperOllamaClient = sp.GetRequiredService<IOllamaApiClient>();
    var logger = sp.GetRequiredService<ILogger<FlapperOllamaAPI>>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new FlapperOllamaAPI(flapperOllamaClient, logger, httpClientFactory, configuration);
});

builder.Services.AddHttpClient("FlapperApiClient", client =>
{
    client.Timeout = Timeout.InfiniteTimeSpan;
});
//builder.Services.AddScoped<AliOllamaAPI>(sp =>
//{
//    var aliOllamaClient = sp.GetRequiredService<IOllamaApiClient>();
//    var logger = sp.GetRequiredService<ILogger<AliOllamaAPI>>();
//    return new AliOllamaAPI(aliOllamaClient, logger);
//});

#endregion

#region Pulse AI IChat Client Integration

// "ollama" must match your AppHost resource name

// 3. Configure HttpClient timeout + resilience for long Ollama calls
//builder.Services.AddHttpClient("PulseAiClient", client =>
//{
//    client.Timeout = Timeout.InfiniteTimeSpan;    // Overall timeout
//})
//    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
//    {
//        PooledConnectionLifetime = TimeSpan.FromMinutes(15),
//        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(15),
//        ConnectTimeout = TimeSpan.FromSeconds(30)
//    })
//.StopPollyTimeouts();





builder.Services.AddScoped<FlapperAPI>(sp =>
{
    var flapperClient = sp.GetRequiredService<IChatClient>();
    var logger = sp.GetRequiredService<ILogger<FlapperAPI>>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return new FlapperAPI(flapperClient, logger, configuration, httpClientFactory);
});

builder.Services.AddScoped<AliAPI>(sp =>
{
    var aliClient = sp.GetRequiredService<IChatClient>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var logger = sp.GetRequiredService<ILogger<AliAPI>>();
    return new AliAPI(aliClient, logger);
});

builder.Services.AddScoped<AliOllamaAPI>(sp =>
{
    var aliOllamaClient = sp.GetRequiredService<IOllamaApiClient>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var logger = sp.GetRequiredService<ILogger<AliOllamaAPI>>();
    return new AliOllamaAPI(aliOllamaClient, logger, httpClientFactory);
});
var pulseAiLocation = builder.Configuration["Aspire:Services:PulseAI:Http:0"] ?? "http://localhost:11434";

builder.Services.AddHttpClient("AliApiClient", client =>
{
    client.BaseAddress = new Uri(pulseAiLocation);
    client.Timeout = Timeout.InfiniteTimeSpan;
});

var remoteTablesUrl = builder.Configuration.GetConnectionString("RemoteAI")
                   ?? "http://127.0.0.1:11434";

var TablesAIUrl = "http://127.0.0.1:11434"; // builder.Configuration.GetConnectionString("RemoteAI") ?? "http://127.0.0.1:11434";

builder.Services.AddSingleton<IPulseAiClientFactory>(sp =>
    new PulseAiClientFactory(new Dictionary<string, string>
    {
        //{ "FlapperAliClient", PulseAIUrl },
        { "TablesClient", TablesAIUrl }
    }));

builder.Services.AddScoped<TablesAPI>();
builder.Services.AddHostedService<FlapperWarmUp>();


//builder.Services.AddHttpClient<TablesAPI>("tablesClient", client =>
//{
//    client.BaseAddress = new Uri(remoteTablesUrl);
//    client.Timeout = TimeSpan.FromMinutes(10);  // or whatever timeout you need
//})
//    .StopPollyTimeouts();


//builder.Services.AddScoped<IRollerModelGenerationService, RollerModelGenerationService>();

#endregion

#region Application Services

builder.Services.AddMemoryCache();
builder.Services.AddScoped<TokenService>();
builder.Services.AddSingleton<PresenceService>();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB limit for large responses
});
builder.Services.AddScoped<UserManager<ApplicationUser>>();
builder.Services.AddSingleton<AiShared>();
// Email Configuration
var emailConfig = builder.Configuration.GetSection("EmailConfiguration");
builder.Services.Configure<EmailConfiguration>(emailConfig);
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordGeneratorService, PasswordGeneratorService>();
builder.Services.AddHostedService<AliAnalysisWorker>();

// AI Services
builder.Services.AddScoped<TablesSQL>();

#endregion

#region JSON Configuration

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new TypeConverter());
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

#endregion

#region API Documentation

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

#endregion

var app = builder.Build();

// Authentication & Authorization
app.UseAuthentication();

#region Middleware Pipeline

// Security headers
app.UseMiddleware<SecurityHeadersMiddleware>();

// Exception handling (must be early)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Request/Response logging
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Servers = new ScalarServer[]
        {
            new ScalarServer("https://localhost:7466", "Development"),
            new ScalarServer("https://api.example.com", "Production")
        };
    });
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}



// CORS before routing
app.UseCors("AllowBlazor");

app.UseRouting();

app.UseAuthorization();

// WebSockets for SignalR
app.UseWebSockets();

#endregion

#region Endpoint Mapping

// Health checks
//app.MapHealthChecks("/health");

// API Endpoints
app.MapDefaultEndpoints();
app.MapCustomerEndpoints();
app.MapAiEndpoints();
app.MapSecurityEndpoints();
app.MapPermissionEndpoints();
app.MapRoleEndpoints();
app.MapUserEndpoints();
app.MapUtilitiesEndpoints();
app.MapDivisionEndpoints();
app.MapTechnicalEndpoints();
app.MapCompanyEndpoints();
app.MapWorkTypeEndpoints();
app.MapPulseAiEndpoints();
app.MapAliEndpoints();
app.MapFlapperEndpoints();

// SignalR Hub
app.MapHub<MessageHub>("/messagehub");

#endregion

#region Database Initialization

// Seed default roles
//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    var roles = new[] { "Admin", "User", "Manager", "Viewer" };

//    foreach (var role in roles)
//    {
//        if (!await roleManager.RoleExistsAsync(role))
//        {
//            await roleManager.CreateAsync(new IdentityRole(role));
//        }
//    }

//    app.Logger.LogInformation("Default roles ensured at startup");
//}

#endregion

await app.RunAsync();


#region old stuff

//var PulseAIUrl = builder.Configuration.GetConnectionString("LocalAI") ?? "http://localhost:11434";
//var TablesAIUrl = builder.Configuration.GetConnectionString("RemoteAI") ?? "http://192.168.0.5:11434";

//builder.Services.AddSingleton<IPulseAiClientFactory>(sp =>
//    new PulseAiClientFactory(new Dictionary<string, string>
//    {
//        { "FlapperAliClient", PulseAIUrl },
//        { "TablesClient", TablesAIUrl }
//    }));

//builder.Services.AddScoped<TablesAPI>();
//builder.Services.AddScoped<AliAPI>();
//builder.Services.AddScoped<MessageHub>();
//builder.Services.AddScoped<FlapperCharacter>();

//builder.Services.AddHttpClient("PulseAiClient", flapperClient =>
//{
//    flapperClient.Timeout = TimeSpan.FromMinutes(10);           // Overall request timeout
//})
//.AddStandardResilienceHandler(options =>
//{
//    options.Retry.MaxRetryAttempts = 2;
//    options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(9);
//    options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(8);   // Per attempt timeout
//});

//// 1. Containerized Ollama for Ali (Aspire-managed)
//builder.Services.AddOllamaApiClient("PulseAI", clientBuilder =>
//{
//    clientBuilder.ConfigureHttpClient(flapperClient => flapperClient.Timeout = TimeSpan.FromMinutes(10));
//});                  // Aspire injects the correct endpoint automatically
// .AddChatClient();     // Optional: clean IChatClient abstraction

// 2. Keep your factory / config for the remote Tables Ollama (different machine)
#endregion