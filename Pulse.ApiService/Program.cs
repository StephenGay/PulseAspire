using Aspire.StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pulse.ApiService;
using Pulse.ApiService.Endpoints;
using Pulse.ApiService.Hubs;
using Pulse.ApiService.Security;
using Pulse.Models;
using Pulse.Models.PulseContext;
using Pulse.Models.Users;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

#region Add Aspire Scaffolding
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

builder.AddRedisDistributedCache("cache");
#endregion

// Database
var strConn = builder.Configuration["PulseSqlDb:ConnectionString"];
builder.Services.AddDbContext<PulseDbContext>(
    options => options.UseSqlServer(strConn, b => b.MigrationsAssembly("Pulse.ApiService")),
    contextLifetime: Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped,
    optionsLifetime: Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton);

builder.Services.AddDbContextFactory<PulseDbContext>(options =>
    options.UseSqlServer(strConn, b => b.MigrationsAssembly("Pulse.ApiService")));

#region Identity, CORS, JWT & Authorization
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<PulseDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policyBuilder =>
    {
        policyBuilder
            .WithOrigins(builder.Configuration["Aspire:Services:PulseWebUI:Http:0"] ?? "https://localhost:7219")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Required for SignalR + cookies if ever used
    });
});

builder.Services.AddScoped<TokenService>();

var jwtKey = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKey-Minimum32Chars1234567890");

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
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Pulse.ApiService",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Pulse.Clients",
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
    };

    // Critical for SignalR: Extract JWT from query string (access_token=...) during WebSocket upgrade
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

builder.Services.AddAuthorization();
#endregion
builder.Services.AddSignalR(options => options.EnableDetailedErrors = true);

builder.Services.AddSingleton<PresenceService>();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new TypeConverter());
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

#region Middleware Pipeline (Optimized Order)
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// CORS early for preflight OPTIONS on /messagehub/negotiate
app.UseCors("AllowBlazor");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets(); // Required for SignalR
#endregion

// Seed default roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new IdentityRole("Admin"));
    if (!await roleManager.RoleExistsAsync("User")) await roleManager.CreateAsync(new IdentityRole("User"));
}

// Map endpoints & protected hub
app.MapDefaultEndpoints();
app.MapCustomerEndpoints();
app.MapAiEndpoints();
app.MapSecurityEndpoints();
app.MapUserEndpoints();
app.MapUtilitiesEndpoints();
app.MapDivisionEndpoints();
app.MapTechnicalEndpoints();
app.MapCompanyEndpoints();

app.MapHub<MessageHub>("/messagehub");//.RequireAuthorization(); // Enforces JWT auth on hub methods

await app.RunAsync();

//var builder = WebApplication.CreateBuilder(args);

//#region Add Aspire Scaffolding
//    builder.AddServiceDefaults();
//    builder.Services.AddProblemDetails();

//    builder.AddRedisDistributedCache("cache");
//    builder.Services.AddSignalR(options => options.EnableDetailedErrors = true);  // For logging reconnect failures


//#endregion

//// Setup Connection to Database
//var strConn = builder.Configuration["PulseSqlDb:ConnectionString"];
//builder.Services.AddDbContext<PulseDbContext>(options =>
//    options.UseSqlServer(strConn, b => b.MigrationsAssembly("Pulse.ApiService")));

////builder.Services.AddDbContext<PulseDbContext>(options =>
////    options.UseSqlServer(builder.Configuration.GetConnectionString("PulseDbConn"), b => b.MigrationsAssembly("Pulse.ApiService")));


//#region Setup Identity and Authentication
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//    .AddEntityFrameworkStores<PulseDbContext>()
//    .AddDefaultTokenProviders();

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowBlazor", policyBuilder =>
//    {
//        policyBuilder
//            .WithOrigins(builder.Configuration["Aspire:Services:PulseWebUI:Http:0"] ?? "https://localhost:7219") // Change to your Blazor app's URL
//            .AllowAnyHeader()
//            .AllowAnyMethod()
//            .AllowCredentials(); // Important for SignalR
//    });
//});

//builder.Services.AddScoped<TokenService>();

//// Add JWT Authentication
//var jwtKey = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSecretKeyHere-Min32Chars"); // From appsettings or Aspire secrets
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "YourIssuer",
//        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "YourAudience",
//        IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
//    };
//});
//builder.Services.AddAuthorization();
//#endregion
//builder.Services.AddSingleton<PresenceService>(); 
//// Makes JSON Serializer safer & more robust
//builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
//{
//    options.SerializerOptions.Converters.Add(new TypeConverter());
//    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
//});

//// More Aspire Scaffolding
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddOpenApi();

//// Build App
//var app = builder.Build();

//app.UseRouting();

//app.UseAuthentication();
//app.UseAuthorization();

//app.UseWebSockets();
//// Post Build Configuration
//app.UseExceptionHandler();
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}



//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    // Ensure Roles are created in new Installs
//    if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new IdentityRole("Admin"));
//    if (!await roleManager.RoleExistsAsync("User")) await roleManager.CreateAsync(new IdentityRole("User"));
//}

////app.MapGet("/secure-data", () => "Protected data").RequireAuthorization();
//app.UseHttpsRedirection();
//app.UseCors("AllowBlazor");
////app.UseOutputCache();
//// Map Custom Endpoints
//app.MapDefaultEndpoints();
//app.MapCustomerEndpoints();
//app.MapAiEndpoints();
//app.MapSecurityEndpoints();
//app.MapUserEndpoints();
//app.MapUtilitiesEndpoints();
//app.MapDivisionEndpoints();
//app.MapTechnicalEndpoints();
//app.MapCompanyEndpoints();



//app.MapHub<MessageHub>("/messagehub"); //.RequireAuthorization();
//// Let's Go!
//await app.RunAsync();

// Add Redis-distributed cache (for general use, if needed)

// Add Redis output cache (for HTTP response caching)
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = builder.Configuration.GetConnectionString("cache");
//});
//builder.AddRedisOutputCache("cache");

// Below code to be changed for production use with proper CORS settings
//builder.Services.AddCors(options => options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//        };
//    })
//    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

//builder.Services.AddAuthorization();