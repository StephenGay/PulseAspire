# Permission Integration Examples

## Overview
This document shows how to integrate the new permissions system into your existing Blazor components.

## 1. Basic Permission Check in Components

### Example: User Management Page
```razor
@page "/AdminZone/UserManagementPg"
@using Microsoft.AspNetCore.Authorization
@using Pulse.Web.Services

@attribute [Authorize(Roles = "Admin")]
@rendermode InteractiveServer

@inject PulseApiService ApiService
@inject AuthenticationStateProvider AuthStateProvider

<div>
    @if (CanCreateUsers)
    {
        <FluentButton @onclick="AddNewUser">
            Add New User
        </FluentButton>
    }
    
    <FluentDataGrid Items="@users.AsQueryable()">
        <TemplateColumn Title="Actions" Context="user">
            @if (CanEditUsers)
            {
                <FluentButton Size="ButtonSize.Small" @onclick="() => EditUser(user)">
                    Edit
                </FluentButton>
            }
            
            @if (CanDeleteUsers)
            {
                <FluentButton Size="ButtonSize.Small" 
                            Appearance="Appearance.Neutral"
                            @onclick="() => DeleteUser(user.Id)">
                    Delete
                </FluentButton>
            }
        </TemplateColumn>
    </FluentDataGrid>
</div>

@code {
    private List<ApplicationUserDto> users = new();
    
    // Permission flags
    private bool CanViewUsers => UserHasPermission("View Users");
    private bool CanCreateUsers => UserHasPermission("Create User");
    private bool CanEditUsers => UserHasPermission("Edit User");
    private bool CanDeleteUsers => UserHasPermission("Delete User");
    
    protected override async Task OnInitializedAsync()
    {
        // Only load if user has permission
        if (CanViewUsers)
        {
            await LoadUsersAsync();
        }
    }
    
    private bool UserHasPermission(string permission)
    {
        var authState = AuthStateProvider.GetAuthenticationStateAsync().Result;
        return authState.User.FindAll("permissions")
            .Any(c => c.Value == permission);
    }
    
    private async Task LoadUsersAsync()
    {
        try
        {
            var response = await ApiService.GetAsync<ApiResponse<List<ApplicationUserDto>>>("/Security/users");
            users = response?.Data ?? new();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load users");
        }
    }
    
    private async Task AddNewUser()
    {
        if (!CanCreateUsers) return;
        // Implementation...
    }
    
    private async Task EditUser(ApplicationUserDto user)
    {
        if (!CanEditUsers) return;
        // Implementation...
    }
    
    private async Task DeleteUser(string userId)
    {
        if (!CanDeleteUsers) return;
        // Implementation...
    }
}
```

## 2. Create a Permission Helper Service

Create `Pulse.Web/Services/PermissionService.cs`:

```csharp
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Pulse.Web.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string permission);
        Task<List<string>> GetUserPermissionsAsync();
        bool HasPermission(string permission, ClaimsPrincipal user);
    }

    public class PermissionService : IPermissionService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(
            AuthenticationStateProvider authStateProvider,
            ILogger<PermissionService> logger)
        {
            _authStateProvider = authStateProvider;
            _logger = logger;
        }

        public async Task<bool> HasPermissionAsync(string permission)
        {
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            return HasPermission(permission, state.User);
        }

        public bool HasPermission(string permission, ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            return user.FindAll("permissions")
                .Any(c => c.Value.Equals(permission, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<string>> GetUserPermissionsAsync()
        {
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            return state.User.FindAll("permissions")
                .Select(c => c.Value)
                .ToList();
        }
    }
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddScoped<IPermissionService, PermissionService>();
```

## 3. Use Permission Service in Components

```razor
@page "/AdminZone/ReportsPg"
@using Microsoft.AspNetCore.Authorization
@using Pulse.Web.Services

@attribute [Authorize]
@rendermode InteractiveServer

@inject IPermissionService PermissionService

<div>
    @if (CanViewReports)
    {
        <h2>Reports</h2>
        <ReportsList />
    }
    else
    {
        <FluentMessageBar Severity="MessageSeverity.Warning">
            You don't have permission to view reports.
        </FluentMessageBar>
    }
    
    @if (CanExportReports)
    {
        <FluentButton @onclick="ExportReports">
            Export Reports
        </FluentButton>
    }
</div>

@code {
    private bool CanViewReports { get; set; }
    private bool CanExportReports { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        CanViewReports = await PermissionService.HasPermissionAsync("View Reports");
        CanExportReports = await PermissionService.HasPermissionAsync("Export Reports");
        
        if (!CanViewReports)
        {
            Logger.LogWarning("User attempted to access reports without permission");
        }
    }
    
    private async Task ExportReports()
    {
        if (!CanExportReports)
        {
            throw new UnauthorizedAccessException("You don't have permission to export reports");
        }
        // Export logic...
    }
}
```

## 4. Create Custom Authorization Attributes

Create `Pulse.Web/Authorization/RequirePermissionAttribute.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;

namespace Pulse.Web.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public RequirePermissionAttribute(params string[] permissions)
        {
            Permissions = permissions;
            Policy = $"Permission_{string.Join("_", permissions)}";
        }

        public string[] Permissions { get; }
    }
}
```

## 5. Create Authorization Handler

Create `Pulse.Web/Authorization/PermissionAuthorizationHandler.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;

namespace Pulse.Web.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string[] Permissions { get; }

        public PermissionRequirement(params string[] permissions)
        {
            Permissions = permissions;
        }
    }

    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var userPermissions = context.User
                .FindAll("permissions")
                .Select(c => c.Value)
                .ToList();

            if (requirement.Permissions.Any(p => userPermissions.Contains(p)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
```

Register in `Program.cs`:
```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanManageUsers", policy =>
        policy.Requirements.Add(new PermissionRequirement("Create User", "Edit User", "Delete User")))
    .AddPolicy("CanViewReports", policy =>
        policy.Requirements.Add(new PermissionRequirement("View Reports")));

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
```

## 6. Use in Components with Policy

```razor
@page "/AdminZone/UserManagementPg"
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize(Policy = "CanManageUsers")]

<!-- Component only renders if user has required permissions -->
<UserManagementComponent />
```

## 7. Dynamic Permission UI Example

```razor
@page "/AdminZone/DashboardPg"
@using Pulse.Web.Services
@inject IPermissionService PermissionService

<FluentStack Orientation="Orientation.Vertical" Style="gap: 16px;">
    @foreach (var card in DashboardCards)
    {
        @if (card.RequiredPermission == null || CanAccess(card.RequiredPermission))
        {
            <DashboardCard Title="@card.Title" 
                          Description="@card.Description"
                          @onclick="() => Navigate(card.Route)" />
        }
    }
</FluentStack>

@code {
    private List<DashboardCardConfig> DashboardCards;
    
    protected override async Task OnInitializedAsync()
    {
        DashboardCards = new List<DashboardCardConfig>
        {
            new("Users", "Manage system users", "CanViewUsers", "/AdminZone/UserManagementPg"),
            new("Reports", "View reports", "CanViewReports", "/AdminZone/ReportsPg"),
            new("Settings", "System settings", "CanEditSettings", "/AdminZone/SettingsPg"),
            new("Permissions", "Manage permissions", null, "/AdminZone/RoleManagementPg"), // Always visible
        };
    }
    
    private bool CanAccess(string permission)
    {
        // Get from UI state or permission service
        return true;
    }
    
    private class DashboardCardConfig
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string RequiredPermission { get; set; }
        public string Route { get; set; }

        public DashboardCardConfig(string title, string desc, string perm, string route)
        {
            Title = title;
            Description = desc;
            RequiredPermission = perm;
            Route = route;
        }
    }
}
```

## 8. Menu Navigation with Permissions

```razor
<!-- MasterLayout.razor or Navigation component -->
@using Pulse.Web.Services
@inject IPermissionService PermissionService
@inject NavigationManager Nav

<NavLink href="/AdminZone/UserManagementPg" Match="NavLinkMatch.All">
    @if (await PermissionService.HasPermissionAsync("View Users"))
    {
        <span>👥 Users</span>
    }
</NavLink>

<NavLink href="/AdminZone/ReportsPg" Match="NavLinkMatch.All">
    @if (await PermissionService.HasPermissionAsync("View Reports"))
    {
        <span>📊 Reports</span>
    }
</NavLink>

<NavLink href="/AdminZone/RoleManagementPg" Match="NavLinkMatch.All">
    @if (await PermissionService.HasPermissionAsync("Manage Permissions"))
    {
        <span>🔐 Permissions</span>
    }
</NavLink>
```

## 9. Check Permissions in Code

```csharp
// In any service or component code
public async Task DeleteUserAsync(string userId)
{
    var hasPermission = await _permissionService.HasPermissionAsync("Delete User");
    
    if (!hasPermission)
    {
        throw new UnauthorizedAccessException("You don't have permission to delete users");
    }
    
    // Proceed with deletion
    await _apiService.DeleteAsync($"/Security/users/{userId}");
}
```

## 10. Logging Permission Access

```csharp
private async Task<bool> HasPermissionWithLoggingAsync(string permission)
{
    var hasPermission = await PermissionService.HasPermissionAsync(permission);
    
    if (hasPermission)
    {
        _logger.LogInformation("User accessed permitted action: {Permission}", permission);
    }
    else
    {
        _logger.LogWarning("User attempted unauthorized action: {Permission}", permission);
    }
    
    return hasPermission;
}
```

## Summary

With these patterns, you can:
- ✅ Hide UI elements based on permissions
- ✅ Prevent API calls if unauthorized
- ✅ Log permission access
- ✅ Create dynamic menus
- ✅ Enforce permissions at multiple levels
- ✅ Make permission checks reusable across components

Choose the pattern that works best for your use case!
