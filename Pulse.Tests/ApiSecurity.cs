using System.Net.Http.Json;
using Pulse.Models.Users;

namespace Pulse.Tests.Api;

[TestClass]
public class SecurityEndPoints
{
    [TestMethod]
    public async Task TestLoginMethod()
    {
        var appHost =
            await DistributedApplicationTestingBuilder.CreateAsync<Projects.Pulse_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var resourceNotificationService =
            app.Services.GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService
                .WaitForResourceAsync("PulseApi", KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromSeconds(30));
        var httpClient = app.CreateHttpClient("PulseApi");
        LoginModel loginData = new();
        
        loginData.Email = "SGAY1703@GMAIL.COM";
        loginData.Password = "$T3ph3n$$$$";
        var response = await httpClient.PostAsJsonAsync("/Security/login", loginData);
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode, "Login request failed");

    }

    [TestMethod]
    public async Task TestGetPulseUserMethod()
    {
        var appHost =
            await DistributedApplicationTestingBuilder.CreateAsync<Projects.Pulse_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var resourceNotificationService =
            app.Services.GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService
                .WaitForResourceAsync("PulseApi", KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromSeconds(30));
        var httpClient = app.CreateHttpClient("PulseApi");
        LoginModel loginData = new();

        loginData.Email = "SGAY1703@GMAIL.COM";
        
        var response = await httpClient.GetAsync($"/Security/EmployeeLogin/email={loginData.Email}");
        
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode, "Login request failed");

    }

}
