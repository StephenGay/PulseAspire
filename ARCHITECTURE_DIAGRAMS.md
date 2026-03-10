# Pulse Permissions System - Architecture Diagram

## System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    BLAZOR WEB APPLICATION                        │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  /AdminZone/RoleManagementPg.razor                       │   │
│  │  - Manage Permissions Tab                                │   │
│  │    * Create permission (name, desc, category)            │   │
│  │    * View all permissions by category                    │   │
│  │  - Assign Permissions Tab                                │   │
│  │    * Select role from dropdown                           │   │
│  │    * Toggle permissions with checkboxes                  │   │
│  └──────────────┬───────────────────────────────────────────┘   │
│                 │ HTTP                                            │
│                 │ /Permissions endpoints                          │
│                 ▼                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  PulseApiService                                         │   │
│  │  - GetAsync<T>()                                         │   │
│  │  - PostAsync<TReq, TRes>()                              │   │
│  │  - DeleteAsync<T>() [NEW]                              │   │
│  └──────────────┬───────────────────────────────────────────┘   │
└─────────────────┼──────────────────────────────────────────────┘
                  │ JWT Bearer Token
                  │
┌─────────────────▼──────────────────────────────────────────────┐
│              API SERVICE (Pulse.ApiService)                     │
│                                                                  │
│  ┌────────────────────────────────────────────────────────┐    │
│  │  PermissionEndpoints.cs                                │    │
│  │  ┌──────────────────────────────────────────────────┐  │    │
│  │  │ GET /Permissions                                │  │    │
│  │  │ - Returns: List<PermissionDto>                 │  │    │
│  │  │ - Auth: Admin only                             │  │    │
│  │  └──────────────────────────────────────────────────┘  │    │
│  │  ┌──────────────────────────────────────────────────┐  │    │
│  │  │ POST /Permissions (Create)                       │  │    │
│  │  │ - Input: CreatePermissionModel                  │  │    │
│  │  │ - Returns: PermissionDto                        │  │    │
│  │  │ - Auth: Admin only                             │  │    │
│  │  └──────────────────────────────────────────────────┘  │    │
│  │  ┌──────────────────────────────────────────────────┐  │    │
│  │  │ GET /Permissions/role/{roleId}                  │  │    │
│  │  │ - Returns: RolePermissionDto                    │  │    │
│  │  │ - Auth: Admin only                             │  │    │
│  │  └──────────────────────────────────────────────────┘  │    │
│  │  ┌──────────────────────────────────────────────────┐  │    │
│  │  │ POST /Permissions/assign                         │  │    │
│  │  │ - Input: AssignPermissionModel                  │  │    │
│  │  │ - Auth: Admin only                             │  │    │
│  │  └──────────────────────────────────────────────────┘  │    │
│  │  ┌──────────────────────────────────────────────────┐  │    │
│  │  │ DELETE /Permissions/remove/{roleId}/{permId}   │  │    │
│  │  │ - Auth: Admin only                             │  │    │
│  │  └──────────────────────────────────────────────────┘  │    │
│  └────────┬───────────────────────────────────────────────┘    │
└───────────┼──────────────────────────────────────────────────┘
            │ Entity Framework
            │
┌───────────▼──────────────────────────────────────────────────┐
│         SQL SERVER DATABASE                                  │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Permissions Table                                   │   │
│  │ ┌──────────┬───────────┬───────────┬──────────────┐ │   │
│  │ │ Id (PK)  │ Name      │ Category  │ Description  │ │   │
│  │ ├──────────┼───────────┼───────────┼──────────────┤ │   │
│  │ │ 1        │View Users │ User Mgmt │ Can view... │ │   │
│  │ │ 2        │Edit Users │ User Mgmt │ Can edit... │ │   │
│  │ │ 3        │View Rpts  │ Reports   │ Can view... │ │   │
│  │ └──────────┴───────────┴───────────┴──────────────┘ │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ RolePermissions (Junction Table)                    │   │
│  │ ┌──────────┬──────────┬──────────┐                 │   │
│  │ │ Id (PK)  │ RoleId   │ PermId   │                 │   │
│  │ ├──────────┼──────────┼──────────┤                 │   │
│  │ │ 1        │ admin-id │ 1 (FK)   │ ─┐              │   │
│  │ │ 2        │ admin-id │ 2 (FK)   │  │ Admin role   │   │
│  │ │ 3        │ admin-id │ 3 (FK)   │ ─┘ permissions │   │
│  │ │ 4        │ user-id  │ 1 (FK)   │ ─┐              │   │
│  │ │ 5        │ user-id  │ 3 (FK)   │ ─┘ User role   │   │
│  │ │          │          │          │    permissions │   │
│  │ └──────────┴──────────┴──────────┘                 │   │
│  └─────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────┘
```

## Data Flow Diagram

```
┌──────────────┐
│ Admin User   │
└──────┬───────┘
       │ 1. Navigate to /AdminZone/RoleManagementPg
       │
       ▼
┌──────────────────────────────────────────────┐
│ RoleManagementPg Component                   │
│                                              │
│ ┌──────────────────────────────────────┐   │
│ │ CreatePermissionAsync()              │   │
│ │ 1. Get user input (name, desc, cat) │   │
│ │ 2. Call ApiService.PostAsync()      │   │
│ └──────┬───────────────────────────────┘   │
│        │ 2. POST /Permissions
│        │    + CreatePermissionModel
│        │
│        ▼
│ ┌──────────────────────────────────────┐   │
│ │ ApiService                           │   │
│ │ - Serializes request                 │   │
│ │ - Adds JWT token                     │   │
│ │ - Sends to API                       │   │
│ └──────┬───────────────────────────────┘   │
└────────┼──────────────────────────────────┘
         │ 3. HTTP POST /Permissions
         │
         ▼
┌──────────────────────────────────────┐
│ PermissionEndpoints.CreatePermission │
│                                      │
│ 1. Validate input                    │
│ 2. Check authorization (Admin)       │
│ 3. Check for duplicates              │
│ 4. Create new Permission entity      │
│ 5. Save to database                  │
│ 6. Return PermissionDto              │
└──────┬───────────────────────────────┘
       │ 4. HTTP 200 OK
       │    + PermissionDto
       │
       ▼
┌──────────────────────────────────────┐
│ RoleManagementPg Component           │
│                                      │
│ 1. Receive response                  │
│ 2. Show success toast                │
│ 3. Play PulseAI voice message        │
│ 4. Reload permissions list           │
│ 5. Clear input fields                │
└──────────────────────────────────────┘
```

## Class Diagram

```
┌────────────────────────────┐
│      Permission            │
├────────────────────────────┤
│ + Id: int                  │
│ + Name: string             │
│ + Description: string      │
│ + Category: string         │
│ + CreatedDate: DateTime    │
├────────────────────────────┤
│ + RolePermissions: List    │
└────────────────┬───────────┘
                 │ 1..* has many
                 │
                 │
┌────────────────▼───────────┐
│   RolePermission           │
├────────────────────────────┤
│ + Id: int                  │
│ + RoleId: string (FK)      │
│ + PermissionId: int (FK)   │
│ + AssignedDate: DateTime   │
├────────────────────────────┤
│ + Permission: Permission   │
└────────────────────────────┘

┌────────────────────────────┐
│    PermissionDto           │
├────────────────────────────┤
│ + Id: int                  │
│ + Name: string             │
│ + Description: string      │
│ + Category: string         │
└────────────────────────────┘

┌────────────────────────────┐
│  RolePermissionDto         │
├────────────────────────────┤
│ + RoleId: string           │
│ + RoleName: string         │
│ + Permissions: List<Perm>  │
└────────────────────────────┘
```

## API Call Sequence

```
┌─────────────┐          ┌──────────────┐          ┌──────────────┐
│   Client    │          │  API Server  │          │   Database   │
└─────┬───────┘          └──────┬───────┘          └──────┬───────┘
      │                         │                         │
      │ 1. GET /Permissions      │                         │
      │ (with JWT token)         │                         │
      ├────────────────────────>│                         │
      │                         │ 2. Authorize (Admin)     │
      │                         │─┐                        │
      │                         │ │                        │
      │                         │<┘                        │
      │                         │                         │
      │                         │ 3. SELECT * FROM Perms   │
      │                         ├────────────────────────>│
      │                         │                    List of
      │                         │ 4. Return results        │
      │                         │<────────────────────────┤
      │                         │                         │
      │<────────────────────────┤                         │
      │ 5. JSON response         │                         │
      │    [PermissionDto...]    │                         │
      │                         │                         │
      │ 6. Deserialize           │                         │
      │─┐                        │                         │
      │ │                        │                         │
      │<┘                        │                         │
      │                         │                         │
      │ 7. Update UI             │                         │
      │ - Show permissions       │                         │
      │ - Display categories     │                         │
      │─┐                        │                         │
      │ │                        │                         │
      │<┘                        │                         │
```

## User Permission Assignment Flow

```
┌─────────────────┐
│  Select Role    │
└────────┬────────┘
         │ Role ID
         ▼
┌─────────────────────────────────┐
│ LoadRolePermissionsAsync()       │
│ - GET /Permissions/role/{id}   │
│ - Fetch assigned permissions    │
└────────┬────────────────────────┘
         │ RolePermissionDto
         ▼
┌─────────────────────────────────┐
│ Display All Permissions         │
│ - Show 3 categories             │
│ - Checkbox for each permission  │
│ - Check if currently assigned   │
└────────┬────────────────────────┘
         │ User interaction
         ├─ Check permission  (Want to assign)
         │
         ▼
┌─────────────────────────────────┐
│ TogglePermissionAsync()          │
│ - POST /Permissions/assign      │
│ - { roleId, permissionId }      │
└────────┬────────────────────────┘
         │ Success response
         ▼
┌─────────────────────────────────┐
│ Show Toast: "Permission assigned"│
│ Play PulseAI voice notification │
│ Reload permissions list         │
└─────────────────────────────────┘
```

## Authorization Flow

```
Admin User
    │
    ├─ Request to /Permissions
    │
    ├─ Include JWT Token (Bearer)
    │
    ├─ API Server Receives Request
    │
    ├─ Extract Token
    │
    ├─ Validate Token
    │  ├─ Signature valid?
    │  ├─ Not expired?
    │  └─ Issued by trusted authority?
    │
    ├─ Check Claims
    │  └─ Has "Admin" role?
    │
    ├─ If authorized:
    │  └─ Execute endpoint (200 OK)
    │
    └─ If not authorized:
       └─ Return 403 Forbidden
```

## Component Lifecycle

```
RoleManagementPg.razor
│
├─ OnInitializedAsync()
│  ├─ LoadPermissionsAsync()
│  │  └─ GET /Permissions
│  │     └─ Populate: permissions list
│  │
│  └─ LoadRolesAsync()
│     └─ GET /Permissions/roles/all
│        └─ Populate: roles dropdown
│
├─ User Creates Permission
│  ├─ User Input: name, desc, category
│  ├─ CreatePermissionAsync()
│  │  └─ POST /Permissions
│  │     ├─ Show Toast
│  │     ├─ Play Voice
│  │     └─ Reload Permissions
│  └─ UI Updates
│
├─ User Selects Role
│  ├─ OnRoleSelectedAsync()
│  ├─ LoadRolePermissionsAsync()
│  │  └─ GET /Permissions/role/{roleId}
│  └─ UI Updates with role permissions
│
└─ User Toggles Permission
   ├─ TogglePermissionAsync()
   │  ├─ POST /Permissions/assign (if adding)
   │  ├─ DELETE /Permissions/remove (if removing)
   │  ├─ Show Toast
   │  └─ Reload Role Permissions
   └─ UI Updates checkbox state
```

---

**System Architecture Version:** 1.0  
**Last Updated:** 2025-03-06  
**Status:** Complete and Functional ✅
