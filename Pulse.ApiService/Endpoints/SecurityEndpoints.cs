using Microsoft.EntityFrameworkCore;
using Pulse.Models.PulseContext;

namespace Pulse.ApiService.Endpoints
{
    internal static class SecurityEndpoints
    {
        internal const string BasePath = "/Security";
        
        public static void MapSecurityEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup(BasePath).WithTags("Security");

            group.MapGet("/EmployeeLogin/email={email}pwd={passwordhash}", async (string email, string passwordhash, PulseDbContext db) =>
            {
                var user = await db.UserMaster.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordhash && u.IsActive);

                if (user != null)
                {
                    return Results.Ok(user);
                }
                else
                {
                    return Results.Unauthorized();
                }
            });
        }
    }
}
