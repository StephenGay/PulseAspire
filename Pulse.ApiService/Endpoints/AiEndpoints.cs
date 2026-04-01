using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Pulse.ApiService.PulseAI.Services;
using Pulse.Models.AI;
using Pulse.Models.Api;
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

            group.MapPost("/execute-validated", async (
                [FromBody] AiQueryRequest request,
                PulseDbContext dbContext,
                ILoggerFactory loggerFactory) =>
                        {
                            var logger = loggerFactory.CreateLogger<TablesSQL>();
                            var functions = new TablesSQL(logger, dbContext);

                            try
                            {
                                var connectionString = dbContext.Database.GetConnectionString();
                                var response = await functions.ExecuteValidatedQuery(connectionString, request.Query, request.Timeout ?? 30);

                                return response.Success
                                    ? Results.Ok(response)
                                    : Results.BadRequest(response);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error executing validated AI query");
                                return Results.Problem("An unexpected error occurred");
                            }
                        })
            .Produces<AiQueryResponse<List<Dictionary<string, object>>>>(200)
            .ProducesProblem(400)
            .ProducesProblem(500);

            
        group.MapGet("/schema", (PulseDbContext dbContext) =>
            {
                var schemas = dbContext.Model.GetEntityTypes()
                    .Select(et => new SchemaDto
                    {
                        EntityType = et.ClrType.Name, // C# entity type, e.g., "ClientSales"
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
                                Cardinality = fk.IsUnique ? "OneToOne" : "OneToMany"  // Updated: Use IsUnique to determine cardinality (OneToOne if unique FK, otherwise OneToMany)
                            })
                            .ToList()
                    })
                    .ToList();

                return Results.Ok(schemas); // Safe to serialize (no System.Type issues)
            })
.Produces<List<SchemaDto>>(200);

            group.MapGet("/execute:{sqlS}", async (
                string sqlS, 
                PulseDbContext dbContext,
                ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger<TablesSQL>();
                var functions = new TablesSQL(logger, dbContext);
                var connectionString = dbContext.Database.GetConnectionString();

                try
                {
                    var response = await functions.ExecuteValidatedQuery(connectionString, sqlS);
                    return response.Success ? Results.Ok(response.Data) : Results.BadRequest(response);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error executing query");
                    return Results.Problem("Error executing query");
                }
            })
            .Produces<List<Dictionary<string, object>>>(200)
            .ProducesProblem(400);

            group.MapGet("/ExecuteAiUpdateInsertQry:{sqlS2}", async (
                string sqlS2, 
                PulseDbContext dbContext,
                ILoggerFactory loggerFactory) =>
            {
                // Note: This endpoint is deprecated - use POST with proper validation instead
                var logger = loggerFactory.CreateLogger("ExecuteUpdateInsert");
                logger.LogWarning("ExecuteAiUpdateInsertQry endpoint is deprecated and will be removed");

                return Results.BadRequest(new
                {
                    error = "This endpoint is deprecated. Please use POST /execute-validated instead.",
                    message = "For security reasons, INSERT/UPDATE/DELETE operations should use proper validation."
                });
            })
            .Produces<int>(200)
            .ProducesProblem(400);

            group.MapGet("/execute4bot:{sql}", async (string sql, PulseDbContext dbContext) =>
            {
                // Safety: Validate SQL is SELECT-only (e.g., sql.ToLower().StartsWith("select"))
                if (!sql.TrimStart().ToLowerInvariant().StartsWith("select"))
                {
                    return Results.BadRequest("Only SELECT queries allowed.");
                }

                try
                {
                    var data = await dbContext.Database.SqlQueryRaw<dynamic>(sql).ToListAsync();
                    // Convert dynamic to Dictionary for JSON
                    var results = data.Select(d => d as IDictionary<string, object>).Select(dict => dict.ToDictionary(pair => pair.Key, pair => pair.Value)).ToList();
                    return Results.Ok(results);
                }
                catch (Exception ex)
                {
                    //_logger.LogError(ex, "SQL execution error");
                    return Results.Problem("Error executing SQL.");
                }
            })
            .Produces<List<Dictionary<string, object>>>(200);

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

            group.MapPut("/savedqueries/vote/{id}/{vote}", async (int id, string vote, PulseDbContext dbContext) =>
            {
                var qry = await dbContext.AiSavedQueries.FindAsync(id);
                if (qry == null)
                {
                    return Results.NotFound($"Query ID {id} not found.");
                }
                if (vote == "up")
                {
                    qry.UpVote++;
                }
                else
                {
                    qry.DownVote++;
                }
                await dbContext.SaveChangesAsync();
                return Results.Ok(qry);
            });
            
        }
    }
}
