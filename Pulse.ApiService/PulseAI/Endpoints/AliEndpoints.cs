using Microsoft.EntityFrameworkCore;
using Pulse.ApiService.PulseAI.Characters;
using Pulse.Models.AI;
using Pulse.Models.Api;
using Pulse.Models.PulseContext;

namespace Pulse.ApiService.PulseAI.Endpoints;

public static class AliAiEndpoints
{
    public static void MapAliEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/PulseAI/Ali").WithTags("AliEndpoints");

        group.MapGet("/CustomPrompts/GetAll", GetAllPromptsAsync) 
            .WithName("GetAllCustomPrompts")
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .Produces<ApiResponse<List<ContextualArea>>>(StatusCodes.Status200OK);

        //group.MapGet("/CustomPrompts/{promptid}", async (int promptid, PulseDbContext dbContext) =>
        //{
        //    var prompt = await dbContext.ContextualPromptMaster
        //        .AsNoTracking()
        //        .Where(a => a.ContextualPromptId == promptid)
        //        .Select(a => new ContextualPrompt
        //        {
        //            ContextualPromptId = a.ContextualPromptId,
        //            ContextualAreaId = a.ContextualAreaId,
        //            ContextualPromptTitle = a.ContextualPromptTitle ?? string.Empty,
        //            ContextualPromptDescription = a.ContextualPromptDescription ?? string.Empty,
        //            Prompt = a.Prompt ?? string.Empty,
        //            ApplicationUserId = a.ApplicationUserId ?? string.Empty,
        //            Likes = a.Likes
        //        })
        //        .FirstOrDefaultAsync();
        //    return Results.Ok(prompt);
        //});

        //group.MapPost("/ContextualPrompt", async (ContextualPrompt prompt, PulseDbContext dbContext) =>
        //{
        //    dbContext.ContextualPromptMaster.Add(prompt);
        //    await dbContext.SaveChangesAsync();
        //    return Results.Created($"/AI/ContextualPrompt/{prompt.ContextualPromptId}", prompt);
        //});

        //group.MapGet("/GetContextualPromptsByAreaId/{areaid}", async (int areaid, PulseDbContext dbContext) =>
        //{
        //    var prompts = await dbContext.ContextualPromptMaster
        //        .AsNoTracking()
        //        .Where(a => a.ContextualAreaId == areaid)
        //        .Select(a => new ContextualPrompt
        //        {
        //            ContextualPromptId = a.ContextualPromptId,
        //            ContextualAreaId = a.ContextualAreaId,
        //            ContextualPromptTitle = a.ContextualPromptTitle ?? string.Empty,
        //            ContextualPromptDescription = a.ContextualPromptDescription ?? string.Empty,
        //            Prompt = a.Prompt ?? string.Empty,
        //            ApplicationUserId = a.ApplicationUserId ?? string.Empty,
        //            Likes = a.Likes
        //        })
        //        .ToListAsync();
        //    return Results.Ok(prompts);
        //});


        //group.MapPost("/Tables/SendRequest", HandleUserRequestAsync)
        //    .WithName("SendTablesARequest")
        //    .WithDescription("Sends Tables a request and returns the response.")
        //    .Accepts<PulseAiRequest>("application/json")
        //    .Produces<ApiResponse<TablesResponse>>(StatusCodes.Status200OK)
        //    .ProducesProblem(400);

        //group.MapPost("/Flapper/SendRequest", HandleUserRequestAsync)
        //    .WithName("SendFlapperARequest")
        //    .WithDescription("Sends Flapper a request and returns the response.")
        //    .Accepts<PulseAiRequest>("application/json")
        //    .Produces<ApiResponse<TablesResponse>>(StatusCodes.Status200OK)
        //    .ProducesProblem(400);
    }

    //private static async Task<IResult> HandleUserRequestAsync(
    //    PulseAiRequest request,
    //    TablesAPI tablesAPI,
    //    ILoggerFactory loggerFactory)
    //{
    //    var logger = loggerFactory.CreateLogger("PulseAI");

    //    try
    //    {
    //        if (request.AiName == AiName.Tables)
    //        {
    //            request.ModelName = "sqlcoder:latest";
    //            var response = await tablesAPI.AskTablesAsync(request);
    //            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
    //        }

    //        // Handle other AI types here in the future
    //        return Results.BadRequest(ApiResponse<PulseAiResponse>.ErrorResponse(
    //            $"AI type '{request.AiName}' is not yet implemented."));
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error processing AI request");
    //        return Results.Problem("Error processing AI request");
    //    }
    //}

    private static async Task<IResult> GetAllPromptsAsync(
            PulseDbContext db,
            ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("GetAllPromptsAsync");

        try
        {
            var pAreas = await db.ContextualAreaMaster
                .AsNoTracking()
                .Select(a => new ContextualArea
                {
                    ContextAreaId = a.ContextAreaId,
                    AreaAiPrompt = a.AreaAiPrompt ?? string.Empty,
                    AreaDescription = a.AreaDescription ?? string.Empty,
                    ExpectedRequestDataFormat = a.ExpectedRequestDataFormat ?? string.Empty,
                    ContextualPrompts = a.ContextualPrompts!.Select(p => new ContextualPrompt
                    {
                        ContextualPromptId = p.ContextualPromptId,
                        ContextualAreaId = p.ContextualAreaId,
                        ContextualPromptTitle = p.ContextualPromptTitle ?? string.Empty,
                        ContextualPromptDescription = p.ContextualPromptDescription ?? string.Empty,
                        Prompt = p.Prompt ?? string.Empty,
                        ApplicationUserId = p.ApplicationUserId ?? string.Empty,
                        Likes = p.Likes
                    }).ToList()
                })
                .ToListAsync();

            if (pAreas == null || pAreas.Count == 0)
            {
                logger.LogInformation("No contextual areas found.");
                pAreas = new List<ContextualArea>();
            }

            return Results.Ok(ApiResponse<List<ContextualArea>>.SuccessResponse(pAreas));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error fetching contextual areas and prompts");
            return Results.Problem("An error occurred while fetching contextual areas and prompts.");
        }
    }
}