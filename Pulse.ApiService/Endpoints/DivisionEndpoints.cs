using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Pulse.Models.Api;
using Pulse.Models.CustomComponents;
using Pulse.Models.Customers;
using Pulse.Models.Organizational;
using Pulse.Models.Permissions;
using Pulse.Models.Production;
using Pulse.Models.Production.Layout;
using Pulse.Models.PulseContext;
using Pulse.Models.Users;
using System.Linq;

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
                .ToListAsync())
                .WithName("GetAllDivisions")
                .Produces<List<Division>>(StatusCodes.Status200OK);

            group.MapGet("/GetActive", async (PulseDbContext db) =>
            {
                var divisions = await db.DivisionMaster
                    .Where(d => d.IsActive)
                    .ToListAsync();
                return Results.Ok(new ApiResponse<List<Division>> { Data = divisions, Success = true });
            })
                .WithName("GetActiveDivisions")
                .Produces<ApiResponse<List<Division>>>(StatusCodes.Status200OK);

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

            group.MapGet(ByDivIdPath + "/GetAllProductionStagesByDiv", async (string divisionId, PulseDbContext db) =>
                await db.ProductionStageMaster
                .Where(w => w.IsActive && w.DivisionID == divisionId)
                .Include(p => p.WorkType)
                .ToListAsync())
                .WithName("GetProdStagesByDiv")
                .Produces<List<ProductionStage>>(StatusCodes.Status200OK);

            // Get all factory layout zones for a division
            group.MapGet("/WithDivisionID/{divisionid}/Factory/FactoryLayout/Zones/{parentId}/GetAll", GetAllDivisionFactoryLayoutZones)
                .WithName("GetAllDivisionFactoryLayoutZones")
                .Produces<ApiResponse<List<FactoryZone>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapPost("/WithDivisionID/{divisionid}/Factory/FactoryLayout/{parentId}/SaveLayout", SaveFactoryLayout)
                .WithName("SaveFactoryLayout")
                .Produces<ApiResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            // Get all work centres for a division
            group.MapGet("/WithDivisionID/{divisionid}/Factory/WorkCentres/GetAll", GetAllDivisionWorkCentres)
                .WithName("GetAllDivisionWorkCentres")
                .Produces<ApiResponse<List<WorkCentre>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapGet(ByDivIdPath + "/GetWorkCentres", async (string divisionId, PulseDbContext db) =>
                await db.WorkCentreMaster.AsNoTracking()
                .Where(w => w.DivisionID == divisionId)
                .ToListAsync())
                .WithName("GetWorkCentresByDivID")
                .Produces<List<WorkCentre>>(StatusCodes.Status200OK);

            group.MapGet("/GetProductionStageEquipment/{productionStageId}", async (int productionStageId, PulseDbContext db) =>
                await db.EquipmentCapabilities
                .AsNoTracking()
                .Where(w => w.ProductionStageID == productionStageId)
                .Include(e => e.EquipmentItem)
                    .ThenInclude(i => i.EquipmentCategory!) 
                .ToListAsync())
                .WithName("GetProdStageEquipment")
                .Produces<List<EquipmentCapability>>(StatusCodes.Status200OK);

            group.MapPost("/Equipment/Capabilities/Add/", async (EquipmentCapability equipCap, PulseDbContext dbContext) =>
            {
                dbContext.EquipmentCapabilities.Add(equipCap);
                await dbContext.SaveChangesAsync();
                return Results.Created("Divisions/Equipment/Capabilities/Add/",equipCap);
            });

            group.MapPost("/WorkCentre/Functions/Add/", async (WorkCentreFunctions wcFunc, PulseDbContext dbContext) =>
            {
                dbContext.WorkCentreFunctionsMaster.Add(wcFunc);
                await dbContext.SaveChangesAsync();
                return Results.Created("Divisions/WorkCentre/Functions/Add/", wcFunc);
            });

            group.MapGet(ByDivIdPath + "/GetDivisionEquipment", async (string divisionId, PulseDbContext db) =>
                await db.EquipmentItemMaster
                .AsNoTracking()
                .Where(eq => eq.DivisionID == divisionId && eq.IsActive)
                .Include(e => e.EquipmentCategory)
                .ToListAsync())
                .WithName("GetDivisionEquipment")
                .Produces<List<EquipmentItem>>(StatusCodes.Status200OK);

            group.MapGet(ByDivIdPath + "/GetWIP", async (string divisionId, PulseDbContext db) =>
            {
                try
                {
                    const string sql = @"
                        SELECT 
                            wo.WorksOrderNo, 
                            wo.DivisionID,
                            wo.WorkTypeID,
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
            .WithName("GetWipByDivision");

            group.MapGet("/Production/WorkOrder/{WorkOrderNo}/CreateProductionPlan", async (int WorkOrderNo, PulseDbContext db) =>
            {
                var WO = db.WorksOrder.AsNoTracking()
                        .Where(w => w.WorksOrderNo == WorkOrderNo)
                        .FirstOrDefault();
                var wtPS = await db.ProductionStageMaster
                    .Where(ps => ps.IsActive && ps.WorkTypeID == WO.WorkTypeID && ps.DivisionID == WO.DivisionID && !ps.IsOptional)
                    .OrderBy(e => e.StepNo)
                    .ToListAsync();

                int sNo = 0;
                foreach (var item in wtPS)
                {
                    sNo++;
                    var ppi = new ProductionPlanItem()
                    {
                        WorkOrderNo = WO.WorksOrderNo,
                        DivisionID = WO.DivisionID,
                        StepNo = sNo,
                        ProductionStageID = item.ProductionStageId,
                        Status = "Unplanned",
                    };
                    db.ProductionPlanItems.Add(ppi);
                    await db.SaveChangesAsync();
                }
                var woPP = db.ProductionPlanItems.AsNoTracking()
                            .Where(w => w.WorkOrderNo == WorkOrderNo)
                            .ToListAsync();
            })
                            .Produces<List<ProductionPlanItem>>(StatusCodes.Status200OK);

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
            .WithName("GetWipPlan");

            group.MapGet(ByDivIdPath + "/GetWIP/PlanItems", async (string divisionId, PulseDbContext db) =>
            {
                TimeSpan d = new TimeSpan(0, 30, 0);
                
                var items = await db.ProductionPlanItems
                    .Where(p => p.DivisionID == divisionId && (p.Status == "Planned" || p.Status=="Started"))
                    .Include(wo => wo.WorksOrder)
                        .ThenInclude(c => c.Customer)

                    .Select(p => new ProductionPlanEvent
                    {
                        Id = p.ProductionPlanItemID,
                        ResourceId = p.EquipmentItemID,
                        Title = p.WorkOrderNo.ToString(),
                        Start = p.Status == "Started" ? p.ActualStartTime : p.PlannedStartTime,
                        End = p.Status == "Started" ? p.ActualStartTime + d : p.PlannedEndTime,
                        BackgroundColor = p.Status == "Started" ? "#009900" : p.PlannedStartTime > DateTime.Now ? "#66c2ff" : "#ff3333",
                        ClientName = p.WorksOrder != null ? p.WorksOrder.Customer.ClientName : null,
                        Description = p.WorksOrder != null ? p.WorksOrder.Description : null
                    })
                    .ToListAsync();
                return Results.Ok(items);
            })
            .WithName("GetWipPlannedItems");

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
                return Results.Ok(items);
            })
            .WithName("GetWipUnPlannedItems");

            group.MapGet("/GetWIP/UnPlannedItems/ByWorkCentre/{workCentreId}", async (int workCentreId, PulseDbContext db) =>
            {
                var supportedStageIds = await db.Set<WorkCentreFunctions>()
                    .Where(wcf => wcf.WorkCentreID == workCentreId)
                    .Select(wcf => wcf.ProductionStageID)
                    .ToListAsync();

                if (!supportedStageIds.Any())
                {
                    return Results.BadRequest($"Work centre {workCentreId} has no allocated production stages.");
                }

                var items = await db.ProductionPlanItems
                    .Where(p => p.Status == "UnPlanned" && supportedStageIds.Contains(p.ProductionStageID))
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

                return Results.Ok(items);
            })
            .WithName("GetWipUnPlannedItemsByWorkCentre");

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
            .WithName("GetWipPlanEquip");

            group.MapGet("/GetPlanningEquipment/ByWorkCentre/{workCentreId}", async (int workCentreId, PulseDbContext db) =>
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
                    .SqlQueryRaw<ProductionPlanResource>(
                        @"SELECT 
                            ei.EquipmentItemID AS ID , 
                            ei.EquipmentItemDescription AS Title
                        FROM EquipmentCapabilities ec
                        JOIN EquipmentItems ei ON ec.EquipmentItemID = ei.EquipmentItemID
                        JOIN ProductionStageMaster ps ON ec.ProductionStageID = ps.ProductionStageID
                        WHERE ec.ProductionStageID IN (
                            SELECT ProductionStageID FROM WorkCentreFunctionsMapping WHERE WorkCentreID = {0}
                        )
                        GROUP BY ei.EquipmentItemID, ei.EquipmentItemDescription",
                        workCentreId
                    )
                    .ToListAsync();
                var noequip = await db.Database
                    .SqlQueryRaw<ProductionPlanResource>(
                        @"SELECT EquipmentItemID AS ID , 
                            EquipmentItemDescription AS Title
                            FROM EquipmentItems WHERE 
                            EquipmentItemID=0")
                    .FirstOrDefaultAsync();
                equipment.Add(noequip);
                return Results.Ok(equipment);
            })
            .WithName("GetWipPlanEquipByWC");

            group.MapGet("/GetWIP/PlanItems/ByWorkCentre/{workCentreId}", async (int workCentreId, PulseDbContext db) =>
            {
                var supportedStageIds = await db.Set<WorkCentreFunctions>()
                    .Where(wcf => wcf.WorkCentreID == workCentreId)
                    .Select(wcf => wcf.ProductionStageID)
                    .ToListAsync();

                    if (!supportedStageIds.Any())
                    {
                        return Results.BadRequest($"Work centre {workCentreId} has no allocated production stages.");
                    }

                var items = await db.ProductionPlanItems
                    .Where(p => (p.Status == "Planned" || p.Status == "Started") && supportedStageIds.Contains(p.ProductionStageID))
                    .Include(wo => wo.WorksOrder)
                        .ThenInclude(c => c.Customer)

                    .Select(p => new ProductionPlanEvent
                    {
                        Id = p.ProductionPlanItemID,
                        ResourceId = p.EquipmentItemID,
                        Title = p.WorkOrderNo.ToString(),
                        Start = p.PlannedStartTime,
                        End = p.PlannedEndTime,
                        
                
                    BackgroundColor = p.Status == "Started" ? "#009900" : p.PlannedStartTime > DateTime.Now ? "#66c2ff" : "#ff3333",
                        ClientName = p.WorksOrder != null ? p.WorksOrder.Customer.ClientName : null,
                        Description = p.WorksOrder != null ? p.WorksOrder.Description : null
                    })
                    .ToListAsync();

                return Results.Ok(items);
            })
            .WithName("GetWipPlannedItemsByWorkCentre");

            group.MapGet("/WIP/GetPlanCalEvent/{itemID}", async (int itemID, PulseDbContext db) =>
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
            .WithName("GetWipPlanEventById");

            group.MapGet("/WIP/GetPlanItem/{itemID}", async (int itemID, PulseDbContext db) =>
            {
                var item = await db.ProductionPlanItems
                    .Where(p => p.ProductionPlanItemID == itemID)
                    .Include(wo => wo.WorksOrder)
                        .ThenInclude(c => c.Customer)
                    .Include(wo => wo.WorksOrder)
                        .ThenInclude(wt => wt.WorkType)
                    .Include(p => p.ProductionStage)
                    .Include(e => e.EquipmentItem)
                    .FirstOrDefaultAsync();
                    if (item == null)
                    {
                        return Results.NotFound($"No Plan Item found with ID {itemID}");
                    }
                    return Results.Ok(item);
                })
                .WithName("GetWipPlanItemById");

            group.MapGet("/Production/GetProductionStageByID/{itemID}", async (int itemID, PulseDbContext db) =>
            {
                var item = await db.ProductionStageMaster
                    .Where(p => p.ProductionStageId == itemID)
                    .FirstOrDefaultAsync();
                if (item == null)
                {
                    return Results.NotFound($"No Production Stage found with ID {itemID}");
                }
                return Results.Ok(item);
            })
                .WithName("GetProductionStageById");

            group.MapGet("/Production/GetWorkCentreByID/{itemID}", async (int itemID, PulseDbContext db) =>
            {
                var item = await db.WorkCentreMaster
                    .Where(p => p.WorkCentreId == itemID)
                    .FirstOrDefaultAsync();
                if (item == null)
                {
                    return Results.NotFound($"No Work Centre found with ID {itemID}");
                }
                return Results.Ok(item);
            })
                .WithName("GetWorkCentreById");

            group.MapGet("/Production/GetWorkCentreFunctionsByID/{itemID}", async (int itemID, PulseDbContext db) =>
            {
                var item = await db.WorkCentreFunctionsMaster
                    .Where(p => p.WorkCentreID == itemID)
                    .Include(p => p.ProductionStage)
                        .ThenInclude(ps => ps.WorkType)
                    .ToListAsync();
                if (item == null)
                {
                    return Results.NotFound($"No Work Centre Functions found for Work Centre ID {itemID}");
                }
                return Results.Ok(item);
            })
                .WithName("GetWorkCentreFunctionsById");

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
                item.ActualStartTime = updatedItem.ActualStartTime;
                item.ActualEndTime = updatedItem.ActualEndTime;
                item.StepNo = updatedItem.StepNo;
                item.Status = updatedItem.Status;
                await db.SaveChangesAsync();
                return Results.Ok();
            })
                .WithName("UpdateWIPPlanItem");

            group.MapGet("/Production/WorkOrder/{WorkOrderNo}/GetProductionPlan", async (int WorkOrderNo, PulseDbContext db) =>
            {
                var pp = await db.ProductionPlanItems
                        .Where(p => p.WorkOrderNo == WorkOrderNo)
                        .Include(ps => ps.ProductionStage)
                        .ToListAsync();
                if (pp == null)
                {
                    return Results.NotFound(null);
                }
                return Results.Ok(pp);
            })
                .WithName("GetPlanByWO");
        }

        private static async Task<IResult> SaveFactoryLayout(
            string divisionid,
            string parentId,
            List<FactoryZone> zones,
            PulseDbContext db,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("SaveFactoryLayout");
            try
            {
                var existing = await db.FactoryZones
                    .Where(wc => wc.DivisionId == divisionid && wc.ParentZoneId == parentId)
                    .ToListAsync();

                //db.FactoryZones.RemoveRange(existing);

                foreach (var zone in zones)
                {
                    var existingZone = existing.FirstOrDefault(e => e.Id == zone.Id);
                    if (existingZone != null)
                    {
                        // Update existing
                        db.Entry(existingZone).CurrentValues.SetValues(zone);
                    }
                    else
                    {
                        db.FactoryZones.Add(zone);
                    }
                }
                await db.SaveChangesAsync();

                return Results.Ok(new ApiResponse
                {
                    Success = true,
                    Message = $"Factory Layout saved",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving the divisions factory layout");
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        private static async Task<IResult> GetAllDivisionFactoryLayoutZones(
            string divisionid,
            string parentId,
            PulseDbContext db,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GetAllDivisionFactoryLayoutZones");

            try
            {
                var zones = await db.FactoryZones
                    .AsNoTracking()
                    .Where(wc => wc.DivisionId == divisionid && wc.ParentZoneId == parentId)
                    .ToListAsync();

                if (zones == null) { zones = new List<FactoryZone>(); }

                return Results.Ok(new ApiResponse<List<FactoryZone>>
                {
                    Success = true,
                    Data = zones,
                    Message = $"Retrieved {zones.Count} factory layout zones",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving the divisions factory layout zones");
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        private static async Task<IResult> GetAllDivisionWorkCentres(
            string divisionid,
            PulseDbContext db,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GetAllDivisionWorkCentres");

            try
            {
                var dwc = await db.WorkCentreMaster
                    .AsNoTracking()
                    .Where(wc => wc.DivisionID == divisionid)
                    .OrderBy(wc => wc.WorkCentreName)
                    .ToListAsync();

                if (dwc == null) { dwc = new List<WorkCentre>(); }

                return Results.Ok(new ApiResponse<List<WorkCentre>>
                {
                    Success = true,
                    Data = dwc,
                    Message = $"Retrieved {dwc.Count} work centres",
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving the divisions work centres");
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
