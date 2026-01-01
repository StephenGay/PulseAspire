using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pulse.ApiService.Security;
using Pulse.Models.PulseContext;
using Pulse.Models.Users;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Pulse.ApiService.Endpoints
{
    internal static class SecurityEndpoints
    {
        internal const string BasePath = "/Security";
        private const string AdminRole = "Admin";

        public static void MapSecurityEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup(BasePath).WithTags("Security");

            group.MapGet("/EmployeeLogin/email={email}", async (string email, PulseDbContext db) =>
            {
                try
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
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            });

            group.MapPost("/login", async (SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, TokenService tokenService, [FromBody] LoginModel model) =>
            {
                if (signInManager == null || userManager == null || tokenService == null)
                {
                    return Results.Problem("Required services are not available.");
                }

                var validationContext = new ValidationContext(model);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
                {
                    return Results.BadRequest(validationResults.Select(vr => new { vr.ErrorMessage }));
                }

                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    var user = await userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        return Results.Unauthorized();
                    }

                    var token = await tokenService.GenerateJwtToken(user);
                    return Results.Ok(new { Token = token});
                }
                return Results.Unauthorized();
            }).AllowAnonymous();

            group.MapPost("/register", async (UserManager<IdentityUser> userManager, [FromBody] RegisterModel model) =>
            {
                if (userManager == null)
                {
                    return Results.Problem("UserManager service is not available.");
                }

                var validationContext = new ValidationContext(model);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
                {
                    return Results.BadRequest(validationResults.Select(vr => new { vr.ErrorMessage }));
                }

                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded) return Results.Ok("User created");
                return Results.BadRequest(result.Errors);
            }).AllowAnonymous();

            group.MapPost("/create-role", async (RoleManager<IdentityRole> roleManager, [FromBody] string roleName) =>
            {
                if (roleManager == null)
                {
                    return Results.Problem("RoleManager service is not available.");
                }

                //var validationContext = new ValidationContext(model);
                //var validationResults = new List<ValidationResult>();
                //if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
                //{
                //    return Results.BadRequest(validationResults.Select(vr => new { vr.ErrorMessage }));
                //}

                if (string.IsNullOrEmpty(roleName)) return Results.BadRequest("Role name required");
                var role = new IdentityRole(roleName);
                var result = await roleManager.CreateAsync(role);
                return result.Succeeded ? Results.Ok($"Role {roleName} created") : Results.BadRequest(result.Errors);
                //if (result.Succeeded)
                //{
                //    return Results.Created($"/roles/{role.Name}", role);
                //}
                //return Results.BadRequest(result.Errors);
            }).RequireAuthorization(new AuthorizeAttribute { Roles = AdminRole }).WithName("CreateRole");

            group.MapGet("/roles", async (RoleManager<IdentityRole> roleManager) =>
            {
                if (roleManager == null)
                {
                    return Results.Problem("RoleManager service is not available.");
                }

                var roles = await roleManager.Roles.ToListAsync();
                return Results.Ok(roles.Select(r => r.Name));
            }).WithName("GetRoles");

            group.MapGet("/users", async (UserManager<IdentityUser> userManager) =>
            {
                if (userManager == null)
                {
                    return Results.Problem("UserManager service is not available.");
                }

                var users = await userManager.Users.ToListAsync();
                var userDtos = new List<object>();
                foreach (var u in users)
                {
                    var roles = await userManager.GetRolesAsync(u);
                    userDtos.Add(new { u.Id, u.Email, Roles = roles });
                }
                return Results.Ok(userDtos);
            }); //.RequireAuthorization(new AuthorizeAttribute { Roles = AdminRole }).WithName("GetUsers");

            group.MapPost("/assign-role", async (UserManager<IdentityUser> userManager, [FromBody] AssignRoleModel model) =>
             {
                 var user = await userManager.FindByIdAsync(model.UserId);
                 if (user == null) return Results.NotFound("User not found");

                 var result = await userManager.AddToRoleAsync(user, model.RoleName);
                 return result.Succeeded ? Results.Ok($"Assigned {model.RoleName} to user") : Results.BadRequest(result.Errors);
             }).RequireAuthorization(new AuthorizeAttribute { Roles = AdminRole });


            //group.MapPost("/users/{userId}/roles", async (UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, [FromBody] AssignRoleModel model, string userId) =>
            //{
            //    if (userManager == null || roleManager == null)
            //    {
            //        return Results.Problem("Required services are not available.");
            //    }

            //    var validationContext = new ValidationContext(model);
            //    var validationResults = new List<ValidationResult>();
            //    if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
            //    {
            //        return Results.BadRequest(validationResults.Select(vr => new { vr.ErrorMessage }));
            //    }

            //    var user = await userManager.FindByIdAsync(userId);
            //    if (user == null)
            //    {
            //        return Results.NotFound("User not found");
            //    }

            //    if (!await roleManager.RoleExistsAsync(model.RoleName))
            //    {
            //        return Results.BadRequest(new { Errors = new[] { $"Role '{model.RoleName}' does not exist." } });
            //    }

            //    if (await userManager.IsInRoleAsync(user, model.RoleName))
            //    {
            //        return Results.BadRequest(new { Errors = new[] { $"User already has the role '{model.RoleName}'." } });
            //    }

            //    var result = await userManager.AddToRoleAsync(user, model.RoleName);
            //    if (result.Succeeded)
            //    {
            //        return Results.Ok();
            //    }
            //    return Results.BadRequest(result.Errors);
            //}).RequireAuthorization(new AuthorizeAttribute { Roles = AdminRole }).WithName("AssignRole");

            group.MapDelete("/users/{userId}/roles/{roleName}", async (UserManager<IdentityUser> userManager, string userId, string roleName) =>
            {
                if (userManager == null)
                {
                    return Results.Problem("UserManager service is not available.");
                }

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
            }).RequireAuthorization(new AuthorizeAttribute { Roles = AdminRole }).WithName("RemoveRole");

            group.MapGet("/users/{userId}/roles", async (UserManager<IdentityUser> userManager, string userId) =>
            {
                if (userManager == null)
                {
                    return Results.Problem("UserManager service is not available.");
                }

                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Results.NotFound("User not found");
                }

                var roles = await userManager.GetRolesAsync(user);
                return Results.Ok(roles);
            }); //.RequireAuthorization(new AuthorizeAttribute { Roles = AdminRole }).WithName("GetUserRoles");
        }
    }
}

