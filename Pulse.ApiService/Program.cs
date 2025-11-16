
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Pulse.ApiService;
using Pulse.ApiService.Endpoints;
using Pulse.Models.PulseContext;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<PulseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PulseDbConn"),b => b.MigrationsAssembly("Pulse.ApiService")));

builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("UserAccess", policy => policy.RequireRole("user", "admin"));  // Allow user or admin
});

builder.Services.AddEndpointsApiExplorer(); // If using Swagger/OpenAPI
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new TypeConverter());
    // Optional: Ignore cycles if your schema has circular refs
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();



var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapCustomerEndpoints();
app.MapAiEndpoints();
app.MapSecurityEndpoints();
app.MapUserEndpoints();
app.MapUtilitiesEndpoints();
app.MapDivisionEndpoints();

app.UseHttpsRedirection();

await app.RunAsync();

