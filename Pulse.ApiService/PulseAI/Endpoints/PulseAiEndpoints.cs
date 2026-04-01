using OllamaSharp.Models.Chat;
using Pulse.ApiService.PulseAI.Characters;
using Pulse.ApiService.PulseAI.Services;
using Pulse.Models.AI;
using Pulse.Models.Api;

namespace Pulse.ApiService.PulseAI.Endpoints;

public static class PulseAiEndpoints
{
    public static void MapPulseAiEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/PulseAI").WithTags("PulseAIEndpoints");

        group.MapPost("/Tables/SendRequest", HandleUserRequestAsync)
            .WithName("SendTablesARequest")
            .WithDescription("Sends Tables a request and returns the response.")
            .Accepts<PulseAiRequest>("application/json")
            .Produces<ApiResponse<TablesResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(400);

        group.MapPost("/Flapper/SendRequest", HandleUserRequestAsync)
            .WithName("SendFlapperARequest")
            .WithDescription("Sends Flapper a request and returns the response.")
            .Accepts<PulseAiRequest>("application/json")
            .Produces<ApiResponse<TablesResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(400);
    }

    private static async Task<IResult> HandleUserRequestAsync(
        PulseAiRequest request,
        TablesAPI tablesAPI,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("PulseAI");

        try
        {
            if (request.AiName == AiName.Tables)
            {
                request.ModelName = "sqlcoder:latest";
                var response = await tablesAPI.AskTablesAsync(request);
                return response.Success ? Results.Ok(response) : Results.BadRequest(response);
            }

            // Handle other AI types here in the future
            return Results.BadRequest(ApiResponse<PulseAiResponse>.ErrorResponse(
                $"AI type '{request.AiName}' is not yet implemented."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing AI request");
            return Results.Problem("Error processing AI request");
        }
    }
}
