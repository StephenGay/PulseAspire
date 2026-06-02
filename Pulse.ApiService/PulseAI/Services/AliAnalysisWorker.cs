using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Pulse.ApiService.Hubs;
using Pulse.ApiService.PulseAI.Characters;
using Pulse.Models.AI;
using Pulse.Models.AI.Ali;
using Pulse.Models.Communication;
using Pulse.Models.Customers;
using Pulse.Models.Production;
using Pulse.Models.PulseContext;
using Pulse.Models.Users;
using System.Text.Json;

namespace Pulse.ApiService.PulseAI.Services;

public class AliAnalysisWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AliAnalysisWorker> _logger;

    // Removed direct injection of AliAPI and MessageHub
    public AliAnalysisWorker(IServiceScopeFactory scopeFactory, ILogger<AliAnalysisWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //var aliModel = builder.Configuration["OllamaApi:AliModel"] ?? "neural-chat";
        while (!stoppingToken.IsCancellationRequested)
        {
            // Create a fresh scope for EVERY iteration → this gives us safe access to Scoped services
            await using var scope = _scopeFactory.CreateAsyncScope();

            var db = scope.ServiceProvider.GetRequiredService<PulseDbContext>();
            var aliApi = scope.ServiceProvider.GetRequiredService<AliOllamaAPI>();        // ← now safe
            
            var presenceService = scope.ServiceProvider.GetRequiredService<PresenceService>();  // ← Added
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<MessageHub>>(); // ← now safe

            var next = await db.AnalysisRequests
                .Where(r => r.Status == "Queued")
                .OrderBy(r => r.QueuedAt)
                .FirstOrDefaultAsync(stoppingToken);

            if (next is null)
            {
                await Task.Delay(30000, stoppingToken);
                continue;
            }

            next.Status = "Processing";
            await db.SaveChangesAsync(stoppingToken);

            var pulseMessage = new PulseMessage
            {
                SenderUserName = "Ali",
                RecipientUserId = next.ApplicationUserId,
                Role = "PulseAI",
                Subject = next.AnalysisTitle ?? "Analysis By Ali",
                ContentType = "HTML"
            };

            try
            {
                var reqDto = new AnalysisRequestDto(
                    next.KeyValue,
                    next.ApplicationUserId,
                    next.ContextualAreaID,
                    next.UserQuery,
                    next.AnalysisTitle);

                // 1. Load contextual area
                var context = await LoadContextAsync(db, next.ContextualAreaID);
                if (context is null)
                    throw new InvalidOperationException($"ContextArea not found: {next.ContextualAreaID}");

                // 2. Load the real object Ali needs
                var entity = await LoadEntityAsync(db, context.TargetEntityType, next.KeyValue);

                // 3. Hand it to Ali (now inside the correct scope)
                var result = await aliApi.AnalyseWithAliAsync(context, entity, next.UserQuery);

                if (!result.Success)
                {
                    throw new InvalidOperationException($"Ali analysis failed: {result.Message}");
                }

                next.ResultSummary = result.Data;
                next.Status = "Completed";
                pulseMessage.Subject = "Completed: " + pulseMessage.Subject;
                pulseMessage.Content = result.Data;

                

                
            }
            catch (Exception ex)
            {
                next.Status = "Failed";
                next.ErrorMessage = ex.Message;
                pulseMessage.Subject = "Error: " + pulseMessage.Subject;
                pulseMessage.Content = $"<p>Ali analysis failed: {ex.Message}</p>";
                _logger.LogError(ex, "Ali analysis failed for request {RequestId}", next.Id);
            }

            await db.SaveChangesAsync(stoppingToken);

            // === Lookup RecipientUserName if missing (matches your hub logic) ===
            if (string.IsNullOrEmpty(pulseMessage.RecipientUserName))
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByIdAsync(next.ApplicationUserId.ToString());
                pulseMessage.RecipientUserName = user?.FullName ?? "Unknown Recipient";
            }
            // === SAVE TO DATABASE FIRST (critical - matches hub) ===
            db.PulseMessages.Add(pulseMessage);
            await db.SaveChangesAsync(stoppingToken);

            // === SEND USING PRESENCE SERVICE (exact match to your SendPrivateMessage) ===
            var recipientConnectionIds = presenceService.GetUserConnectionIds(next.ApplicationUserId);

            if (recipientConnectionIds.Any())
            {
                await hubContext.Clients.Clients(recipientConnectionIds.ToList())
                                .SendAsync("ReceiveMessage", pulseMessage);

                _logger.LogInformation("Ali private message sent to user {UserId} ({Count} connections) for request {RequestId}",
                    next.ApplicationUserId, recipientConnectionIds.Count(), next.Id);
            }
            else
            {
                _logger.LogWarning("Recipient {UserId} not online. Message saved to DB but not delivered live. RequestId: {RequestId}",
                    next.ApplicationUserId, next.Id);
            }
        }
    }

    private async Task<ContextualArea?> LoadContextAsync(PulseDbContext db, int contextAreaId)
    {
        return await db.ContextualAreaMaster
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ContextAreaId == contextAreaId);
    }

    private async Task<object?> LoadEntityAsync(PulseDbContext db, string entityType, string keyValue)
    {
        switch (entityType)
        {
            case "ClientSales":
                var sales = await db.ClientSales
                    .AsNoTracking()
                    .Where(c => c.FullClientID == keyValue)
                    .ToListAsync();

                if (sales == null || !sales.Any())
                    throw new InvalidOperationException($"No ClientSales found for key: {keyValue}");
                return sales;

            case "ClientRollerSpecification":
                var spec = await db.ClientRollerSpecificationMaster
                    .AsNoTracking()
                    .Where(c => c.ClientRollerSpecificationID == Convert.ToInt32(keyValue))
                    .FirstOrDefaultAsync();

                if (spec == null)
                    throw new InvalidOperationException($"No ClientRollerSpecification found for key: {keyValue}");

                var rollers = await db.ClientRollerMaster
                    .AsNoTracking()
                    .Where(r => r.ClientRollerSpecificationID == spec.ClientRollerSpecificationID)
                    .ToListAsync();

                foreach(var roll in rollers)
                {
                    var wo = await db.WorksOrder
                        .AsNoTracking()
                        .Where(w => w.ClientRollerID == roll.ClientRollerID)
                        .Include(p => p.Period)
                        .Include(wt => wt.WorkType)
                        .ToListAsync();
                    if(wo != null) roll.WorksOrders = wo;
                }

                if(rollers != null) spec.ClientRollers = rollers;

                return spec;

            case "WorkInProgressDto":
                const string sql = @"
                    SELECT 
                        wo.WorksOrderNo, wo.DivisionID, wo.WorkTypeID, wtm.WorkTypeName, 
                        wtm.TargetWorkingDays, cm.ClientName, wo.UndelQty, wo.MaterialCost,
                        wo.SellPrice, wo.ProductionStageID, wo.ProductionStageName, 
                        wo.ProgressChange, wo.RequiredDate, wcm.WorkCentreName,
                        wo.DateStarted, wo.Description, wcfm.WorkCentreID
                    FROM dbo.WorkCentreMaster wcm 
                    FULL OUTER JOIN dbo.ClientMaster cm 
                    RIGHT OUTER JOIN dbo.WorksOrder wo ON cm.FullClientID = wo.FullClientID 
                    LEFT OUTER JOIN dbo.WorkTypeMaster wtm ON wo.WorkTypeID = wtm.WorkTypeID 
                    LEFT OUTER JOIN dbo.WorkCentreFunctionsMapping wcfm ON wo.ProductionStageID = wcfm.ProductionStageID 
                    ON wcm.WorkCentreId = wcfm.WorkCentreID
                    WHERE (wo.UndelQty > 0) AND (wo.DivisionID = @divisionId)";

                var parameter = new SqlParameter("@divisionId", keyValue);

                var wipItems = await db.Database.SqlQueryRaw<WorkInProgressDto>(sql, parameter).ToListAsync();

                if (!wipItems.Any())
                    throw new InvalidOperationException($"No WorkInProgress items found for DivisionID: {keyValue}");

                return wipItems;

            default:
                throw new InvalidOperationException($"Unknown entity type: {entityType}");
        }
    }
}



//using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
//using Pulse.ApiService.Hubs;
//using Pulse.ApiService.PulseAI.Characters;
//using Pulse.Models.AI;
//using Pulse.Models.AI.Ali;
//using Pulse.Models.Communication;
//using Pulse.Models.Customers;
//using Pulse.Models.Production;
//using Pulse.Models.PulseContext;
//using System.Text.Json;

//namespace Pulse.ApiService.PulseAI.Services;

//// Pulse.ApiService/Background/AliAnalysisWorker.cs
//public class AliAnalysisWorker : BackgroundService
//{
//    private readonly IServiceScopeFactory _scopeFactory;
//    private readonly ILogger<AliAnalysisWorker> _logger;
//    private MessageHub _messageHub; // Injected via scope
//    private readonly AliAPI _aliApi;

//    public AliAnalysisWorker(IServiceScopeFactory scopeFactory, ILogger<AliAnalysisWorker> logger, AliAPI aliApi)
//    {
//        _scopeFactory = scopeFactory;
//        _logger = logger;
//        _aliApi = aliApi;
//    }

//    protected override async Task ExecuteAsync( CancellationToken stoppingToken)
//    {
//        while (!stoppingToken.IsCancellationRequested)
//        {
//            await using var scope = _scopeFactory.CreateAsyncScope();
//            var db = scope.ServiceProvider.GetRequiredService<PulseDbContext>();

//            var next = await db.AnalysisRequests
//                .Where(r => r.Status == "Queued")
//                .OrderBy(r => r.QueuedAt)
//                .FirstOrDefaultAsync(stoppingToken);

//            if (next is null)
//            {
//                await Task.Delay(5000, stoppingToken); // poll every 5 s
//                continue;
//            }

//            next.Status = "Processing";
//            await db.SaveChangesAsync(stoppingToken);

//            try
//            {
//                var reqDto = new AnalysisRequestDto
//                (
//                    next.KeyValue,
//                    next.ApplicationUserId,
//                    next.ContextualAreaID,
//                    next.UserQuery
//                );

//                // 1. Load the contextual area to understand what Ali needs to analyze
//                var context = await LoadContextAsync(db, next.ContextualAreaID);
//                if(context is null)
//                    throw new InvalidOperationException($"ContextArea not found: {next.ContextualAreaID}");

//                // 2. Load the real object Ali needs
//                var entity = await LoadEntityAsync(db, context.TargetEntityType, next.KeyValue);

//                // 3. Hand it to your Ali character (Ollama call)
//                var result = await _aliApi.AnalyseWithAliAsync(context, entity, next.UserQuery);
//                if(!result.Success)
//                {
//                    throw new InvalidOperationException($"Ali analysis failed: {result.Message}");
//                }
//                PulseMessage pulseMessage = new PulseMessage
//                {
//                    SenderUserName = "Ali",
//                    RecipientUserId = next.ApplicationUserId,
//                    Role = "PulseAI",
//                    Subject = $"{next.AnalysisTitle}",
//                    ContentType = "HTML",
//                    Content = result.Data
//                };
//                await _messageHub.SendPrivateMessage(pulseMessage);

//                    next.ResultSummary = result.Data;
//                    next.Status = "Completed";

//                //next.LoadedObjectJson = JsonSerializer.Serialize(entity); // optional
//            }
//            catch (Exception ex)
//            {
//                next.Status = "Failed";
//                next.ErrorMessage = ex.Message;
//                _logger.LogError(ex, "Ali analysis failed");
//            }

//            await db.SaveChangesAsync(stoppingToken);
//        }
//    }

//    // Simple generic loader (expand with a switch if you need more complex queries)
//    private async Task<ContextualArea?> LoadContextAsync(PulseDbContext db, int contextAreaId)
//    {
//        return await db.ContextualAreaMaster
//            .AsNoTracking()
//            .FirstOrDefaultAsync(c => c.ContextAreaId == contextAreaId);
//    }
//    // Simple generic loader (expand with a switch if you need more complex queries)

//    private async Task<object?> LoadEntityAsync(PulseDbContext db, string entityType, string keyValue)
//    {
//        switch(entityType)
//        {
//            case "ClientSales":
//                var sales = await db.ClientSales
//                    .AsNoTracking()
//                    .Where(c => c.FullClientID == keyValue)
//                    .ToListAsync();

//                if(sales == null || sales.Count == 0)
//                    throw new InvalidOperationException($"No ClientSales found for key: {keyValue}");
//                return sales;

//            case "ClientRollerSpecification":
//                var spec = await db.ClientRollerSpecificationMaster
//                    .AsNoTracking()
//                    .Where(c => c.ClientRollerSpecificationID == Convert.ToInt32(keyValue))
//                    .FirstOrDefaultAsync();

//                if(spec == null)
//                    throw new InvalidOperationException($"No ClientRollerSpecification found for key: {keyValue}");
//                return spec;

//            case "WorkInProgressDto":
//                const string sql = @"
//                        SELECT 
//                            wo.WorksOrderNo, 
//                            wo.DivisionID,
//                            wo.WorkTypeID,
//                            wtm.WorkTypeName, 
//                            wtm.TargetWorkingDays, 
//                            cm.ClientName, 
//                            wo.UndelQty, 
//                            wo.MaterialCost,
//                            wo.SellPrice, 
//                            wo.ProductionStageID, 
//                            wo.ProductionStageName, 
//                            wo.ProgressChange, 
//                            wo.RequiredDate, 
//                            wcm.WorkCentreName,
//                            wo.DateStarted, 
//                            wo.Description, 
//                            wcfm.WorkCentreID
//                        FROM dbo.WorkCentreMaster wcm 
//                        FULL OUTER JOIN dbo.ClientMaster cm 
//                        RIGHT OUTER JOIN dbo.WorksOrder wo ON cm.FullClientID = wo.FullClientID 
//                        LEFT OUTER JOIN dbo.WorkTypeMaster wtm ON wo.WorkTypeID = wtm.WorkTypeID 
//                        LEFT OUTER JOIN dbo.WorkCentreFunctionsMapping wcfm ON wo.ProductionStageID = wcfm.ProductionStageID 
//                        ON wcm.WorkCentreId = wcfm.WorkCentreID
//                        WHERE (wo.UndelQty > 0) AND (wo.DivisionID = @divisionId)
//                    ";

//                var parameter = new SqlParameter("@divisionId", keyValue);

//                var wipItems = await db.Database.SqlQueryRaw<WorkInProgressDto>(sql, parameter).ToListAsync();

//                if (!wipItems.Any())
//                    throw new InvalidOperationException($"No WorkInProgress items found for DivisionID: {keyValue}");

//                return wipItems;

//                default:
//                 throw new InvalidOperationException($"Unknown entity type: {entityType}");
//                //.Set<Customer>().FirstOrDefaultAsync(c => EF.Property<string>(c, keyProp) == keyValue),
//                //"Policy" => await db.Set<Policy>().FirstOrDefaultAsync(p => EF.Property<string>(p, keyProp) == keyValue),
//                // add more as you grow
//        };
//    }

//    private async Task<string> RunAliAnalysisAsync(ContextualArea context, object entity)
//    {
//        // Your existing Ollama call here – pass the loaded entity + analysisType
//        // e.g. build a prompt like "Perform a Client Sales analysis on this Customer object: {JsonSerializer.Serialize(entity)}"
//        // return the text response from Ali
//        return "Analysis result from Ali...";
//    }
//}