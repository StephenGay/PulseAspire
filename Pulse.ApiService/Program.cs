
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pulse.ApiService;
using Pulse.ApiService.Endpoints;
using Pulse.ApiService.Security;
using Pulse.Models;
using Pulse.Models.PulseContext;
using Pulse.Models.Users;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

#region Add Aspire Scaffolding
    builder.AddServiceDefaults();
    builder.Services.AddProblemDetails();
#endregion

// Setup Connection to Database
builder.Services.AddDbContext<PulseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PulseDbConn"),b => b.MigrationsAssembly("Pulse.ApiService")));

#region Setup Identity and Authentication
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("PulseDbConn"), b => b.MigrationsAssembly("Pulse.ApiService")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Below code to be changed for production use with proper CORS settings
builder.Services.AddCors(options => options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddAuthorization();
builder.Services.AddScoped<TokenService>();
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

// Post Build Configuration
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//app.MapGet("/secure-data", () => "Protected data").RequireAuthorization();
app.UseHttpsRedirection();

// Map Custom Endpoints
app.MapDefaultEndpoints();
app.MapCustomerEndpoints();
app.MapAiEndpoints();
app.MapSecurityEndpoints();
app.MapUserEndpoints();
app.MapUtilitiesEndpoints();
app.MapDivisionEndpoints();
app.MapTechnicalEndpoints();

// Let's Go!
await app.RunAsync();

