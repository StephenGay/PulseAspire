using Microsoft.EntityFrameworkCore;
using Pulse.Models.Customers;
using Pulse.Models.Industries;
using Pulse.Models.PulseContext;
using Pulse.Models.Rollers;

namespace Pulse.ApiService.Endpoints
{
    internal static class TechnicalEndpoints
    {
        internal const string BasePath = "/Technical";
        

        public static void MapTechnicalEndpoints(this IEndpointRouteBuilder routes)
        { 
        
            var group = routes.MapGroup(BasePath).WithTags("Technical");

            group.MapGet("/RollerTypes/GetAll", async (PulseDbContext db) =>
               await db.RollerTypeMaster
                   .AsNoTracking()
                   .Where(r => r.IsActive)
                   .ToListAsync())
                .Produces<List<RollerType>>(StatusCodes.Status200OK);

            group.MapGet("/IndustryProcess/GetByIndustry/{industryId}", async (int industryId, PulseDbContext db) =>
               {
                   var pm = await db.IndustryProcessMaster
                       .AsNoTracking()
                       .Where(r => r.IsActive && r.IndustryID == industryId)
                       .ToListAsync();
                    if(pm == null) { pm = new List<Models.Industries.IndustryProcess>(); }
                   return Results.Ok(pm);
               })
                 .Produces<List<IndustryProcess>>(StatusCodes.Status200OK);
        }
    }
}