using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Pulse.ApiService.Hubs;
using Pulse.Models.Api;
using Pulse.Models.CustomComponents;
using Pulse.Models.Dtos.Production;
using Pulse.Models.Production;
using Pulse.Models.PulseContext;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Pulse.ApiService.Endpoints.Production;

internal static class WorkInProgressEndpoints
{
    internal const string BasePath = "/Production/WorkInProgress";
    internal const string ByDivisionPath = "/Production/WorkInProgress/ByDivision";
    public static void MapWorkInProgressEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(BasePath).WithTags("WorkInProgress");
        var byDivisionGroup = routes.MapGroup(ByDivisionPath).WithTags("WorkInProgressByDivision");

        byDivisionGroup.MapGet("/Basic/{divisionID}", GetWipBasic)
            .WithName("GetWipBasicByDivision")
            .Produces<List<BaseWorkInProgressDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        byDivisionGroup.MapGet("/Basic/WithPlan/{divisionID}", GetWipBasicWithPlan)
            .WithName("GetWipBasicWithPlanByDivision")
            .Produces<List<WipItemsWithPlanDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        byDivisionGroup.MapGet("/Plan/Today/{divisionID}", GetWipPlannedToday)
            .WithName("GetWipPlannedTodayByDivision")
            .Produces<ApiResponse<List<ProductionPlanDto>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        byDivisionGroup.MapGet("/Plan/WorkCentres/GetStats/{divisionID}", GetWipWorkCentreStats)
            .WithName("GetWipWorkCentreStatsByDivision")
            .Produces<List<WorkCentreWipStatsDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        byDivisionGroup.MapGet("/Plan/GetResources/{divisionID}", GetWipPlanResources)
            .WithName("GetWipPlanResourcesByDivision")
            .Produces<ApiResponse<List<ProductionPlanResource>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        byDivisionGroup.MapPatch("/Plan/CompleteStage/{StepID}", CompleteProductionStep)
            .WithName("CompleteWipStageByDivision")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    }

    private static async Task<IResult> GetWipBasic(string divisionID, PulseDbContext dbPulse)
    {
        var wipBasic = await dbPulse.WorksOrder
            .Where(wo => wo.DivisionID == divisionID && wo.UndelQty > 0)
            .Include(wo => wo.ProductionStage)
                .ThenInclude(ps => ps.WorkCentre)
            .Select(wo => new BaseWorkInProgressDto
            {
                WorksOrderNo = wo.WorksOrderNo,
                DivisionID = wo.DivisionID,
                WorkTypeID = wo.WorkTypeID,
                UndelQty = wo.UndelQty,
                WorkCentreID = wo.ProductionStage != null ? wo.ProductionStage.WorkCentreID : null,
                WorkCentreName = wo.ProductionStage != null && wo.ProductionStage.WorkCentre != null ? wo.ProductionStage.WorkCentre.WorkCentreName : null
            })
            .ToListAsync();

        if (wipBasic == null || !wipBasic.Any())
        {
            return Results.NotFound(ApiResponse<List<BaseWorkInProgressDto>>.ErrorResponse("No work in progress items found.", statusCode: StatusCodes.Status404NotFound));
        }
        return Results.Ok(ApiResponse<List<BaseWorkInProgressDto>>.SuccessResponse(wipBasic));
    }
    private static async Task<IResult> GetWipBasicWithPlan(string divisionID, PulseDbContext dbPulse)
    {
        var wipWithPlan = await dbPulse.WorksOrder
            .Where(wo => wo.DivisionID == divisionID && wo.UndelQty > 0)
            .Include(wo => wo.ProductionPlanItems)
            .Include(wo => wo.ProductionStage)
                .ThenInclude(ps => ps.WorkCentre)
            .Select(wo => new WipItemsWithPlanDto
            {
                WorksOrderNo = wo.WorksOrderNo,
                DivisionID = wo.DivisionID,
                WorkTypeID = wo.WorkTypeID,
                UndelQty = wo.UndelQty,
                WorkCentreID = wo.ProductionStage != null ? wo.ProductionStage.WorkCentreID : null,
                WorkCentreName = wo.ProductionStage != null && wo.ProductionStage.WorkCentre != null ? wo.ProductionStage.WorkCentre.WorkCentreName : null,
                PlanItems = wo.ProductionPlanItems.ToList()
            })
            .ToListAsync();

        if (wipWithPlan == null || !wipWithPlan.Any())
        {
            return Results.NotFound(ApiResponse<List<WipItemsWithPlanDto>>.ErrorResponse("No work in progress items found.", statusCode: StatusCodes.Status404NotFound));
        }
        return Results.Ok(ApiResponse<List<WipItemsWithPlanDto>>.SuccessResponse(wipWithPlan));
    }
    private static async Task<IResult> GetWipPlannedToday(string divisionID, PulseDbContext dbPulse)
    {
        var wipPlannedToday = await dbPulse.ProductionPlanItems
            .Where(wp => wp.DivisionID == divisionID && wp.PlannedStartTime.HasValue && wp.PlannedStartTime.Value.Date == DateTime.Today)
            .Include(wc => wc.WorkCentre)
            .Include(ps => ps.ProductionStage)
            .Include(fz => fz.FactoryZone)
            .Include(wo => wo.WorksOrder)
                .ThenInclude(w => w.Customer)
            .Include(ei => ei.EquipmentItem)
            .ToListAsync();

            List<ProductionPlanDto> wipPlannedTodayDtos = [];
            foreach (var wo in wipPlannedToday)
            {
                wipPlannedTodayDtos.Add(new ProductionPlanDto
                {
                    ProductionPlanItemID = wo.ProductionPlanItemID,
                    WorkOrderNo = wo.WorkOrderNo,
                    DivisionID = wo.DivisionID,
                    ClientName = wo.WorksOrder != null && wo.WorksOrder.Customer != null ? wo.WorksOrder.Customer.ClientName : null,
                    WODescription = wo.WorksOrder != null ? wo.WorksOrder.Description : null,
                    StepNo = wo.StepNo,
                    ProductionStageID = wo.ProductionStageID,
                    ProductionStageDescription = wo.ProductionStage != null ? wo.ProductionStage.ProductionStageName : null,
                    EquipmentItemID = wo.EquipmentItemID,
                    EquipmentItemDescription = wo.EquipmentItem != null ? wo.EquipmentItem.EquipmentItemDescription : null,
                    ZoneId = wo.ZoneId,
                    FactoryZoneName = wo.FactoryZone != null ? wo.FactoryZone.Name : null,
                    WorkCentreID = wo.WorkCentreID,
                    WorkCentreName = wo.WorkCentre != null ? wo.WorkCentre.WorkCentreName : null,
                    PlannedStartTime = wo.PlannedStartTime,
                    PlannedEndTime = wo.PlannedEndTime,
                    ActualStartTime = wo.ActualStartTime,
                    ActualEndTime = wo.ActualEndTime,
                    ClosedByUserID = wo.ClosedByUserID,
                    Status = wo.Status,
                    IsPulsePlan = wo.IsPulsePlan,
                    WoAtStep = wo.WorksOrder != null && wo.WorksOrder.ProductionStage != null ? wo.WorksOrder.ProductionStage.StepNo : 0,
                    RequiredDate = wo.WorksOrder != null ? wo.WorksOrder.RequiredDate : null
                });
            };

        if (wipPlannedTodayDtos == null)
        {
            return Results.NotFound(ApiResponse<List<ProductionPlanDto>>.ErrorResponse("No planned work items found for today.", statusCode: StatusCodes.Status404NotFound));
        }
        return Results.Ok(ApiResponse<List<ProductionPlanDto>>.SuccessResponse(wipPlannedTodayDtos));
    }

#region Production Plan Stats
    private static async Task<IResult> GetWipWorkCentreStats(string divisionID, PulseDbContext dbPulse)
    {
        //var wipPlannedToday = await dbPulse.ProductionPlanItems
        //    .Where(wo => wo.DivisionID == divisionID && wo.PlannedStartTime.HasValue && wo.PlannedStartTime.Value.Date == DateTime.Today)
        //    .ToListAsync();

        var wipWC = await dbPulse.WorkCentreMaster
            .Where(wc => wc.DivisionID == divisionID)
            .Select(wc => new WorkCentreWipStatsDto
            {
                WorkCentreID = wc.WorkCentreId,
                WorkCentreName = wc.WorkCentreName,
                TotalWOCount = GetWipCountForWorkCentre(wc.WorkCentreId, dbPulse),
                PlannedTodayCount = GetTodaysPlannedWipCountForWorkCentre(wc.WorkCentreId, dbPulse),
                CompletedTodayCount = GetTodaysCompletedWipCountForWorkCentre(wc.WorkCentreId, dbPulse),
                InProgress = GetInProgressWipCountForWorkCentre(wc.WorkCentreId, dbPulse)
            })
            .ToListAsync();

        if (wipWC == null || !wipWC.Any())
        {
            return Results.NotFound(ApiResponse<List<WorkCentreWipStatsDto>>.ErrorResponse("No work centre stats found.", statusCode: StatusCodes.Status404NotFound));
        }
        return Results.Ok(ApiResponse<List<WorkCentreWipStatsDto>>.SuccessResponse(wipWC));
    }
    private static int GetWipCountForWorkCentre(int workCentreID, PulseDbContext dbPulse)
    {
        return dbPulse.WorksOrder
            .Include(ps => ps.ProductionStage)
            .Where(wo => wo.ProductionStage != null && wo.ProductionStage.WorkCentreID == workCentreID && wo.UndelQty > 0)
            .Count();
    }
    private static int GetInProgressWipCountForWorkCentre(int workCentreID, PulseDbContext dbPulse)
    {
        return dbPulse.ProductionPlanItems
            .Where(wo => wo.WorkCentreID == workCentreID && wo.ActualStartTime.HasValue && !wo.ActualEndTime.HasValue)
            .Count();
    }
    private static int GetTodaysCompletedWipCountForWorkCentre(int workCentreID, PulseDbContext dbPulse)
    {
        return dbPulse.ProductionPlanItems
            .Where(wo => wo.WorkCentreID == workCentreID && wo.ActualEndTime.HasValue && wo.ActualEndTime.Value.Date == DateTime.Today)
            .Count();
    }
    private static int GetTodaysPlannedWipCountForWorkCentre(int workCentreID, PulseDbContext dbPulse)
    {
        return dbPulse.ProductionPlanItems
            .Where(wo => wo.WorkCentreID == workCentreID && wo.PlannedStartTime.HasValue && wo.PlannedStartTime.Value.Date == DateTime.Today)
            .Count();
    }

#endregion
    private static async Task<IResult> GetWipPlanResources(string divisionID, PulseDbContext dbPulse)
    {
        var WCresources = await dbPulse.WorkCentreMaster
            .Where(wc => wc.DivisionID == divisionID)
            .Include(z => z.FactoryZones!)
                .ThenInclude(e => e.EquipmentItems!)
            .ToListAsync();

        List<ProductionPlanResource> resources = [];
        foreach(var wc in WCresources)
        {
            resources.Add(new ProductionPlanResource
            {
                id = wc.WorkCentreId.ToString(),
                title = wc.WorkCentreName
            });
            foreach (var zone in wc.FactoryZones!)
            {
                resources.Add(new ProductionPlanResource
                {
                    id = $"{wc.WorkCentreId.ToString()}_{zone.Id.ToString()}".ToUpper(),
                    title = zone.Name,
                    parentId = wc.WorkCentreId.ToString()
                });
                foreach (var eq in zone.EquipmentItems!)
                {
                    resources.Add(new ProductionPlanResource
                    {
                        id = $"{wc.WorkCentreId}_{zone.Id.ToString()}_{eq.EquipmentItemID}".ToUpper(),
                        title = eq.EquipmentItemDescription,
                        parentId = $"{wc.WorkCentreId.ToString()}_{zone.Id.ToString()}".ToUpper()
                    });
                }
            }
        }
        return Results.Ok(ApiResponse<List<ProductionPlanResource>>.SuccessResponse(resources));

    }

#region Production Plan Events

    private static async Task<IResult> CompleteProductionStep(
        int StepID, 
        CompleteProductionPlanDto CompletePlanDto, 
        IHubContext<MessageHub> hubContext,
        PulseDbContext dbPulse)
    {
        var planItem = await dbPulse.ProductionPlanItems
            .Where(pp => pp.ProductionPlanItemID == StepID)
            .Include(ppps => ppps.ProductionStage)
            .FirstAsync();

        if(planItem == null) { return Results.NotFound(ApiResponse<string>.ErrorResponse("Production plan item not found.", statusCode: StatusCodes.Status404NotFound)); }

        planItem.ActualEndTime = CompletePlanDto.ActualEndTime;
        planItem.ClosedByUserID = CompletePlanDto.ClosedByUserID;
        planItem.Status = CompletePlanDto.Status;

        var nextStep = await dbPulse.ProductionPlanItems
            .Where(pp => pp.WorkOrderNo == planItem.WorkOrderNo && pp.StepNo == planItem.StepNo + 1)
            .Include(ps => ps.ProductionStage)
            .FirstOrDefaultAsync();

        var wo = await dbPulse.WorksOrder
        .Where(wo => wo.WorksOrderNo == planItem.WorkOrderNo)
        .FirstAsync();
        if (wo != null)
        {
            wo.ProductionStageID = nextStep != null ? nextStep.ProductionStageID : wo.ProductionStageID;
            wo.ProductionStageName = nextStep?.ProductionStage?.ProductionStageName ?? wo.ProductionStageName;
            wo.ProgressChange = DateTime.Now;
            //wo.ProgressComment = $"{wo.ProgressComment} ; Pulse Aspire Completed step {planItem.StepNo} - {(planItem.ProductionStage != null ? planItem.ProductionStage.ProductionStageName : "Unknown Stage")}";
        }

        await dbPulse.SaveChangesAsync();

        ProductionStreamMessageDto productionStreamMessageDto = new ProductionStreamMessageDto
        {
            ProductionGroupName = $"0_1_{CompletePlanDto.DivisionID}",
            StreamType = ProductionStreamType.StageCompleted,
            WorkOrderNo = planItem.WorkOrderNo,
            StepNo = planItem.StepNo,
            Status = planItem.Status,
            MsgTimestamp = DateTime.Now,
            MsgActionDateTime = planItem.ActualEndTime ?? DateTime.Now,
            UserName = CompletePlanDto.UserName,
            Result = $"Work Order {planItem.WorkOrderNo} - Step {planItem.StepNo} marked as completed.",
            ProductionStageName = planItem.ProductionStage != null ? planItem.ProductionStage.ProductionStageName : null,
            Title = $"WO No:{planItem.WorkOrderNo} - Step {planItem.StepNo} - {(planItem.ProductionStage != null ? planItem.ProductionStage.ProductionStageName : null)} Completed"
        };

        await hubContext.Clients.Group($"0_1_{CompletePlanDto.DivisionID}")
                .SendAsync("ProductionStreamBroadcast", productionStreamMessageDto);

        return Results.Ok(ApiResponse<string>.SuccessResponse("Production plan item completed successfully."));
    }

    #endregion
}
