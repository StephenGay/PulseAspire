# Quick Start Guide - Role-Based Permissions System

## Installation Complete ✅

Your role-based permissions system is now fully integrated and ready to use!

## Access the System

1. **Log in** with an Admin account
2. **Navigate** to `/AdminZone/RoleManagementPg`
3. **You'll see two tabs:**
   - **Manage Permissions** - Create and view all permissions
   - **Assign Permissions** - Assign permissions to roles

## First Steps

### Step 1: Create Some Permissions
Go to "Manage Permissions" tab and create these starter permissions:

**User Management Category:**
- `View Users` - Can view the user list
- `Create User` - Can create new users
- `Edit User` - Can edit user details
- `Delete User` - Can delete users

**Reports Category:**
- `View Reports` - Can view all reports
- `Export Reports` - Can export report data
- `Delete Reports` - Can delete reports

**Settings Category:**
- `View Settings` - Can view system settings
- `Edit Settings` - Can modify system settings

### Step 2: Assign Permissions to Roles
Go to "Assign Permissions" tab:
1. Select a role (e.g., "Admin", "Manager", "User")
2. Check the permissions you want to assign
3. Changes save automatically

### Step 3: Implement in Your Pages

To enforce permissions in your Blazor components:

```razor
@page "/admin/users"
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize(Roles = "Admin")]

<div>
    @if (User.HasPermission("View Users"))
    {
        <button @onclick="ShowUserList">View Users</button>
    }
    
    @if (User.HasPermission("Create User"))
    {
        <button @onclick="ShowCreateForm">Create User</button>
    }
</div>

@code {
    [CascadingParameter]
    public Task<AuthenticationState> AuthenticationStateTask { get; set; }
    
    private async Task<bool> UserHasPermissionAsync(string permission)
    {
        var state = await AuthenticationStateTask;
        return state.User.HasClaim("permissions", permission);
    }
}
```

## API Endpoints Reference

### Get All Permissions
```
GET /Permissions
Authorization: Bearer {token}
```

Response:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "View Users",
      "description": "Can view user list",
      "category": "User Management"
    }
  ],
  "message": "Retrieved 5 permissions",
  "statusCode": 200
}
```

### Create Permission
```
POST /Permissions
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "View Reports",
  "description": "Can view all reports",
  "category": "Reports"
}
```

### Get Role Permissions
```
GET /Permissions/role/{roleId}
Authorization: Bearer {token}
```

### Assign Permission to Role
```
POST /Permissions/assign
Authorization: Bearer {token}
Content-Type: application/json

{
  "roleId": "role-uuid-here",
  "permissionId": 1
}
```

### Remove Permission from Role
```
DELETE /Permissions/remove/{roleId}/{permissionId}
Authorization: Bearer {token}
```

## Testing

1. **Create a test role:**
   - In User Management, create a new role "Tester"

2. **Assign some permissions:**
   - Go to Role Management
   - Select "Tester" role
   - Check "View Users" and "Create User" permissions

3. **Test in API:**
   - Use Swagger/Scalar UI at `/scalar`
   - Try the endpoints with your token

4. **Test in UI:**
   - Grant user "Tester" role
   - Verify they can see/do permitted actions

## Common Tasks

### Add a new permission category
1. Go to "Manage Permissions"
2. Create permission with new category name
3. Done! The category appears in dropdown for future permissions

### Check if current user has permission
```csharp
var hasPermission = User.FindAll("permissions")
    .Any(c => c.Value == "View Users");
```

### View all permissions for current user
```csharp
var userPermissions = User.FindAll("permissions")
    .Select(c => c.Value)
    .ToList();
```

## Troubleshooting

### Permissions not showing up
- Ensure you're logged in as Admin
- Check browser console for errors
- Verify database migrations ran: `dotnet ef database update`

### Can't create permissions
- Check that you have Admin role
- Verify API is running on correct port
- Check API logs for errors

### Role selection not working
- Clear browser cache
- Try refreshing the page
- Check console for JavaScript errors

## Customization

### Change Categories
Edit the dropdown in `RoleManagementPg.razor`:
```razor
<select @onchange="OnCategoryChanged">
    <option>Your Category</option>
</select>
```

### Customize Styling
The component uses CSS grid. Edit the `<style>` section in `RoleManagementPg.razor`

### Add Permission Validation
Add custom validators in `PermissionEndpoints.cs` `CreatePermission()` method

## Support

For issues or questions:
1. Check the `PERMISSIONS_SYSTEM_IMPLEMENTATION.md` file
2. Review API error messages
3. Check application logs
4. Inspect browser network tab for API responses

## Next Phase

Consider implementing:
- Permission hierarchies (parent/child permissions)
- Temporary permission grants
- Permission audit logging
- Permission-based UI element visibility
- Dynamic permission generation from route attributes
