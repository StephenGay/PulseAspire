using Pulse.Models.Organizational;
using Microsoft.EntityFrameworkCore;

using Pulse.Models.PulseContext;
using Pulse.Models.Misc;

namespace Pulse.ApiService.Endpoints
{
    internal static class CompanyEndpoints
    {
        internal const string BasePath = "/Companies";
        internal const string ByCompanyIdPath = "/{companyid}";
        public static void MapCompanyEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup(BasePath).WithTags("Companies");

            group.MapGet("/GetAll", async (PulseDbContext db) =>
                await db.CompanyMaster
                .Where(d => d.IsActive)
                .ToListAsync())
                .WithName("GetAllCompanies")
                .Produces<List<Company>>(StatusCodes.Status200OK);

            group.MapGet("/{companyid}/CurrentPeriod/{mnth}/{yr}", async (int CompanyId,string mnth, string yr, PulseDbContext db) =>
            {
                var curPer = db.PeriodMaster
                        .AsNoTracking()
                        .Where(p => p.Month == mnth && p.CalendarYear == yr )
                        .FirstOrDefault();
                return Results.Ok(curPer);
            })
                .Produces<Period>(StatusCodes.Status200OK);



        }
    }
}

