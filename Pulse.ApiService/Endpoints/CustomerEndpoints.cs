using Microsoft.EntityFrameworkCore;
using Pulse.Models.Customers;
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
        }
    }
}
