using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Pulse.ApiService.Services;
using Pulse.Models.Api;
using Pulse.Models.Communication;
using Pulse.Models.CustomComponents;
using Pulse.Models.Misc;
using Pulse.Models.Production;
using Pulse.Models.PulseContext;
using Pulse.Models.Users;
using static Pulse.Models.Api.ApiEndpoints;

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



            group.MapPost("/Favourites/Queries/Add/", async (UserFavouriteQuery qry, PulseDbContext dbContext) =>
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

            group.MapGet("/SpeedDial/GetByUserId/{UserId}", async (string UserId, PulseDbContext db) =>
            {
                var speedDials = await db.AspNetUserSpeedDials
                    .AsNoTracking()
                    .Where(p => p.ApplicationUserId == UserId)
                    .ToListAsync();
                if (speedDials == null)
                {
                    speedDials = new List<ApplicationUserSpeedDial>();
                }
                return Results.Ok(new ApiResponse<List<ApplicationUserSpeedDial>> { Data = speedDials, Success = true });
            })
                .WithName("GetUserSpeedDial");

            group.MapPost("/SpeedDial/Add/", async (ApplicationUserSpeedDial sd, PulseDbContext dbContext) =>
            {
                dbContext.AspNetUserSpeedDials.Add(sd);
                await dbContext.SaveChangesAsync();
                return Results.Created($"/User/SpeedDial/Add/{sd.Id}", sd);
            });

            group.MapDelete("/SpeedDial/Delete/{id}", async (int id, PulseDbContext dbContext) =>
            {
                var item = await dbContext.AspNetUserSpeedDials.FindAsync(id);
                if (item == null)
                {
                    return Results.NotFound($"Item with ID {id} not found.");
                }
                dbContext.AspNetUserSpeedDials.Remove(item);
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

            #region Group Chats

            //group.MapPost("/GroupChat/Invite", async (ApplicationUserStream stream, PulseDbContext dbContext) =>
            //{
            //    stream.IsInvite = true;
            //    dbContext.AspNetUserStreams.Add(stream);
            //    await dbContext.SaveChangesAsync();
            //    return Results.Created($"/User/GroupChat/Invite/{stream.StreamName}", ApiResponse<ApplicationUserStream>.SuccessResponse(stream));
            //})
            //    .WithName("InviteUserToGroupChat")
            //    .Produces<ApiResponse<ApplicationUserStream>>(StatusCodes.Status201Created);

            group.MapPost("/GroupChat/Create", async (GroupChat groupChat, PulseDbContext dbContext) =>
            {
                dbContext.GroupChats.Add(groupChat);
                await dbContext.SaveChangesAsync();
                return Results.Created($"/User/GroupChat/Create/{groupChat.GroupName}", ApiResponse<GroupChat>.SuccessResponse(groupChat));
            })
                .WithName("CreateUserStream")
                .Produces<ApiResponse<GroupChat>>(StatusCodes.Status201Created);


            group.MapPost("/Subscriptions/Subscribe", AddUserSubscription)
                .WithName("AddUserSubscription")
                .Produces<ApiResponse<ApplicationUserStream>>(StatusCodes.Status201Created);

            group.MapGet("/Subscriptions/GetSystemStreams/{UserId}", async (string UserId, PulseDbContext db) =>
            {
                var streams = await db.AspNetUserStreams
                    .AsNoTracking()
                    .Where(p => p.ApplicationUserID == UserId && p.StreamType == StreamType.System)
                    .ToListAsync();
                if (streams == null)
                {
                    streams = new List<ApplicationUserStream>();
                }
                return Results.Ok(new ApiResponse<List<ApplicationUserStream>> { Data = streams, Success = true });
            })
                .WithName("GetUserSystemStreams")
                .Produces<ApiResponse<List<ApplicationUserStream>>>(StatusCodes.Status200OK);

            group.MapGet("/Subscriptions/GetGroupChats/{UserId}", async (string UserId, PulseDbContext db) =>
            {
                var streams = await db.AspNetUserStreams
                    .AsNoTracking()
                    .Where(p => p.ApplicationUserID == UserId && p.StreamType == StreamType.ChatGroup)
                    .ToListAsync();
                if (streams == null)
                {
                    streams = new List<ApplicationUserStream>();
                }
                return Results.Ok(new ApiResponse<List<ApplicationUserStream>> { Data = streams, Success = true });
            })
                .WithName("GetUserGroupChats")
                .Produces<ApiResponse<List<ApplicationUserStream>>>(StatusCodes.Status200OK);

            group.MapDelete("/Subscriptions/Unsubscribe/{id}", async (int id, PulseDbContext dbContext) =>
            {
                var item = await dbContext.AspNetUserStreams.FindAsync(id);
                if (item == null)
                {
                    return Results.NotFound($"Item with ID {id} not found.");
                }
                dbContext.AspNetUserStreams.Remove(item);
                await dbContext.SaveChangesAsync();
                return Results.Ok(new ApiResponse<string> { Data = $"Item with ID {id} has been deleted.", Success = true });
            });

            group.MapPut("/DataStream/AcceptInvite", async (ApplicationUserStream updatedStream, PulseDbContext dbContext) =>
            {
                var item = await dbContext.AspNetUserStreams.FindAsync(updatedStream.ApplicationUserID, updatedStream.StreamName);
                if (item == null)
                {
                    return Results.NotFound($"Item with ID {updatedStream.ApplicationUserID} and StreamName {updatedStream.StreamName} not found.");
                }
                await dbContext.SaveChangesAsync();
                return Results.Ok(new ApiResponse<ApplicationUserStream> { Data = item, Success = true });
            });

#endregion

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
        private static async Task<ApiResponse<ApplicationUserStream>> AddUserSubscription(ApplicationUserStream stream, PulseDbContext dbContext)
        {
            dbContext.AspNetUserStreams.Add(stream);
            var result = await dbContext.SaveChangesAsync();
            if(result <= 0)
            {
                return new ApiResponse<ApplicationUserStream> { Data = null, Success = false, Message = "Failed to add user subscription." };
            }
            return new ApiResponse<ApplicationUserStream> { Data = stream, Success = true };
        }
    }
}

