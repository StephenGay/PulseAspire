using Microsoft.EntityFrameworkCore;
using Pulse.Models.CustomComponents;
using Pulse.Models.Misc;
using Pulse.Models.PulseContext;
using Pulse.Models.Users;

namespace Pulse.ApiService.Endpoints
{
    internal static class UserEndpoints
    {

        internal const string BasePath = "/User";

        public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup(BasePath).WithTags("User Endpoints");

            group.MapGet("/Favourites/SavedQueries/GetById/{UserId}", async (int UserId, PulseDbContext dbContext) =>
            {

                var favQrys = await dbContext.UserFavouriteQueries
                    .AsNoTracking()
                    .Where(a => a.UserId == UserId)
                    .ToListAsync();
                if (favQrys == null) { favQrys = new List<UserFavouriteQry>(); }
                return Results.Ok(favQrys);
            });

            group.MapPost("/Favourites/SavedQueries/Add/", async (UserFavouriteQry qry, PulseDbContext dbContext) =>
            {
                dbContext.UserFavouriteQueries.Add(qry);
                await dbContext.SaveChangesAsync();
                return Results.Created($"/User/Favourites/SavedQueries/Add/{qry.Id}", qry);
            });

            group.MapDelete("/Favourites/SavedQueries/Delete/{id}", async (int id, PulseDbContext dbContext) =>
            {
                var item = await dbContext.UserFavouriteQueries.FindAsync(id);
                if (item == null)
                {
                    return Results.NotFound($"Item with ID {id} not found.");
                }
                dbContext.UserFavouriteQueries.Remove(item);
                await dbContext.SaveChangesAsync();
                return Results.Ok($"Item with ID {id} has been deleted.");
            });
        }
    }
}
