using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Pulse.Models.CustomComponents;
using Pulse.Models.Misc;
using Pulse.Models.PulseContext;
using System.Data;
using System.Text;

namespace Pulse.ApiService.Endpoints
{
    internal static class AiEndpoints
    {
        public static void MapAiEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/AI").WithTags("AI Endpoints");

            group.MapGet("/schema", (PulseDbContext dbContext) =>
            {
                var schemas = dbContext.Model.GetEntityTypes()
                    .Select(et => new SchemaDto
                    {
                        EntityType = et.ClrType.Name, // C# entity type, e.g., "ClientMaster"
                        TableName = et.GetTableName(),
                        PrimaryKeys = et.GetKeys()
                            .Where(k => k.IsPrimaryKey())
                            .SelectMany(k => k.Properties.Select(p => p.GetColumnName()))
                            .ToList(),
                        Columns = et.GetProperties()
                            .Select(p => new ColumnDto
                            {
                                Name = p.GetColumnName(),
                                DataType = p.GetColumnType(),
                                IsNullable = p.IsNullable,
                                IsPrimaryKey = et.FindPrimaryKey()?.Properties.Any(pk => pk.Name == p.Name) ?? false
                            })
                            .ToList(),
                        Relationships = et.GetForeignKeys()
                            .Select(fk => new RelationshipDto
                            {
                                NavigationName = fk.DependentToPrincipal?.Name ?? fk.PrincipalToDependent?.Name ?? "Unnamed",
                                RelatedEntityType = fk.PrincipalEntityType.ClrType.Name,
                                RelatedTableName = fk.PrincipalEntityType.GetTableName(),
                                ForeignKeyColumns = fk.Properties.Select(p => p.GetColumnName()).ToList(),
                                Cardinality = fk.IsUnique ? (fk.PrincipalToDependent != null ? "OneToOne" : "ManyToOne") : "OneToMany"
                            })
                            .ToList()
                    })
                    .ToList();

                return Results.Ok(schemas); // Safe to serialize (no System.Type issues)
            })
            .Produces<List<SchemaDto>>(200);

            group.MapGet("/execute:{sqlS}", async (string sqlS, PulseDbContext dbContext) =>
            {
                Functions f = new Functions();
                string sdb = dbContext.Database.GetConnectionString();
                var qRes = await f.ExecuteAiQry(sdb,sqlS); 
                return Results.Ok(qRes);
            })
                .Produces<List<Dictionary<string, object>>>(200)
                ;

            group.MapGet("/examples", async (PulseDbContext dbContext) =>
            {
                var examples = await dbContext.AiSavedQueries
                    .AsNoTracking()
                    .Where(a => a.IsActive)
                    .Select(a => new { a.Question, a.SqlQuery })
                    .ToListAsync();
                return Results.Ok(examples);
            });

            group.MapGet("/savedqueries", async (PulseDbContext dbContext) =>
            {
                var savedqueries = await dbContext.AiSavedQueries
                    .AsNoTracking()
                    .Where(a => a.IsActive)
                    .ToListAsync();
                return Results.Ok(savedqueries);
            });

            group.MapPost("/aiquery", async (AiQuery aiQuery, PulseDbContext dbContext) =>
            {
                dbContext.AiSavedQueries.Add(aiQuery);
                await dbContext.SaveChangesAsync();
                return Results.Created($"/AI/aiquery/{aiQuery.AiQueryID}", aiQuery);
            });
                
            
        }
    }
}
