using Microsoft.EntityFrameworkCore;
using Pulse.Models.CustomComponents;
using Pulse.Models.Customers;
using Pulse.Models.Production;
using Pulse.Models.PulseContext;

namespace Pulse.ApiService.Endpoints
{
    internal static class CustomerEndpoints
    {
        internal const string BasePath = "/Customers";
        internal const string ByIdPath = "/Details/{fullclientid}";
        internal const string SalesByIdPath = "/Details/{fullclientid}/Sales";

        public static void MapCustomerEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup(BasePath).WithTags("Customers");
            
            group.MapGet("/GetAll", async (PulseDbContext db) =>
                await db.ClientMaster.ToListAsync())
                .WithName("GetAllCustomers")
                .Produces<List<Customer>> (StatusCodes.Status200OK);

            group.MapGet(ByIdPath, async (string fullclientid, PulseDbContext db) =>
            await db.ClientMaster.AsNoTracking()
                .FirstOrDefaultAsync(c => c.FullClientID == fullclientid)
                is { } c
                    ? Results.Ok(c)
                    : Results.NotFound())
                .WithName("GetCustomerById")
                .Produces<Customer>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(SalesByIdPath, async (string fullclientid, PulseDbContext dbContext) =>
            {
                var sales = await dbContext.ClientSales
                    .Where(s => s.FullClientID == fullclientid)
                    //.Include(cs => cs.Customer)
                    .Include(cs => cs.Period)
                    .Select(cs => new
                    {
                        cs.Id,
                        cs.FullClientID,
                        //CustomerName = cs.Customer.ClientName,
                        cs.PeriodID,
                        Period = new
                        {
                            cs.Period.PeriodID,
                            cs.Period.Month,
                            cs.Period.CalendarYear,
                            cs.Period.FinancialYear,
                            cs.Period.StartDate
                        },
                        cs.CompanyID,
                        cs.Amount
                    })
                    .ToListAsync();
                return Results.Ok(sales);
            })
                .WithName("GetCustomerSalesById")
                .Produces<object[]>(200);

            group.MapGet(ByIdPath + "/RollerSpecifications", async (string fullclientid, PulseDbContext dbContext) =>
            {
                var specs = await dbContext.ClientRollerSpecificationMaster
                    .Where(s => s.FullClientID == fullclientid)
                    //.Include(cs => cs.Customer)
                    .ToListAsync();
                return Results.Ok(specs);
            })
                .WithName("GetCustomerRollerSpecificationsById")
                .Produces<List<ClientRollerSpecification>>(200);

            group.MapGet("/RollerSpecifications/GetWOByRollerID/{rollId}", async (int rollId, PulseDbContext db) =>
            {
                var wO = await db.WorksOrder
                        .Where(w => w.ClientRollerID == rollId)
                        .Include(p => p.Period)
                        .Include(wt => wt.WorkType)
                        .ToListAsync();
                return Results.Ok(wO);
            })
                .WithName("GetWObyRollerId")
                .Produces<List<WorksOrder>>(200);

            group.MapGet("/RollerSpecifications/GetWOBySpecID/{specId}", async (int specId, PulseDbContext db) =>
            {
                var wO = await db.WorksOrder
                        .Where(w => w.ClientRollerSpecificationID == specId)
                        .Include(p => p.Period)
                        .Include(wt => wt.WorkType)
                        .ToListAsync();
                return Results.Ok(wO);
            })
                .WithName("GetRollerWObyId")
                .Produces<List<WorksOrder>>(200);

            group.MapGet("/RollerSpecifications/AllDetailsBySpecID/{specId}", async (int specId, PulseDbContext db) =>
            {
                // 1. Verify the spec exists
                var specExists = await db.ClientRollerSpecificationMaster
                                         .AnyAsync(s => s.ClientRollerSpecificationID == specId);
                if (!specExists) return Results.NotFound($"Specification {specId} not found.");

                // 2. Load ONLY the rollers that belong to that spec
                var rollers = await db.ClientRollerMaster
                    .AsNoTracking()
                    .Where(r => r.ClientRollerSpecificationID == specId && r.IsActive)

                    // ---- EAGER LOAD EVERYTHING ON WorksOrder ----
                    .Include(r => r.WorksOrders!)
                        .ThenInclude(wo => wo.Customer!)
                    .Include(r => r.WorksOrders!)
                        .ThenInclude(wo => wo.Period!)
                    .Include(r => r.WorksOrders!)
                        .ThenInclude(wo => wo.WorkType!)
                    .Include(r => r.WorksOrders!)
                        .ThenInclude(wo => wo.Division!)
                    .Include(r => r.WorksOrders!)
                        .ThenInclude(wo => wo.ClientRollerSpecification!)
                    .Include(r => r.WorksOrders!)
                        .ThenInclude(wo => wo.ClientRoller!)
                    .Include(r => r.WorksOrders!)
                        .ThenInclude(wo => wo.Compound!)

                    // ---- PROJECT TO DTO ----
                    .Select(r => new ClientRollerDto(
                        r.ClientRollerID,
                        r.ClientRollerSpecificationID,
                        r.ClientRollerNumber,
                        r.ShellDiameter,
                        r.ShellLength,
                        r.ShellWeight,
                        r.CoverDiameter,
                        r.ShellDefects,
                        r.IsActive,
                        r.WorksOrders!.Select(wo => new WorksOrderDto(
                            wo.WorksOrderNo,
                            wo.FullClientID,
                            wo.PeriodID,
                            wo.DateStarted,
                            wo.Description,
                            wo.WorkTypeID,
                            wo.Quantity,
                            wo.DivisionID,
                            wo.ClientRollerSpecificationID,
                            wo.ClientRollerID,
                            wo.ClientRollNo,
                            wo.CoverCompoundCode,
                            wo.ClientOrderNo,
                            wo.ClientPRNo,
                            wo.ClientRFQNo,
                            wo.ShellLength,
                            wo.ShellDiameter,
                            wo.CoverDiameter,
                            wo.UndelQty,
                            wo.MaterialCost,
                            wo.SellPrice,
                            wo.Status,
                            wo.Customer != null ? new CustomerDto(wo.Customer.FullClientID, wo.Customer.ClientName, wo.Customer.TaxCodeID,wo.Customer.FullChargeClientID,wo.Customer.CompanyID,wo.Customer.ClientID) : null,
                            wo.Period != null ? new PeriodDto(wo.Period.PeriodID, wo.Period.Month,wo.Period.CalendarYear,wo.Period.FinancialYear, wo.Period.StartDate) : null,
                            wo.WorkType != null ? new WorkTypeDto(wo.WorkType.WorkTypeID, wo.WorkType.WorkTypeName) : null,
                            wo.Division != null ? new DivisionDto(wo.Division.DivisionID, wo.Division.DivisionName,wo.Division.CompanyID,wo.Division.BranchID) : null,
                            wo.ClientRollerSpecification != null
                                ? new ClientRollerSpecificationDto(
                                    wo.ClientRollerSpecification.ClientRollerSpecificationID,
                                    wo.ClientRollerSpecification.FullClientID)
                                : null,
                            wo.ClientRoller != null
                                ? new ClientRollerDto(
                                    wo.ClientRoller.ClientRollerID,
                                    wo.ClientRoller.ClientRollerSpecificationID,
                                    wo.ClientRoller.ClientRollerNumber,
                                    wo.ClientRoller.ShellDiameter,
                                    wo.ClientRoller.ShellLength,
                                    wo.ClientRoller.ShellWeight,
                                    wo.ClientRoller.CoverDiameter,
                                    wo.ClientRoller.ShellDefects,
                                    wo.ClientRoller.IsActive,
                                    new List<WorksOrderDto>())   // minimal self-reference
                                : null,
                            wo.Compound != null ? new CompoundDto(wo.Compound.CompoundCode, wo.Compound.CompoundDescription,wo.Compound.CompoundType) : null
                        )).ToList()
                    ))
                    .ToListAsync();

                // 3. Return the collection directly (no wrapper)
                return Results.Ok(rollers);
            })
                .WithName("GetRollersForSpecification");

            group.MapGet(ByIdPath + "/CurrentStats", async (string fullclientid, PulseDbContext db) =>
            {
                var openWOCount = await db.WorksOrder
                    .Where(wo => wo.FullClientID == fullclientid && wo.UndelQty > 0)
                    .CountAsync();

                var openQuoteCount = 0;
                var isset = true;

                var stats = new ClientCurrentStats {
                    FullClientID = fullclientid,
                    isSet = isset,
                    countWipWOs = openWOCount,
                    countOpenQuotes = openQuoteCount
                };
                return Results.Ok(stats);
            })
                .WithName("GetClientStats");
        }
    }
}
