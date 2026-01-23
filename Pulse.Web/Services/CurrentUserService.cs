// Services/CurrentUserService.cs
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}

// Services/CurrentUserService.cs


public class CurrentUserService : ICurrentUserService
{
    private readonly AuthenticationStateProvider AuthStateProvider;
    public string? UserId { get; }
    public string? UserName { get; }
    public bool IsAuthenticated { get; }

    public CurrentUserService()
    {
        
        var authState = AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.Result.User;

        if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
        {
            IsAuthenticated = true;
            UserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            UserName = user.Identity.Name;
        }
        else
        {
            IsAuthenticated = false;
        }
    }
}
