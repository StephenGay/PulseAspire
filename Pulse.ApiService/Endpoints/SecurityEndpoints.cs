using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pulse.ApiService.Security;
using Pulse.Models.Api;
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

        public static void MapSecurityEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/Security")
                .WithName("Security");
                //.WithOpenApi();

            group.MapPost("/login", async (LoginModel model, UserManager<ApplicationUser> userManager, TokenService tokenService, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("Login");

                if (string.IsNullOrEmpty(model?.Email) || string.IsNullOrEmpty(model?.Password))
                {
                    logger.LogWarning("Login attempt with missing email or password");
                    return Results.BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Email and password are required",
                        StatusCode = 400
                    });
                }

                try
                {
                    var user = await userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        logger.LogWarning("Login attempt with non-existent email: {Email}", model.Email);
                        return Results.Unauthorized();
                    }

                    var passwordValid = await userManager.CheckPasswordAsync(user, model.Password);
                    if (!passwordValid)
                    {
                        logger.LogWarning("Login attempt with invalid password for user: {Email}", model.Email);
                        return Results.Unauthorized();
                    }

                    // Fetch user roles
                    var roles = await userManager.GetRolesAsync(user);
                    logger.LogInformation("User {Email} logged in with roles: {Roles}", model.Email, string.Join(", ", roles));

                    // Generate JWT token with roles
                    var token = await tokenService.GenerateJwtToken(user, roles);

                    return Results.Ok(new ApiResponse<AuthenticationToken>
                    {
                        Success = true,
                        Data = new AuthenticationToken
                        {
                            Token = token,
                            UserId = user.Id,
                            Username = user.UserName ?? user.Email,
                            Email = user.Email,
                            TokenType = "Bearer",
                            ExpiresIn = 86400  // Changed from 3600 (1 hour) to 86400 (24 hours)
                        },
                        Message = "Login successful",
                        StatusCode = 200
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error during login for {Email}", model.Email);
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("Login")
            //.WithOpenApi()
            .Produces<ApiResponse<AuthenticationToken>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);

            group.MapGet("/EmployeeLogin", async (string email, UserManager<ApplicationUser> userManager, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("EmployeeLogin");

                if (string.IsNullOrEmpty(email))
                {
                    return Results.BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Email is required",
                        StatusCode = 400
                    });
                }

                try
                {
                    var user = await userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        logger.LogWarning("Employee lookup failed for email: {Email}", email);
                        return Results.NotFound(new ApiResponse
                        {
                            Success = false,
                            Message = "User not found",
                            StatusCode = 404
                        });
                    }

                    return Results.Ok(new ApiResponse<ApplicationUser>
                    {
                        Success = true,
                        Data = user,
                        Message = "Employee retrieved successfully",
                        StatusCode = 200
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retrieving employee for email: {Email}", email);
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetEmployeeByEmail")
            //.WithOpenApi()
            //.RequireAuthorization()
            .Produces<ApiResponse<ApplicationUser>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

            group.MapPost("/register", async (UserManager<ApplicationUser> userManager, [FromBody] RegisterModel model, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("Register");

                if (userManager == null)
                {
                    logger.LogError("UserManager service is not available");
                    return Results.Problem("UserManager service is not available.");
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(model?.Email) || string.IsNullOrWhiteSpace(model?.Password))
                {
                    logger.LogWarning("Registration attempt with missing email or password");
                    return Results.BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Email and password are required",
                        StatusCode = 400
                    });
                }

                // Check if user already exists
                var existingUser = await userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    logger.LogWarning("Registration attempt with existing email: {Email}", model.Email);
                    return Results.BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "User with this email already exists",
                        StatusCode = 400
                    });
                }

                try
                {
                    // Create the new user
                    var user = new ApplicationUser
                    {
                        UserName = model.UserName ?? model.Email,
                        Email = model.Email,
                        FullName = model.FullName ?? model.Email
                    };

                    var result = await userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        logger.LogInformation("User registered successfully: {Email}", model.Email);
                        return Results.Ok(new ApiResponse<AuthenticationToken>
                        {
                            Success = true,
                            Data = new AuthenticationToken
                            {
                                UserId = user.Id,
                                Email = user.Email,
                                Username = user.UserName ?? user.Email,
                                TokenType = "Bearer",
                                Message = "User created successfully"
                            },
                            Message = "User registered successfully",
                            StatusCode = 200
                        });
                    }

                    logger.LogWarning("Failed to create user {Email}: {Errors}", model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return Results.BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to create user",
                        Errors = result.Errors.ToDictionary(e => "user", e => new[] { e.Description }),
                        StatusCode = 400
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error during registration for {Email}", model.Email);
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }
            })
            .AllowAnonymous()
            .WithName("Register")
            .Produces<ApiResponse<AuthenticationToken>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

            group.MapGet("/OldEmployeeLogin/email={email}", async (string email, PulseDbContext db) =>
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

            group.MapGet("/roles", async (RoleManager<IdentityRole> roleManager, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("GetRoles");

                if (roleManager == null)
                {
                    logger.LogError("RoleManager service is not available");
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }

                try
                {
                    var roles = await roleManager.Roles
                        .Select(r => r.Name)
                        .ToListAsync();

                    return Results.Ok(new ApiResponse<List<string>>
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
            })
            .WithName("GetRoles")
            .Produces<ApiResponse<List<string>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

            // CREATE ROLE ENDPOINT
            group.MapPost("/roles", async (RoleManager<IdentityRole> roleManager, [FromBody] string roleName, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("CreateRole");

                if (roleManager == null)
                {
                    logger.LogError("RoleManager service is not available");
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }

                if (string.IsNullOrWhiteSpace(roleName))
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
                    // Check if role already exists
                    var existingRole = await roleManager.FindByNameAsync(roleName);
                    if (existingRole != null)
                    {
                        return Results.BadRequest(new ApiResponse
                        {
                            Success = false,
                            Message = $"Role '{roleName}' already exists",
                            StatusCode = 400
                        });
                    }

                    var role = new IdentityRole(roleName);
                    var result = await roleManager.CreateAsync(role);

                    if (result.Succeeded)
                    {
                        logger.LogInformation("Role created: {RoleName}", roleName);
                        return Results.Ok(new ApiResponse<string>
                        {
                            Success = true,
                            Data = roleName,
                            Message = $"Role '{roleName}' created successfully",
                            StatusCode = 200
                        });
                    }

                    logger.LogWarning("Failed to create role {RoleName}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return Results.BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to create role",
                        Errors = result.Errors.ToDictionary(e => "role", e => new[] { e.Description }),
                        StatusCode = 400
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error creating role {RoleName}", roleName);
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }
            })
            //.RequireAuthorization(AdminRole)
            .WithName("CreateRole")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

            // GET USERS ENDPOINT
            group.MapGet("/users", async (UserManager<ApplicationUser> userManager, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("GetUsers");

                if (userManager == null)
                {
                    logger.LogError("UserManager service is not available");
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }

                try
                {
                    var users = await userManager.Users.ToListAsync();
                    var userDtos = new List<UserRoleDto>();

                    foreach (var user in users)
                    {
                        var roles = await userManager.GetRolesAsync(user);
                        userDtos.Add(new UserRoleDto
                        {
                            Id = user.Id,
                            Email = user.Email ?? string.Empty,
                            UserName = user.UserName ?? string.Empty,
                            Roles = roles.ToList()
                        });
                    }

                    return Results.Ok(new ApiResponse<List<UserRoleDto>>
                    {
                        Success = true,
                        Data = userDtos,
                        Message = $"Retrieved {userDtos.Count} users",
                        StatusCode = 200
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retrieving users");
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }
            })
            //.RequireAuthorization(AdminRole)
            .WithName("GetUsers")
            .Produces<ApiResponse<List<UserRoleDto>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

            // ASSIGN ROLE ENDPOINT
            group.MapPost("/assign-role", async (UserManager<ApplicationUser> userManager, [FromBody] AssignRoleModel model, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("AssignRole");

                if (userManager == null)
                {
                    logger.LogError("UserManager service is not available");
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }

                if (string.IsNullOrWhiteSpace(model?.UserId) || string.IsNullOrWhiteSpace(model?.RoleName))
                {
                    return Results.BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "UserId and RoleName are required",
                        StatusCode = 400
                    });
                }

                try
                {
                    var user = await userManager.FindByIdAsync(model.UserId);
                    if (user == null)
                    {
                        logger.LogWarning("User not found for assignment: {UserId}", model.UserId);
                        return Results.NotFound(new ApiResponse
                        {
                            Success = false,
                            Message = "User not found",
                            StatusCode = 404
                        });
                    }

                    // Check if user already has the role
                    var hasRole = await userManager.IsInRoleAsync(user, model.RoleName);
                    if (hasRole)
                    {
                        return Results.BadRequest(new ApiResponse
                        {
                            Success = false,
                            Message = $"User already has role '{model.RoleName}'",
                            StatusCode = 400
                        });
                    }

                    var result = await userManager.AddToRoleAsync(user, model.RoleName);

                    if (result.Succeeded)
                    {
                        logger.LogInformation("Role assigned - UserId: {UserId}, Role: {RoleName}", model.UserId, model.RoleName);
                        return Results.Ok(new ApiResponse<string>
                        {
                            Success = true,
                            Data = model.RoleName,
                            Message = $"Role '{model.RoleName}' assigned successfully",
                            StatusCode = 200
                        });
                    }

                    logger.LogWarning("Failed to assign role {RoleName} to user {UserId}: {Errors}", model.RoleName, model.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return Results.BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to assign role",
                        Errors = result.Errors.ToDictionary(e => "role", e => new[] { e.Description }),
                        StatusCode = 400
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", model.RoleName, model.UserId);
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }
            })
            //.RequireAuthorization(AdminRole)
            .WithName("AssignRole")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

            // DIAGNOSTIC: Check current user roles
            group.MapGet("/debug/my-roles", async (HttpContext httpContext, UserManager<ApplicationUser> userManager) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                    return Results.Unauthorized();

                var roles = await userManager.GetRolesAsync(user);
                return Results.Ok(new
                {
                    userId = user.Id,
                    email = user.Email,
                    roles = roles,
                    claims = httpContext.User.Claims.Select(c => new { c.Type, c.Value })
                });
            })
            .WithName("DebugMyRoles");

            // TEMPORARY: Assign Admin role (remove after bootstrapping)
            group.MapGet("/debug/assign-admin/{email}", async (string email, UserManager<ApplicationUser> userManager, ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("AssignAdmin");
                
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return Results.NotFound($"User '{email}' not found");
                }

                var hasRole = await userManager.IsInRoleAsync(user, "Admin");
                if (hasRole)
                {
                    return Results.Ok($"User already has Admin role");
                }

                var result = await userManager.AddToRoleAsync(user, "Admin");
                if (result.Succeeded)
                {
                    logger.LogInformation("Admin role assigned to {Email}", email);
                    return Results.Ok($"Admin role assigned to {email}");
                }

                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                logger.LogError("Failed to assign Admin to {Email}: {Errors}", email, errors);
                return Results.BadRequest(errors);
            })
            .WithName("AssignAdminDebug");
        }
    }
}

