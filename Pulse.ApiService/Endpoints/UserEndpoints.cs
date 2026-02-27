using Microsoft.EntityFrameworkCore;
using Pulse.Models.CustomComponents;
using Pulse.Models.Misc;
using Pulse.Models.Production;
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

            

            group.MapGet("/Favourites/Queries/GetByUserId/{UserId}", async (string UserId, PulseDbContext dbContext) =>
            {

                var favQrys = await dbContext.UserFaveQueries
                    .AsNoTracking()
                    .Where(a => a.UserId == UserId)
                    .ToListAsync();
                if (favQrys == null) { favQrys = new List<UserFavouriteQuery>(); }
                return Results.Ok(favQrys);
            });

            

            group.MapPost("/Favourites/Queries/Add/",async (UserFavouriteQuery qry, PulseDbContext dbContext) =>
            {
                dbContext.UserFaveQueries.Add(qry);
                await dbContext.SaveChangesAsync();
                return Results.Created($"/User/Favourites/Queries/Add/{qry.Id}", qry);
            });

            group.MapDelete("/Favourites/Queries/Delete/{id}", async (int id, PulseDbContext dbContext) =>
            {
                var item = await dbContext.UserFaveQueries.FindAsync(id);
                if (item == null)
                {
                    return Results.NotFound($"Item with ID {id} not found.");
                }
                dbContext.UserFaveQueries.Remove(item);
                await dbContext.SaveChangesAsync();
                return Results.Ok($"Item with ID {id} has been deleted.");
            });

            

            group.MapGet("/Settings/GetSettings/{UserId}", async (string UserId, PulseDbContext db) =>
            {
                var settings = await db.AspNetUserSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == UserId);
                if (settings == null)
                {
                    settings = new ApplicationUserSettings
                    {
                        UserId = UserId,
                        PreferredUserName = "User",
                        AIHasVoice = false,
                        AIVoiceID = null,
                        UserTheme = "pulse"
                    };
                }
                return Results.Ok(settings);
            })
                .WithName("GetUserSettings");

            group.MapPut("/Settings/Update", async (ApplicationUserSettings updatedUserSettings, PulseDbContext db) =>
            {
                var item = await db.AspNetUserSettings
                    .FirstOrDefaultAsync(p => p.UserId == updatedUserSettings.UserId);
                if (item == null)
                {
                    item = new ApplicationUserSettings
                    {
                        UserId = updatedUserSettings.UserId,
                        PreferredUserName = updatedUserSettings.PreferredUserName,
                        AIHasVoice = updatedUserSettings.AIHasVoice,
                        AIVoiceID = updatedUserSettings.AIVoiceID,
                        UserTheme = updatedUserSettings.UserTheme
                    };
                    db.AspNetUserSettings.Add(item);
                    //return Results.NotFound($"No Settings found for User ID {UserId}");
                }
                else
                {
                    item.PreferredUserName = updatedUserSettings.PreferredUserName;
                    item.AIHasVoice = updatedUserSettings.AIHasVoice;
                    item.AIVoiceID = updatedUserSettings.AIVoiceID;
                    item.UserTheme = updatedUserSettings.UserTheme;
                }
                await db.SaveChangesAsync();
                return Results.Ok();
            })
                .WithName("UpdateAppUserSettings");

            #region Broken - Keeping till sure to remove
            group.MapGet("/Favourites/SavedQueries/GetById/{UserId}", async (string UserId, PulseDbContext dbContext) =>
            {

                var favQrys = await dbContext.UserFavouriteQuery
                    .AsNoTracking()
                    .Where(a => a.UserId == UserId)
                    .ToListAsync();
                if (favQrys == null) { favQrys = new List<UserFavouriteQry>(); }
                return Results.Ok(favQrys);
            });

            group.MapDelete("/Favourites/SavedQueries/Delete/{id}", async (int id, PulseDbContext dbContext) =>
            {
                var item = await dbContext.UserFavouriteQuery.FindAsync(id);
                if (item == null)
                {
                    return Results.NotFound($"Item with ID {id} not found.");
                }
                dbContext.UserFavouriteQuery.Remove(item);
                await dbContext.SaveChangesAsync();
                return Results.Ok($"Item with ID {id} has been deleted.");
            });

            group.MapPost("/Favourites/SavedQueries/Add/", async (UserFavouriteQry qry, PulseDbContext dbContext) =>
            {
                dbContext.UserFavouriteQuery.Add(qry);
                await dbContext.SaveChangesAsync();
                return Results.Created($"/User/Favourites/SavedQueries/Add/{qry.Id}", qry);
            });
            #endregion
        }
    }
}
