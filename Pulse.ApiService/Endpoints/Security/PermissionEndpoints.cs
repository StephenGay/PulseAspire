using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse.Models.Api;
using Pulse.Models.Permissions;
using Pulse.Models.PulseContext;

namespace Pulse.ApiService.Endpoints.Security
{
    internal static class PermissionEndpoints
    {
        internal const string BasePath = "/Security/Permissions";

        public static void MapPermissionEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/Security/Permissions")
                .WithName("Permissions")
                .RequireAuthorization("Admin");

            // Get all permissions
            group.MapGet("/GetAll", GetAllPermissions)
                .WithName("GetPermissions")
                .Produces<ApiResponse<List<PermissionDto>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            // Create new permission
            group.MapPost("/Create", CreatePermission)
                .WithName("CreatePermission")
                .Produces<ApiResponse<PermissionDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapPost("/Update", UpdatePermission)
                .WithName("UpdatePermission")
                .Produces<ApiResponse<PermissionDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);

            // Get permissions by category
            group.MapGet("/GetByCategory/{category}", GetPermissionsByCategory)
                .WithName("GetPermissionsByCategory")
                .Produces<ApiResponse<List<PermissionDto>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapGet("/Categories/GetAll", GetAllCategories)
                .WithName("GetAllCategories")
                .Produces<ApiResponse<List<PermissionCategory>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapPost("/Categories/Create", CreatePermissionCategory)
                .WithName("CreatePermissionCategory")
                .Produces<ApiResponse<PermissionCategory>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);

            // Get role permissions
            group.MapGet("/GetByRole/{roleId}", GetRolePermissions)
                .WithName("GetRolePermissions")
                .Produces<ApiResponse<RolePermissionDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            // Assign permission to role
            group.MapPost("/AssignToRole", AssignPermissionToRole)
                .WithName("AssignPermissionToRole")
                .Produces<ApiResponse>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            // Remove permission from role
            group.MapDelete("/RemoveFromRole/{roleId}/{permissionId}", RemovePermissionFromRole)
                .WithName("RemovePermissionFromRole")
                .Produces<ApiResponse>(StatusCodes.Status200OK)
                .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapGet("/GetAssignedRoles/{permissionId}", GetAssignedRoles)
                .WithName("GetAssignedRoles")
                .Produces<ApiResponse<List<RolePermission>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            // Get all roles with IDs
            group.MapGet("/roles/all", GetAllRoles)
                .WithName("GetAllRolesWithIds")
                .Produces<ApiResponse<List<RoleWithIdDto>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);
        }

        private static async Task<IResult> GetAllPermissions(
            PulseDbContext db,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GetPermissions");

            try
            {
                var permissions = await db.AspNetPermissions
                    .AsNoTracking()
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.Name)
                    .Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Category = p.Category
                    })
                    .ToListAsync();

                return Results.Ok(new ApiResponse<List<PermissionDto>>
                {
                    Success = true,
                    Data = permissions,
                    Message = $"Retrieved {permissions.Count} permissions",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving permissions");
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task<IResult> GetPermissionsByCategory(
            string category,
            PulseDbContext db,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GetPermissionsByCategory");

            try
            {
                var permissions = await db.AspNetPermissions
                    .AsNoTracking()
                    .Where(p => p.Category == category)
                    .OrderBy(p => p.Name)
                    .Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Category = p.Category
                    })
                    .ToListAsync();

                return Results.Ok(new ApiResponse<List<PermissionDto>>
                {
                    Success = true,
                    Data = permissions,
                    Message = $"Retrieved {permissions.Count} permissions for category '{category}'",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving permissions for category: {Category}", category);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task<IResult> CreatePermissionCategory(
            [FromBody] PermissionCategory model,
            PulseDbContext db,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("CreatePermissionCategory");

            if (string.IsNullOrWhiteSpace(model?.Category))
            {
                return Results.BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Permission category is required",
                    StatusCode = 400
                });
            }

            try
            {
                // Check if permission category already exists
                var existing = await db.AspNetPermissionCategories
                    .FirstOrDefaultAsync(p => p.Category == model.Category);

                if (existing != null)
                {
                    return Results.BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = $"Permission category '{model.Category}' already exists",
                        StatusCode = 400
                    });
                }

                db.AspNetPermissionCategories.Add(new PermissionCategory
                {
                    Category = model.Category
                });
                await db.SaveChangesAsync();

                logger.LogInformation("Permission category created: {PermissionCategory}", model.Category);

                return Results.Ok(new ApiResponse<PermissionCategory>
                {
                    Success = true,
                    Data = new PermissionCategory
                    {
                        Category = model.Category
                    },
                    Message = "Permission category created successfully",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating permission category: {PermissionCategory}", model.Category);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        
        private static async Task<IResult> CreatePermission(
            [FromBody] CreatePermissionModel model,
            PulseDbContext db,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("CreatePermission");

            if (string.IsNullOrWhiteSpace(model?.Name))
            {
                return Results.BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Permission name is required",
                    StatusCode = 400
                });
            }

            try
            {
                // Check if permission already exists
                var existing = await db.AspNetPermissions
                    .FirstOrDefaultAsync(p => p.Name == model.Name);

                if (existing != null)
                {
                    return Results.BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = $"Permission '{model.Name}' already exists",
                        StatusCode = 400
                    });
                }

                var permission = new Permission
                {
                    Name = model.Name,
                    Description = model.Description ?? string.Empty,
                    Category = model.Category ?? "General",
                    CreatedDate = DateTime.UtcNow
                };

                db.AspNetPermissions.Add(permission);
                await db.SaveChangesAsync();

                logger.LogInformation("Permission created: {PermissionName}", model.Name);

                return Results.Ok(new ApiResponse<PermissionDto>
                {
                    Success = true,
                    Data = new PermissionDto
                    {
                        Id = permission.Id,
                        Name = permission.Name,
                        Description = permission.Description,
                        Category = permission.Category
                    },
                    Message = "Permission created successfully",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating permission: {PermissionName}", model.Name);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task<IResult> UpdatePermission(
            [FromBody] PermissionDto model,
            PulseDbContext db,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("UpdatePermission");
            if (string.IsNullOrWhiteSpace(model?.Name))
            {
                return Results.BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Permission name is required",
                    StatusCode = 400
                });
            }

            try
            {
                var permission = db.AspNetPermissions.Find(model.Id);
                if (permission == null)
                {
                    return Results.NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Permission not found",
                        StatusCode = 404
                    });
                }

                permission.Name = model.Name;
                permission.Description = model.Description ?? string.Empty;
                permission.Category = model.Category ?? "General";
                permission.CreatedDate = DateTime.UtcNow;

                db.AspNetPermissions.Update(permission);
                await db.SaveChangesAsync();

                logger.LogInformation("Permission updated: {PermissionName}", model.Name);

                return Results.Ok(new ApiResponse<PermissionDto>
                {
                    Success = true,
                    Data = new PermissionDto
                    {
                        Id = permission.Id,
                        Name = permission.Name,
                        Description = permission.Description,
                        Category = permission.Category
                    },
                    Message = "Permission created successfully",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating permission: {PermissionName}", model.Name);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        private static async Task<IResult> GetRolePermissions(
            string roleId,
            RoleManager<IdentityRole> roleManager,
            PulseDbContext db,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GetRolePermissions");

            try
            {
                var role = await roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return Results.NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Role not found",
                        StatusCode = 404
                    });
                }

                var permissions = await db.AspNetRolePermissions
                    .AsNoTracking()
                    .Where(rp => rp.RoleId == roleId)
                    .Include(rp => rp.Permission)
                    .OrderBy(rp => rp.Permission.Category)
                    .ThenBy(rp => rp.Permission.Name)
                    .Select(rp => new PermissionDto
                    {
                        Id = rp.Permission.Id,
                        Name = rp.Permission.Name,
                        Description = rp.Permission.Description,
                        Category = rp.Permission.Category
                    })
                    .ToListAsync();

                return Results.Ok(new ApiResponse<RolePermissionDto>
                {
                    Success = true,
                    Data = new RolePermissionDto
                    {
                        RoleId = role.Id,
                        RoleName = role.Name ?? string.Empty,
                        Permissions = permissions
                    },
                    Message = $"Retrieved {permissions.Count} permissions for role '{role.Name}'",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving role permissions for roleId: {RoleId}", roleId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task<IResult> GetAssignedRoles(
            string permissionId,
            RoleManager<IdentityRole> roleManager,
            PulseDbContext db,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GetAssignedRoles");

            try
            {
                var roles = await db.AspNetRolePermissions
                    .AsNoTracking()
                    .Where(rp => rp.PermissionId.ToString() == permissionId)
                    .ToListAsync();
   
                if (roles == null || roles.Count == 0)
                {
                    roles = new List<RolePermission>();
                }

                return Results.Ok(new ApiResponse<List<RolePermission>>
                {
                    Success = true,
                    Data = roles,
                    Message = $"Retrieved {roles.Count} roles for permission '{permissionId}'",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving roles for permissionId: {PermissionId}", permissionId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        private static async Task<IResult> AssignPermissionToRole(
            [FromBody] AssignPermissionModel model,
            RoleManager<IdentityRole> roleManager,
            PulseDbContext db,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("AssignPermission");

            if (string.IsNullOrWhiteSpace(model?.RoleId) || model.PermissionId <= 0)
            {
                return Results.BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "RoleId and PermissionId are required",
                    StatusCode = 400
                });
            }

            try
            {
                var role = await roleManager.FindByIdAsync(model.RoleId);
                if (role == null)
                {
                    return Results.NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Role not found",
                        StatusCode = 404
                    });
                }

                var permission = await db.AspNetPermissions
                    .FirstOrDefaultAsync(p => p.Id == model.PermissionId);

                if (permission == null)
                {
                    return Results.NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Permission not found",
                        StatusCode = 404
                    });
                }

                // Check if already assigned
                var existing = await db.AspNetRolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == model.RoleId && rp.PermissionId == model.PermissionId);

                if (existing != null)
                {
                    return Results.BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = $"Permission '{permission.Name}' is already assigned to role '{role.Name}'",
                        StatusCode = 400
                    });
                }

                var rolePermission = new RolePermission
                {
                    RoleId = model.RoleId,
                    PermissionId = model.PermissionId,
                    AssignedDate = DateTime.UtcNow
                };

                db.AspNetRolePermissions.Add(rolePermission);
                await db.SaveChangesAsync();

                logger.LogInformation("Permission '{PermissionName}' assigned to role '{RoleName}'",
                    permission.Name, role.Name);

                return Results.Ok(new ApiResponse
                {
                    Success = true,
                    Message = $"Permission '{permission.Name}' assigned to role '{role.Name}' successfully",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error assigning permission to role");
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task<IResult> RemovePermissionFromRole(
            string roleId,
            int permissionId,
            RoleManager<IdentityRole> roleManager,
            PulseDbContext db,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("RemovePermission");

            try
            {
                var role = await roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return Results.NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Role not found",
                        StatusCode = 404
                    });
                }

                var rolePermission = await db.AspNetRolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

                if (rolePermission == null)
                {
                    return Results.NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Role-Permission relationship not found",
                        StatusCode = 404
                    });
                }

                db.AspNetRolePermissions.Remove(rolePermission);
                await db.SaveChangesAsync();

                logger.LogInformation("Permission removed from role '{RoleName}'", role.Name);

                return Results.Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Permission removed from role successfully",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing permission from role");
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task<IResult> GetAllRoles(
            RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GetAllRoles");

            try
            {
                var roles = await roleManager.Roles
                    .Select(r => new RoleWithIdDto
                    {
                        Id = r.Id,
                        Name = r.Name ?? string.Empty
                    })
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                return Results.Ok(new ApiResponse<List<RoleWithIdDto>>
                {
                    Success = true,
                    Data = roles,
                    Message = $"Retrieved {roles.Count} roles",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving roles");
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task<IResult> GetAllCategories(
            PulseDbContext db,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GetAllCategories");
            try
            {
                var cats = await db.AspNetPermissionCategories
                    .OrderBy(r => r.Category)
                    .ToListAsync();

                return Results.Ok(new ApiResponse<List<PermissionCategory>>
                {
                    Success = true,
                    Data = cats,
                    Message = $"Retrieved {cats.Count} categories",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving Permission Categories");
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
