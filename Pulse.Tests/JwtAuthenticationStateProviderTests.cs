using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pulse.Web.Services;

[TestClass]
public class JwtAuthenticationStateProviderTests
{
    [TestMethod]
    public async Task MarkUserAsAuthenticated_PersistsToken_And_GetAuthenticationStateReturnsClaims()
    {
        // Arrange
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var circuitStateMock = new Mock<ICircuitState>();
        circuitStateMock.SetupGet(c => c.CurrentCircuitId).Returns("jwt-cid");

        var provider = new JwtAuthenticationStateProvider(cache, circuitStateMock.Object);

        // Create a simple JWT token with a claim so ReadJwtToken can parse it
        var handler = new JwtSecurityTokenHandler();
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123"), new Claim(ClaimTypes.Name, "tester") }, "jwt");
        var token = handler.CreateJwtSecurityToken(subject: identity, expires: DateTime.UtcNow.AddMinutes(30));
        var tokenString = handler.WriteToken(token);

        // Act
        await provider.MarkUserAsAuthenticated(tokenString);

        // Assert - cache contains token
        var cacheKey = $"jwt:jwt-cid";
        var bytes = await cache.GetAsync(cacheKey);
        Assert.IsNotNull(bytes);
        var cached = Encoding.UTF8.GetString(bytes);
        Assert.AreEqual(tokenString, cached);

        // And GetAuthenticationStateAsync returns ClaimsPrincipal with expected claims
        var authState = await provider.GetAuthenticationStateAsync();
        Assert.IsNotNull(authState);
        var principal = authState.User;
        Assert.IsTrue(principal.Identity?.IsAuthenticated == true);
        Assert.IsTrue(principal.HasClaim(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "user-123"));
    }

    [TestMethod]
    public async Task MarkUserAsLoggedOut_RemovesToken()
    {
        // Arrange
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var circuitStateMock = new Mock<ICircuitState>();
        circuitStateMock.SetupGet(c => c.CurrentCircuitId).Returns("out-cid");

        var provider = new JwtAuthenticationStateProvider(cache, circuitStateMock.Object);

        var key = $"jwt:out-cid";
        await cache.SetAsync(key, Encoding.UTF8.GetBytes("sometoken"));

        // Act
        await provider.MarkUserAsLoggedOut();

        // Assert
        var bytes = await cache.GetAsync(key);
        Assert.IsTrue(bytes == null || bytes.Length == 0);
    }
}