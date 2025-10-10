using Microsoft.EntityFrameworkCore;
using Pulse.Models.PulseContext;
using System.Text;
using System.Data;

namespace Pulse.ApiService.Endpoints
{
    internal static class AiEndpoints
    {
        public static void MapAiEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/AI").WithTags("AI Endpoints");

            // Add endpoint for schema info
            group.MapGet("/schema", async (PulseDbContext dbContext) =>
            {

                var sb = new StringBuilder();
                var entityTypes = dbContext.Model.GetEntityTypes();
                await Task.Delay(100);
                foreach (var entityType in entityTypes)
                {
                    sb.AppendLine($"Table: {entityType.GetTableName()}");
                    foreach (var property in entityType.GetProperties())
                    {
                        sb.AppendLine($"  {property.Name} ({property.ClrType.Name})");
                    }
                    sb.AppendLine();
                }

                return Results.Text(sb.ToString(), "text/plain");
            });

            group.MapGet("/execute:{sqlS}", async (string sqlS, PulseDbContext dbContext) =>
            {
                Functions f = new Functions();
                string sdb = dbContext.Database.GetConnectionString();
                var qRes = await f.ExecuteAiQry(sqlS, sdb); 
                return Results.Ok(qRes);
            });

            group.MapGet("/examples", async (PulseDbContext dbContext) =>
            {
                var examples = await dbContext.AiSavedQueries
                    .AsNoTracking()
                    .Where(a => a.IsActive)
                    .Select(a => new { a.Question, a.SqlQuery })
                    .ToListAsync();
                return Results.Ok(examples);
            });

            //group.MapPost("/generate", async (GenerateSqlRequest request, PulseDbContext dbContext) =>
            //{
            //    // Placeholder for AI integration logic
            //    // In a real implementation, you would call your AI service here
            //    // For demonstration, we'll return a dummy SQL query
            //    string dummySql = "SELECT TOP 10 * FROM CustomerMaster;";
            //    // Optionally, save the query to the database
            //    var aiQuery = new Models.Misc.AiQuery
            //    {
            //        Question = request.NaturalLanguageQuery,
            //        SqlQuery = dummySql,
            //        Timestamp = DateTime.UtcNow,
            //        IsActive = true
            //    };
            //    dbContext.AiSavedQueries.Add(aiQuery);
            //    await dbContext.SaveChangesAsync();
            //    return Results.Ok(new { SqlQuery = dummySql });
            //});
        }
    }
}
