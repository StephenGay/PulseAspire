using Microsoft.EntityFrameworkCore;
using Pulse.Models.Api;
using Pulse.Models.Production.WorkTypes;
using Pulse.Models.PulseContext;

namespace Pulse.ApiService.Endpoints.Production;


internal static class WorkTypeEndpoints
{
    internal const string BasePath = "/Production/WorkTypes";
    public static void MapWorkTypeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(BasePath).WithTags("WorkTypes");
        
        group.MapGet("/GetAllActive", GetActiveWorkTypes)
            .WithName("GetActiveWorkTypes")
            .Produces<ApiResponse<List<WorkType>>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/Division/{divisionId}/GetAllActive", GetDivisionActiveWorkTypes)
            .WithName("GetDivisionActiveWorkTypes")
            .Produces<ApiResponse<List<DivisionWorkType>>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/Division/{divisionId}/Add", AddDivisionWorkType)
            .WithName("AddDivisionWorkType")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetActiveWorkTypes(
        PulseDbContext db,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("GetActiveWorkTypes");
        try
        {
            List<WorkType> workTypes = await db.WorkTypeMaster
                .Where(wt => wt.IsActive)
                .ToListAsync();
            
            if(workTypes == null || workTypes.Count == 0)
            {
                workTypes = new List<WorkType>();
            }

            return Results.Ok(new ApiResponse<List<WorkType>>
            {
                Success = true,
                Data = workTypes,
                Message = "Active work types retrieved successfully.",
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching active work types.");
            return Results.Problem("An error occurred while fetching active work types.");
        }
    }

    
    private static async Task<IResult> GetDivisionActiveWorkTypes(
        PulseDbContext db,
        string divisionId,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("GetDivisionActiveWorkTypes");
        try
        {
            List<DivisionWorkType> workTypes = await db.DivisionWorkTypes
                .Where(wt => wt.IsActive && wt.DivisionId == divisionId)
                .Include(wt => wt.WorkType)
                .ToListAsync();

            if (workTypes == null || workTypes.Count == 0)
            {
                workTypes = new List<DivisionWorkType>();
            }

            return Results.Ok(new ApiResponse<List<DivisionWorkType>>
            {
                Success = true,
                Data = workTypes,
                Message = "Active work types for the division retrieved successfully.",
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching active work types for the division.");
            return Results.Problem("An error occurred while fetching active work types for the division.");
        }
    }

    private static async Task<IResult> AddDivisionWorkType(
        PulseDbContext db,
        DivisionWorkType newDivWT,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("AddDivisionWorkType");
        try
        {
            await db.DivisionWorkTypes.AddAsync(newDivWT);
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse
            {
                Success = true,
                Message = "Division work type added successfully.",
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while adding division work type.");
            return Results.Problem("An error occurred while adding division work type.");
        }
    }
}