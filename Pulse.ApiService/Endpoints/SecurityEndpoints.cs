using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse.ApiService.Security;
using System.ComponentModel.DataAnnotations;  // For Validator and annotations
using System.Collections.Generic;
using Pulse.Models.PulseContext;
using Pulse.Models.Users;

namespace Pulse.ApiService.Endpoints
{
    internal static class SecurityEndpoints
    {
        internal const string BasePath = "/Security";
        
        
        public static void MapSecurityEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup(BasePath).WithTags("Security");

            group.MapGet("/EmployeeLogin/email={email}", async (string email, PulseDbContext db) =>
            {
                var user = await db.UserMaster.AsNoTracking()
                    .Include(u => u.UserSettings)
                    .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

                if (user != null)
                {
                    return Results.Ok(user);
                }
                else
                {
                    return Results.Unauthorized();
                }
            });

            group.MapPost("/login", async (SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, TokenService tokenService, [FromBody] LoginModel model) =>
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    var user = await userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        return Results.Unauthorized();
                    }

                    var token = await tokenService.GenerateJwtToken(user);
                    return Results.Ok(new { Token = token });
                }
                return Results.Unauthorized();
            }).AllowAnonymous();

            group.MapPost("/register", async (UserManager<ApplicationUser> userManager, TokenService tokenService, [FromBody] RegisterModel model) =>
            {
                var validationContext = new ValidationContext(model);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
                {
                    return Results.BadRequest(validationResults.Select(vr => new { vr.ErrorMessage }));
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,  // Identity uses UserName for login; set to Email for email-based auth
                    Email = model.Email,
                    FullName = model.FullName ?? string.Empty
                };

                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                    // Optional: Auto-login and return JWT
                    var token = await tokenService.GenerateJwtToken(user);
                    return Results.Ok(new { Token = token });

                    // Alternative: Just return success without token
                    // return Results.Created($"/users/{user.Id}", new { Message = "User registered successfully" });
                }

                // Handle errors (e.g., duplicate email, weak password)
                return Results.BadRequest(result.Errors);
            }).AllowAnonymous();

            // Role management endpoints (secured for Admin)
            //group.MapGroup("/roles")
            //    .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });  // Group-level auth

            group.MapPost("/roles", async (RoleManager<IdentityRole> roleManager, [FromBody] CreateRoleModel model) =>
            {
                var validationContext = new ValidationContext(model);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
                {
                    return Results.BadRequest(validationResults.Select(vr => new { vr.ErrorMessage }));
                }

                var role = new IdentityRole(model.Name);
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return Results.Created($"/roles/{role.Name}", role);
                }
                return Results.BadRequest(result.Errors);
            }).WithName("CreateRole");

            group.MapGet("/roles", async (RoleManager<IdentityRole> roleManager) =>
            {
                var roles = await roleManager.Roles.ToListAsync();
                return Results.Ok(roles.Select(r => r.Name));
            }).WithName("GetRoles");

            group.MapGet("/users", async (UserManager<ApplicationUser> userManager) =>
            {
                var users = await userManager.Users.ToListAsync();
                var userDtos = new List<object>();
                foreach (var u in users)
                {
                    var roles = await userManager.GetRolesAsync(u);
                    userDtos.Add(new { u.Id, u.Email, Roles = roles });
                }
                return Results.Ok(userDtos);
            }).WithName("GetUsers");
            //}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

            // User-role management endpoints (secured for Admin)
            group.MapGroup("/users/{userId}/roles")
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

            // group.MapPost("/users/{userId}/roles", async (UserManager<ApplicationUser> userManager, [FromBody] AssignRoleModel model, string userId) =>
            group.MapPost("/users/{userId}/roles", async (UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, [FromBody] AssignRoleModel model, string userId) =>
            {
                var validationContext = new ValidationContext(model);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
                {
                    return Results.BadRequest(validationResults.Select(vr => new { vr.ErrorMessage }));
                }

                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Results.NotFound("User not found");
                }

                // Validate role exists
                if (!await roleManager.RoleExistsAsync(model.RoleName))
                {
                    return Results.BadRequest(new { Errors = new[] { $"Role '{model.RoleName}' does not exist." } });
                }

                // Check if user already has the role (optional, to avoid duplicates)
                if (await userManager.IsInRoleAsync(user, model.RoleName))
                {
                    return Results.BadRequest(new { Errors = new[] { $"User already has the role '{model.RoleName}'." } });
                }

                var result = await userManager.AddToRoleAsync(user, model.RoleName);
                if (result.Succeeded)
                {
                    return Results.Ok();
                }
                return Results.BadRequest(result.Errors);
            }).WithName("AssignRole");
  //.RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

            group.MapDelete("/users/{userId}/roles/{roleName}", async (UserManager<ApplicationUser> userManager, string userId, string roleName) =>
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Results.NotFound("User not found");
                }

                var result = await userManager.RemoveFromRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    return Results.NoContent();
                }
                return Results.BadRequest(result.Errors);
            }).WithName("RemoveRole");

            group.MapGet("/users/{userId}/roles", async (UserManager<ApplicationUser> userManager, string userId) =>
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Results.NotFound("User not found");
                }

                var roles = await userManager.GetRolesAsync(user);
                return Results.Ok(roles);
            }).WithName("GetUserRoles");

            
        }
    }
}