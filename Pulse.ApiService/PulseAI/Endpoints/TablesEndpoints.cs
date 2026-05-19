using Pulse.ApiService.PulseAI.Characters;
using Pulse.Models.AI;
using Pulse.Models.Api;
using static Pulse.Models.AI.Tables.TablesChatStructures;

namespace Pulse.ApiService.PulseAI.Endpoints
{
    public static class TablesEndpoints
    {
        public static void MapTablesEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/PulseAI/Tables").WithTags("TablesEndpoints");

            group.MapPost("/SendRequest", TablesRequestAsync)
                .WithName("SendTablesARequest")
                .WithDescription("Sends Tables a request and returns the response.")
                .Accepts<TablesRequest>("application/json")
                .Produces<ApiResponse<List<Dictionary<string, object>>>>(StatusCodes.Status200OK)
                .ProducesProblem(400);
        }

        private static async Task<IResult> TablesRequestAsync(
            TablesRequest request,
            TablesAPI tablesAPI,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("TablesRequest");

            try
            {
                var response = await tablesAPI.AskTablesAsync(request);
                return response.Success ? Results.Ok(response) : Results.BadRequest(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Tables request");
                return Results.Problem("Error processing Tables request");
            }
        }
    }
}
