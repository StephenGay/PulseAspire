

using Aspire.StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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
    // Add Redis-distributed cache (for general use, if needed)
    builder.AddRedisDistributedCache("cache");
    builder.Services.AddSignalR();

// Add Redis output cache (for HTTP response caching)
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = builder.Configuration.GetConnectionString("cache");
//});
//builder.AddRedisOutputCache("cache");
#endregion

// Setup Connection to Database
builder.Services.AddDbContext<PulseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PulseDbConn"),b => b.MigrationsAssembly("Pulse.ApiService")));

#region Setup Identity and Authentication
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<PulseDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policyBuilder =>
    {
        policyBuilder
            .WithOrigins(builder.Configuration["Aspire:Services:PulseWebUI:Http:0"] ?? "https://localhost:7219") // Change to your Blazor app's URL
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Important for SignalR
    });
});
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
builder.Services.AddScoped<TokenService>();

// Add JWT Authentication
var jwtKey = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSecretKeyHere-Min32Chars"); // From appsettings or Aspire secrets
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "YourIssuer",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "YourAudience",
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
    };
});
builder.Services.AddAuthorization();
#endregion

// Makes JSON Serializer safer & more robust
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new TypeConverter());
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// More Aspire Scaffolding
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Build App
var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();
// Post Build Configuration
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// Ensure Roles are created in new Installs
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new IdentityRole("Admin"));
    if (!await roleManager.RoleExistsAsync("User")) await roleManager.CreateAsync(new IdentityRole("User"));
}

//app.MapGet("/secure-data", () => "Protected data").RequireAuthorization();
app.UseHttpsRedirection();
app.UseCors("AllowBlazor");
//app.UseOutputCache();
// Map Custom Endpoints
app.MapDefaultEndpoints();
app.MapCustomerEndpoints();
app.MapAiEndpoints();
app.MapSecurityEndpoints();
app.MapUserEndpoints();
app.MapUtilitiesEndpoints();
app.MapDivisionEndpoints();
app.MapTechnicalEndpoints();
app.MapCompanyEndpoints();
app.MapHub<MessageHub>("/messagehub");
// Let's Go!
await app.RunAsync();

