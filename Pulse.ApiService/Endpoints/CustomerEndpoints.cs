using Microsoft.EntityFrameworkCore;
using Pulse.Models.Api;
using Pulse.Models.CustomComponents;
using Pulse.Models.Customers;
using Pulse.Models.Production;
using Pulse.Models.PulseContext;
using Pulse.Models.Rollers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.WebRequestMethods;

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

            #region GET Functions
            group.MapGet("/GetAll", async (PulseDbContext db) =>
                await db.ClientMaster.ToListAsync())
                .WithName("GetAllCustomers")
                .Produces<List<Customer>> (StatusCodes.Status200OK);

            group.MapGet(ByIdPath, async (string fullclientid, PulseDbContext db) =>
            {
                var cust = await db.ClientMaster
                    .AsNoTracking()
                    .Where(x => x.FullClientID == fullclientid)
                    .Include(x => x.Industry!)
                    .Include(x => x.Region!)
                    .ThenInclude(r => r.Province!)
                    .Include(x => x.SalesRepresentative!)
                    .FirstOrDefaultAsync();
                return Results.Ok(cust);
            })
                .WithName("GetClientByID")
                .Produces<Customer> (StatusCodes.Status200OK);

            group.MapGet(ByIdPath + "/Contacts/GetAll", async (string fullclientid, PulseDbContext db) =>
            {
                var contacts = await db.ClientContactMaster
                    .AsNoTracking()
                    .Where(c => c.FullClientID == fullclientid)
                    .ToListAsync();
                if (contacts == null) { contacts = new List<ClientContact>(); }
                return Results.Ok(contacts);
            })
                .WithName("GetContactsByClientId")
                .Produces<List<ClientContact>>(StatusCodes.Status200OK);

            group.MapGet("/Details/{fullclientid}/Budgets/{FinancialYear}", async (string fullclientid, string FinancialYear, PulseDbContext db) =>
            {
                var budgets = await db.ClientBudgetMaster
                    .AsNoTracking()
                    .Where(b => b.FullClientId == fullclientid && b.FinancialYear == FinancialYear)
                    .Include(b => b.period)
                    .ToListAsync();
                if (!budgets.Any()) { budgets = new List<ClientBudgets>(); }

                return Results.Ok(budgets);
            })
                .WithName("GetClientBudget")
                .Produces<List<ClientBudgets>> (StatusCodes.Status200OK);

            group.MapGet("/Details/{fullclientid}/Budgets/{FinancialYear}/CreateBlank", async (string fullclientid, string FinancialYear, PulseDbContext db) =>
            {
                var per = await db.PeriodMaster
                    .AsNoTracking()
                    .Where(p => p.FinancialYear == FinancialYear)
                    .Select(p => new BlankClientBudget
                    {
                        FullClientId = fullclientid,
                        PeriodID = p.PeriodID,
                        Month = p.Month,
                        CalenderYear = p.CalendarYear,
                        FinancialYear = p.FinancialYear,
                        BudgetedSales = 0
                    })
                    .ToListAsync();

                return Results.Ok(per);
            })
                .WithName("GetBlankClientBudget")
                .Produces<List<BlankClientBudget>>(StatusCodes.Status200OK);

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

            group.MapGet("/RollerSpecifications/GetActiveRollerShafts/{specId}", GetActiveShaftsForRoller)
                .WithName("GetActiveRollerShaftsBySpecId")
                .Produces<ApiResponse<List<RollerShaft>>>(200);

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

            #endregion

            group.MapPost("/Budgets/Add/", async (ClientBudgets clBud, PulseDbContext dbContext) =>
            {
                dbContext.ClientBudgetMaster.Add(clBud);
                await dbContext.SaveChangesAsync();
                return Results.Created("/Customers/Budgets/Add/", clBud);
            });

            #region PATCH Functions

            group.MapPatch(ByIdPath + "/Update/MasterFile", async (string fullclientid, CustomerUpdateDto updates,
                PulseDbContext db) =>
            {
                if (updates == null)
                    return Results.BadRequest("No update data provided.");

                var client = await db.ClientMaster
                    .FirstOrDefaultAsync(c => c.FullClientID == fullclientid);

                if (client == null)
                    return Results.NotFound($"Client {fullclientid} not found.");

                // Apply only non-null values from the DTO
                if (updates.ClientName != null) client.ClientName = updates.ClientName.Trim();
                if (updates.Address1 != null) client.Address1 = updates.Address1.Trim();
                if (updates.Address2 != null) client.Address2 = updates.Address2.Trim();
                if (updates.Address3 != null) client.Address3 = updates.Address3.Trim();
                if (updates.Address4 != null) client.Address4 = updates.Address4.Trim();
                if (updates.Address5 != null) client.Address5 = updates.Address5.Trim();
                if (updates.PostalAddress1 != null) client.PostalAddress1 = updates.PostalAddress1.Trim();
                if (updates.PostalAddress2 != null) client.PostalAddress2 = updates.PostalAddress2.Trim();
                if (updates.PostalAddress3 != null) client.PostalAddress3 = updates.PostalAddress3.Trim();
                if (updates.PostalAddress4 != null) client.PostalAddress4 = updates.PostalAddress4.Trim();
                if (updates.PostalAddress5 != null) client.PostalAddress5 = updates.PostalAddress5.Trim();
                if (updates.Phone != null) client.Phone = updates.Phone.Trim();
                if (updates.EMail != null) client.EMail = updates.EMail.Trim();
                if (updates.VATNo != null) client.VATNo = updates.VATNo.Trim();
                if (updates.TaxCodeID != null) client.TaxCodeID = updates.TaxCodeID;
                if (updates.RequireOrderNo.HasValue) client.RequireOrderNo = updates.RequireOrderNo.Value;
                if (updates.SalesRepID != null) client.SalesRepID = updates.SalesRepID.Trim();
                if (updates.RegionID.HasValue) client.RegionID = updates.RegionID;
                if (updates.IndustryID.HasValue) client.IndustryID = updates.IndustryID;
                if (updates.SisterCompany.HasValue) client.SisterCompany = updates.SisterCompany.Value;
                if (updates.Blocked.HasValue) client.Blocked = updates.Blocked.Value;
                if (updates.TermDays.HasValue) client.TermDays = updates.TermDays;
                if (updates.CreditLimit.HasValue) client.CreditLimit = updates.CreditLimit;
                if (updates.AiSummary != null)
                {
                    client.AiSummary = updates.AiSummary.Trim();
                    client.AiUpdated = DateTime.UtcNow;
                }

                // Mark as modified so EF knows something changed (optional but safe)
                db.Entry(client).State = EntityState.Modified;

                try
                {
                    var rowsAffected = await db.SaveChangesAsync();
                    return rowsAffected > 0
                        ? Results.Ok(client)
                        : Results.Problem("No changes were saved.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Results.Conflict("The record was modified by another user.");
                }
            })
    .WithName("PatchClientPartial")
    .Produces<Customer>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status409Conflict)
    .Accepts<CustomerUpdateDto>("application/json");

            #endregion
        }

        private static async Task<IResult> GetActiveShaftsForRoller(int specId, PulseDbContext db)
        {
            var shafts = await db.RollerShaftMaster
                    .Where(s => s.ClientRollerSpecificationID == specId && s.IsActive)
                    .ToListAsync();

            if(shafts == null)
            {
                shafts = new List<RollerShaft>();
            }
            return Results.Ok(new ApiResponse<List<RollerShaft>>(shafts));
        }
    }
}
