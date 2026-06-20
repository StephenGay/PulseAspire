//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using Pulse.Models.PulseContext;

//namespace Pulse.ApiService.Hubs;

///// <summary>
///// Shared logic for joining multi-tenant groups.
///// Call this from every hub's OnConnectedAsync.
///// </summary>
//public static class GroupManager
//{
//    public static async Task ConnectToUserSystemStreams(
//        HubCallerContext context,
//        IDbContextFactory<PulseDbContext> dbFactory,
//        ILogger logger)
//    {
//        var httpContext = context.GetHttpContext();
//        var connectionId = context.ConnectionId;

//        // Prefer claims from authenticated user (JWT / cookie)
//        //var branchId = context.User?.FindFirst("branchId")?.Value
//        //                 ?? httpContext?.Request.Query["branchId"].FirstOrDefault();

//        //var companyId = context.User?.FindFirst("companyId")?.Value;
//        //var divisionId = context.User?.FindFirst("divisionId")?.Value;
//        var userId = context.User?.FindFirst("sub")?.Value
//                         ?? context.User?.Identity?.Name;

//        //if (!string.IsNullOrEmpty(branchId))
//        //    await groups.AddToGroupAsync(connectionId, $"branch-{branchId}");

//        //if (!string.IsNullOrEmpty(companyId))
//        //    await groups.AddToGroupAsync(connectionId, $"company-{companyId}");

//        //if (!string.IsNullOrEmpty(divisionId))
//        //    await groups.AddToGroupAsync(connectionId, $"division-{divisionId}");

//        if (!string.IsNullOrEmpty(userId))
//            await groups.AddToGroupAsync(connectionId, $"user-{userId}");

//        logger.LogInformation("Client {ConnectionId} joined groups (Branch: {Branch}, Company: {Company}, User: {User})",
//            connectionId, branchId, companyId, userId);
//    }
//    public static async Task JoinStandardGroups(
//        HubCallerContext context,
//        IGroupManager groups,
//        ILogger logger)
//    {
//        var httpContext = context.GetHttpContext();
//        var connectionId = context.ConnectionId;

//        // Prefer claims from authenticated user (JWT / cookie)
//        var branchId = context.User?.FindFirst("branchId")?.Value
//                         ?? httpContext?.Request.Query["branchId"].FirstOrDefault();

//        var companyId = context.User?.FindFirst("companyId")?.Value;
//        var divisionId = context.User?.FindFirst("divisionId")?.Value;
//        var userId = context.User?.FindFirst("sub")?.Value
//                         ?? context.User?.Identity?.Name;

//        if (!string.IsNullOrEmpty(branchId))
//            await groups.AddToGroupAsync(connectionId, $"branch-{branchId}");

//        if (!string.IsNullOrEmpty(companyId))
//            await groups.AddToGroupAsync(connectionId, $"company-{companyId}");

//        if (!string.IsNullOrEmpty(divisionId))
//            await groups.AddToGroupAsync(connectionId, $"division-{divisionId}");

//        if (!string.IsNullOrEmpty(userId))
//            await groups.AddToGroupAsync(connectionId, $"user-{userId}");

//        logger.LogInformation("Client {ConnectionId} joined groups (Branch: {Branch}, Company: {Company}, User: {User})",
//            connectionId, branchId, companyId, userId);
//    }
//}