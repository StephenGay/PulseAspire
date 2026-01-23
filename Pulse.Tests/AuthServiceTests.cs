using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using Pulse.Web.Services;
using Pulse.Models.Users;

[TestClass]
public class AuthServiceTests
{
    private static IHttpClientFactory CreateHttpClientFactoryReturning(HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response)
           .Verifiable();

        var client = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        return factoryMock.Object;
    }

    [TestMethod]
    public async Task Login_Success_StoresTokenInCache()
    {
        // Arrange
        var tokenJson = "{\"token\":\"abc123\"}";
        var httpFactory = CreateHttpClientFactoryReturning(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(tokenJson, Encoding.UTF8, "application/json")
        });

        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var circuitStateMock = new Mock<ICircuitState>();
        circuitStateMock.SetupGet(c => c.CurrentCircuitId).Returns("test-circuit");

        var jwtProvider = new JwtAuthenticationStateProvider(cache, circuitStateMock.Object);
        var loggerMock = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(httpFactory, jwtProvider, loggerMock.Object);

        // Act
        var result = await authService.Login(new LoginModel { Email = "u", Password = "p" });

        // Assert
        Assert.AreEqual("Success", result);
        var cacheKey = $"jwt:test-circuit";
        var bytes = await cache.GetAsync(cacheKey);
        Assert.IsNotNull(bytes);
        var token = Encoding.UTF8.GetString(bytes);
        Assert.AreEqual("abc123", token);
    }

    [TestMethod]
    public async Task Login_Failure_ReturnsError()
    {
        // Arrange: server returns 401
        var httpFactory = CreateHttpClientFactoryReturning(new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var circuitStateMock = new Mock<ICircuitState>();
        circuitStateMock.SetupGet(c => c.CurrentCircuitId).Returns("c");
        var jwtProvider = new JwtAuthenticationStateProvider(cache, circuitStateMock.Object);
        var loggerMock = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(httpFactory, jwtProvider, loggerMock.Object);

        // Act
        var result = await authService.Login(new LoginModel { Email = "u", Password = "p" });

        // Assert
        Assert.AreEqual("Error", result);
    }

    [TestMethod]
    public async Task Logout_RemovesTokenFromCache()
    {
        // Arrange
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var circuitStateMock = new Mock<ICircuitState>();
        circuitStateMock.SetupGet(c => c.CurrentCircuitId).Returns("logout-cid");
        var jwtProvider = new JwtAuthenticationStateProvider(cache, circuitStateMock.Object);
        var loggerMock = new Mock<ILogger<AuthService>>();

        // Pre-seed cache as MarkUserAsAuthenticated would
        var key = $"jwt:logout-cid";
        await cache.SetAsync(key, Encoding.UTF8.GetBytes("tok"));

        var httpFactoryMock = new Mock<IHttpClientFactory>();
        var authService = new AuthService(httpFactoryMock.Object, jwtProvider, loggerMock.Object);

        // Act
        await authService.Logout();

        // Assert
        var bytes = await cache.GetAsync(key);
        Assert.IsTrue(bytes == null || bytes.Length == 0);
    }
}