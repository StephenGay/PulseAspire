using Microsoft.EntityFrameworkCore;
using Pulse.Models.Customers;
using Pulse.Models.PulseContext;

namespace Pulse.ApiService.Endpoints
{
    internal static class CustomerEndpoints
    {
        internal const string BasePath = "/Customers";
        internal const string ByIdPath = "/Details/{fullclientid}";
        
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
        }
    }
}
