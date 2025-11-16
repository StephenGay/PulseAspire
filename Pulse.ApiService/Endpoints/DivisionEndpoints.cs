using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Pulse.Models.CustomComponents;
using Pulse.Models.Customers;
using Pulse.Models.Organizational;
using Pulse.Models.Production;
using Pulse.Models.PulseContext;

namespace Pulse.ApiService.Endpoints
{
    internal static class DivisionEndpoints
    {
        internal const string BasePath = "/Divisions";
        internal const string ByDivIdPath = "/{divisionid}";
        

        public static void MapDivisionEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup(BasePath).WithTags("Divisions");

            group.MapGet("/GetAll", async ( PulseDbContext db) =>
                await db.DivisionMaster
                .Where(d => d.IsActive)
                .ToListAsync())
                .WithName("GetAllDivisions")
                .Produces<List<Division>>(StatusCodes.Status200OK);

            group.MapGet(ByDivIdPath, async (string divisionid, PulseDbContext db) =>
            await db.DivisionMaster.AsNoTracking()
                .FirstOrDefaultAsync(c => c.DivisionID == divisionid)
                is { } c
                    ? Results.Ok(c)
                    : Results.NotFound())
                .WithName("GetDivisionById")
                .Produces<Division>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/GetWorkTypes", async (PulseDbContext db) =>
                await db.WorkTypeMaster.AsNoTracking()
                .Where(d => d.IsActive)
                .ToListAsync())
                .WithName("GetAllWorkTypes")
                .Produces<List<WorkType>>(StatusCodes.Status200OK);

            group.MapGet(ByDivIdPath + "/GetProductionStagesByWorkType/{workTypeId}", async(string divisionId, int workTypeId, PulseDbContext db) =>
                await db.ProductionStageMaster
                .Where(w => w.IsActive && w.WorkTypeID == workTypeId && w.DivisionID == divisionId)
                .OrderBy(e => e.StepNo)
                .ToListAsync())
                .WithName("GetWorkTypeProdStagesByDiv")
                .Produces<List<ProductionStage>>(StatusCodes.Status200OK);

            group.MapGet("/GetProductionStageEquipment/{productionStageId}", async (int productionStageId, PulseDbContext db) =>
                await db.EquipmentCapabilities
                .AsNoTracking()
                .Where(w => w.ProductionStageID == productionStageId)
                .Include(e => e.EquipmentItem)
                    .ThenInclude(i => i.EquipmentCategory!) 
                .ToListAsync())
                .WithName("GetProdStageEquipment")
                .Produces<List<EquipmentCapability>>(StatusCodes.Status200OK);

            group.MapGet(ByDivIdPath + "/GetWIP", async (string divisionId, PulseDbContext db) =>
            {
                try
                {
                    const string sql = @"
                        SELECT 
                            wo.WorksOrderNo, 
                            wo.DivisionID, 
                            wtm.WorkTypeName, 
                            wtm.TargetWorkingDays, 
                            cm.ClientName, 
                            wo.UndelQty, 
                            wo.MaterialCost,
                            wo.SellPrice, 
                            wo.ProductionStageID, 
                            wo.ProductionStageName, 
                            wo.ProgressChange, 
                            wo.RequiredDate, 
                            wcm.WorkCentreName,
                            wo.DateStarted, 
                            wo.Description, 
                            wcfm.WorkCentreID
                        FROM dbo.WorkCentreMaster wcm 
                        FULL OUTER JOIN dbo.ClientMaster cm 
                        RIGHT OUTER JOIN dbo.WorksOrder wo ON cm.FullClientID = wo.FullClientID 
                        LEFT OUTER JOIN dbo.WorkTypeMaster wtm ON wo.WorkTypeID = wtm.WorkTypeID 
                        LEFT OUTER JOIN dbo.WorkCentreFunctionsMapping wcfm ON wo.ProductionStageID = wcfm.ProductionStageID 
                        ON wcm.WorkCentreId = wcfm.WorkCentreID
                        WHERE (wo.UndelQty > 0) AND (wo.DivisionID = @divisionId)
                    ";

                                var parameter = new SqlParameter("@divisionId", divisionId);

                                var wipItems = await db.Database.SqlQueryRaw<WorkInProgressDto>(sql, parameter).ToListAsync();

                                if (!wipItems.Any()) return Results.NotFound($"No WIP found for division {divisionId}");

                                return Results.Ok(wipItems);
                            }
                            catch (Exception ex)
                            {
                                // Log ex (inject ILogger if needed)
                                return Results.Problem($"Error fetching WIP: {ex.Message}");
                            }
                        })
            .WithName("GetWipByDivision")
            .WithOpenApi();

            group.MapGet(ByDivIdPath + "/GetWIP/Planning", async (string divisionId, PulseDbContext db) =>
            {
                const string sql = @"
                    SELECT  wo.WorksOrderNo      AS Id,
                            CONCAT(cm.ClientName, ' – ', wo.WorksOrderNo) AS Title,
                            wo.RequiredDate       AS Start,
                            wo.RequiredDate      AS [End],
                            CASE WHEN wo.UndelQty > 0 THEN '#ff9f89' ELSE '#c3e6cb' END AS BackgroundColor
                    FROM    dbo.WorksOrder wo
                            LEFT JOIN dbo.ClientMaster cm ON cm.FullClientID = wo.FullClientID
                    WHERE   wo.DivisionID = @divisionId
                      AND   wo.UndelQty > 0";

                            var param = new SqlParameter("@divisionId", divisionId);

                            var events = await db.Database
                                .SqlQueryRaw<CalendarEvent>(sql, param)
                                .ToListAsync();

                            return Results.Ok(events);
                        })
            .WithName("GetWipPlan")
            .WithOpenApi();

            group.MapGet(ByDivIdPath + "/GetWIP/PlanItems", async (string divisionId, PulseDbContext db) =>
            {

                var items = await db.ProductionPlanItems
                    .Where(p => p.DivisionID == divisionId && p.Status == "Planned")
                    .Include(wo => wo.WorksOrder)
                        .ThenInclude(c => c.Customer)

                    .Select(p => new ProductionPlanEvent
                    {
                        Id = p.ProductionPlanItemID,
                        ResourceId = p.EquipmentItemID,
                        Title = p.WorkOrderNo.ToString(),
                        Start = p.PlannedStartTime,
                        End = p.PlannedEndTime,
                        BackgroundColor = "#c3e6cb",
                        ClientName = p.WorksOrder != null ? p.WorksOrder.Customer.ClientName : null,
                        Description = p.WorksOrder != null ? p.WorksOrder.Description : null
                    })
                    .ToListAsync();


                //const string sql = @"
                //    SELECT  ProductionPlanItemID      AS Id,
                //            EquipmentItemID AS ResourceId,
                //            WorkOrderNo AS Title,
                //            PlannedStartTime       AS Start,
                //            PlannedEndTime      AS [End],
                //            '#c3e6cb' AS BackgroundColor
                //    FROM    dbo.ProductionPlanItems
                //    WHERE   DivisionID = @divisionId
                //      AND   Status = 'Planned'";

                //var param = new SqlParameter("@divisionId", divisionId);

                //var events = await db.Database
                //    .SqlQueryRaw<ProductionPlanEvent>(sql, param)
                //    .ToListAsync();

                return Results.Ok(items);
            })
            .WithName("GetWipPlannedItems")
            .WithOpenApi();

            group.MapGet(ByDivIdPath + "/GetWIP/UnPlannedItems", async (string divisionId, PulseDbContext db) =>
            {
                var items = await db.ProductionPlanItems
                    .Where(p => p.DivisionID == divisionId && p.Status == "Unplanned")
                    .Include(wo => wo.WorksOrder)
                        .ThenInclude(c => c.Customer)

                    .Select(p => new ProductionUnPlannedEvent
                    {
                        Id = p.ProductionPlanItemID,
                        //ResourceId = p.EquipmentItemID,
                        Title = p.WorkOrderNo.ToString(),
                        //Start = p.PlannedStartTime,
                        //End = p.PlannedEndTime,
                        BackgroundColor = "#c3e6cb",
                        ClientName = p.WorksOrder != null ? p.WorksOrder.Customer.ClientName : null,
                        Description = p.WorksOrder != null ? p.WorksOrder.Description : null,
                        Stage = p.ProductionStage.ProductionStageName
                    })
                    .ToListAsync();

                //const string sql = @"
                //    SELECT  ProductionPlanItemID      AS Id,
                //            CONCAT('WO No: ', WorkOrderNo) AS Title,
                //            '#c3e6cb' AS BackgroundColor
                //    FROM    dbo.ProductionPlanItems
                //    WHERE   DivisionID = @divisionId
                //      AND   Status = 'Unplanned'";

                //var param = new SqlParameter("@divisionId", divisionId);

                //var events = await db.Database
                //    .SqlQueryRaw<ProductionUnPlannedEvent>(sql, param)
                //    .ToListAsync();

                return Results.Ok(items);
            })
            .WithName("GetWipUnPlannedItems")
            .WithOpenApi();

            group.MapGet(ByDivIdPath + "/GetPlanningEquipment", async (string divisionId, PulseDbContext db) =>
            {
                const string sql = @"
                    SELECT  EquipmentItemID      AS Id,
                            EquipmentItemDescription AS Title
                    FROM    dbo.EquipmentItems
                    WHERE   DivisionID = @divisionId
                      AND   IsActive=1";

                var param = new SqlParameter("@divisionId", divisionId);

                var resources = await db.Database
                    .SqlQueryRaw<ProductionPlanResource>(sql, param)
                    .ToListAsync();

                return Results.Ok(resources);
            })
            .WithName("GetWipPlanEquip")
            .WithOpenApi();

            group.MapGet("/WIP/GetPlanItem/{itemID}", async (int itemID, PulseDbContext db) =>
            {
                var item = await db.ProductionPlanItems
                    .Where(p => p.ProductionPlanItemID == itemID)
                    .Include(wo => wo.WorksOrder)
                        .ThenInclude(c => c.Customer)
                    .Select(p => new ProductionPlanEvent
                    {
                        Id = p.ProductionPlanItemID,
                        ResourceId = p.EquipmentItemID,
                        Title = p.WorkOrderNo.ToString(),
                        Start = p.PlannedStartTime,
                        End = p.PlannedEndTime,
                        BackgroundColor = "#c3e6cb",
                        ClientName = p.WorksOrder != null ? p.WorksOrder.Customer.ClientName : null,
                        Description = p.WorksOrder != null ? p.WorksOrder.Description : null
                    })
                    .FirstOrDefaultAsync();
                if (item == null)
                {
                    return Results.NotFound($"No Plan Item found with ID {itemID}");
                }
                return Results.Ok(item);
            })
            .WithName("GetWipPlanItemById")
            .WithOpenApi();

            group.MapPut("/WIP/UpdateProductionPlanItem/{id}", async (int id, ProductionPlanItem updatedItem, PulseDbContext db) =>
            {
                var item = await db.ProductionPlanItems
                    .FirstOrDefaultAsync(p => p.ProductionPlanItemID == id);
                if (item == null)
                {
                    return Results.NotFound($"No Plan Item found with ID {id}");
                }
                item.EquipmentItemID = updatedItem.EquipmentItemID;
                item.PlannedStartTime = updatedItem.PlannedStartTime;
                item.PlannedEndTime = updatedItem.PlannedEndTime;
                item.Status = "Planned";
                await db.SaveChangesAsync();
                return Results.Ok(updatedItem);
            })
                .WithName("UpdateWIPPlanItem")
                .WithOpenApi();
        }
                
    }
}
