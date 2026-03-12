using Microsoft.AspNetCore.Identity;
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
    }
}
