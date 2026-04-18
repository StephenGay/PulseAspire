using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse.Models.Api;
using Pulse.Models.Permissions;

namespace Pulse.ApiService.Endpoints.Security
{
    public static class RoleEndpoints
    {
        internal const string BasePath = "/Security/Roles";

        public static void MapRoleEndpoints(this WebApplication app)
        {
            var group = app.MapGroup(BasePath)
                .WithName("Roles")
                .RequireAuthorization("Admin");

            group.MapGet("/GetAll", GetAllRoles)
                .WithName("GetAllRoleDto")
                .Produces<ApiResponse<List<RoleWithIdDto>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapPost("/Add", AddNewRole)
            //.RequireAuthorization(AdminRole)
            .WithName("AddRole")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
        }

        private static async Task<IResult> GetAllRoles(
            RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GetAllRoleDto");

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
        private static async Task<IResult> AddNewRole(
            RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory,
            [FromBody] string newRole)
        {
            var logger = loggerFactory.CreateLogger("AddRole");
            if (string.IsNullOrWhiteSpace(newRole))
            {
                return Results.BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Role name cannot be empty",
                    StatusCode = 400
                });
            }
            try
            {
                var existingRole = await roleManager.FindByNameAsync(newRole);
                if (existingRole != null)
                {
                    return Results.BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "A role with this name already exists",
                        StatusCode = 400
                    });
                }
                var identityRole = new IdentityRole { Name = newRole };
                var result = await roleManager.CreateAsync(identityRole);
                if (result.Succeeded)
                {
                    return Results.Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Data = identityRole.Id,
                        Message = "Role created successfully",
                        StatusCode = 200
                    });
                }
                else
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to create role: {Errors}", errors);
                    return Results.BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = $"Failed to create role: {errors}",
                        StatusCode = 400
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating new role");
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
