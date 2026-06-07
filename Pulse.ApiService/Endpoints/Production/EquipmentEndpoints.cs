using Microsoft.EntityFrameworkCore;
using Pulse.Models.Api;
using Pulse.Models.CustomComponents;
using Pulse.Models.Dtos.Equipment;
using Pulse.Models.Misc;
using Pulse.Models.Production;
using Pulse.Models.Production.Layout;
using Pulse.Models.Production.WorkTypes;
using Pulse.Models.PulseContext;
using static Pulse.Models.Api.ApiEndpoints.Divisions;
using static Pulse.Models.Api.ApiEndpoints.Divisions.WithDivisionID.Factory;

namespace Pulse.ApiService.Endpoints.Production;

internal static class EquipmentEndPoints
{
    internal const string BasePath = "/Production/Equipment";
    public static void MapEquipmentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(BasePath).WithTags("Equipment");

        group.MapGet("/ByDivision/GetList/{divisionId}", GetDivisionList)
            .WithName("GetDivisionList")
            .Produces<ApiResponse<List<EquipmentTableItemDto>>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/ByDivision/GetFullList/{divisionId}", GetDivisionEquipmentList)
            .WithName("GetDivisionEquipmentList")
            .Produces<ApiResponse<List<EquipmentItem>>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/GetByID/{equipmentId}", GetEquipmentByID)
            .WithName("GetEquipmentByID")
            .Produces<ApiResponse<EquipmentItem>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/Categories/Get", GetEquipmentCategories)
            .WithName("GetEquipmentCategories")
            .Produces<ApiResponse<List<EquipmentCategory>>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/MasterFile/Update", UpdateEquipmentMaster)
                .WithName("UpdateEquipmentMaster")
                .Produces<ApiResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

        group.MapGet("/ByWorkCentre/Unzoned/{workCentreId}", GetUnzonedByWorkCentre)
                .WithName("GetWCUnzonedEquipment")
                .Produces<ApiResponse<List<UnzonedEquipmentDto>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetDivisionList(
        PulseDbContext db,
        string divisionId,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("GetDivisionEquipmentList");
        try
        {
            var eqItems = await db.EquipmentItems
                .Where(eq => eq.DivisionID == divisionId)
                .Include(eq => eq.EquipmentCategory)
                .ToListAsync();

            var eqItemsDto = new List<EquipmentTableItemDto>();

            foreach (var item in eqItems)
            {
                eqItemsDto.Add(new EquipmentTableItemDto
                (
                    item.EquipmentItemID,
                    item.EquipmentItemDescription,
                    item.EquipmentCategory?.EquipmentCategoryName,
                    item.ManufacturerName,
                    item.Model,
                    item.IsOperational
                ));

            }

            return Results.Ok(new ApiResponse<List<EquipmentTableItemDto>>
            {
                Success = true,
                Data = eqItemsDto,
                Message = "Division Equipment List retrieved successfully.",
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching Division Equipment List.");
            return Results.Problem("An error occurred while fetching Division Equipment List.");
        }
    }

    private static async Task<IResult> GetDivisionEquipmentList(
        PulseDbContext db,
        string divisionId,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("GetDivisionEquipList");
        try
        {
            var eqItems = await db.EquipmentItems
                .Where(eq => eq.DivisionID == divisionId)
                .Include(eq => eq.EquipmentCategory)
                .ToListAsync();

            

            return Results.Ok(new ApiResponse<List<EquipmentItem>>
            {
                Success = true,
                Data = eqItems,
                Message = "Division Equipment List retrieved successfully.",
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching Division Equipment List.");
            return Results.Problem("An error occurred while fetching Division Equipment List.");
        }
    }
    private static async Task<IResult> GetEquipmentByID(
        PulseDbContext db,
        string equipmentId,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("GetEquipmentByID");
        try
        {
            var equipItem = await db.EquipmentItems
                .Where(eq => eq.EquipmentItemID == equipmentId)
                .FirstOrDefaultAsync();

            return Results.Ok(new ApiResponse<EquipmentItem>
            {
                Success = true,
                Data = equipItem,
                Message = "Equipment Item retrieved successfully.",
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching Equipment Item.");
            return Results.Problem("An error occurred while fetching Equipment Item.");
        }
    }

    private static async Task<IResult> UpdateEquipmentMaster(
            EquipmentItem equip,
            PulseDbContext db,
            ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("UpdateEquipmentMaster");
        try
        {
            var z = db.EquipmentItems
                .Where(e => e.EquipmentItemID == equip.EquipmentItemID)
                .FirstOrDefault();

            if (z == null)
            {
                logger.LogError("Error updating Equipment Master : Not Found");
                return Results.StatusCode(StatusCodes.Status404NotFound);

            }

            db.Entry(z).CurrentValues.SetValues(equip);
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse
            {
                Success = true,
                Message = "Equipment Master was updated",
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating Equipment Master");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
    private static async Task<IResult> GetEquipmentCategories(
        PulseDbContext db,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("GetEquipmentCategories");
        try
        {
            var eqCat = await db.EquipmentCategoryMaster
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<EquipmentCategory>>
            {
                Success = true,
                Data = eqCat,
                Message = "Equipment Item retrieved successfully.",
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching Equipment Categories.");
            return Results.Problem("An error occurred while fetching Equipment Categories.");
        }
    }

    private static async Task<IResult> GetUnzonedByWorkCentre(
        int workCentreId,
        PulseDbContext db,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("GetUnzonedByWorkCentre");
        try
        {
            var stageCount = (await db.Database
                .SqlQueryRaw<int>(
                    @"SELECT COUNT(*) FROM WorkCentreFunctionsMapping WHERE WorkCentreID = {0}",
                    workCentreId
                    )
                    .ToListAsync())
                    .FirstOrDefault();

            if (stageCount == 0)
            {
                return Results.BadRequest($"Work centre {workCentreId} has no allocated production stages.");
            }

            var equipment = await db.Database
                .SqlQueryRaw<UnzonedEquipmentDto>(
                    @"SELECT 
                                ei.EquipmentItemID AS ID , 
                                ei.EquipmentItemDescription AS Description
                            FROM EquipmentCapabilities ec
                            JOIN EquipmentItems ei ON ec.EquipmentItemID = ei.EquipmentItemID
                            JOIN ProductionStageMaster ps ON ec.ProductionStageID = ps.ProductionStageID
                            WHERE ec.ProductionStageID IN (
                                SELECT ProductionStageID FROM WorkCentreFunctionsMapping WHERE WorkCentreID = {0}
                            ) AND ei.ZoneID IS NULL 
                            GROUP BY ei.EquipmentItemID, ei.EquipmentItemDescription",
                    workCentreId
                )
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<UnzonedEquipmentDto>>
            {
                Success = true,
                Data = equipment,
                Message = "Equipment Item retrieved successfully.",
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching Work Centre Unzoned Equipment.");
            return Results.Problem("An error occurred while fetching Work Centre Unzoned Equipment.");
        }
    }
}
