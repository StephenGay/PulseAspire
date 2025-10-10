using Microsoft.EntityFrameworkCore;
using Pulse.ApiService.Endpoints;
using Pulse.Models.PulseContext;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<PulseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PulseDbConn"),b => b.MigrationsAssembly("Pulse.ApiService")));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();
app.MapCustomerEndpoints();
app.MapAiEndpoints();
app.MapSecurityEndpoints();

app.UseHttpsRedirection();

await app.RunAsync();

