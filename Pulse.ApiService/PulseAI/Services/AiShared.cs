using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Pulse.Models.CustomComponents;
using Pulse.Models.Misc;
using Pulse.Models.PulseContext;
using System.Text;
using System.Text.Json;

namespace Pulse.ApiService.PulseAI.Services
{
    public class AiShared
    {
        private static class CacheKeys
        {
            public const string Schema = "pulse_ai_schema";
            public const string Examples = "pulse_ai_examples";
        }

        private static readonly TimeSpan SchemaCacheDuration = TimeSpan.FromMinutes(120);
        private static readonly TimeSpan ExamplesCacheDuration = TimeSpan.FromMinutes(60);

        private readonly IMemoryCache _cache;
        private readonly IDbContextFactory<PulseDbContext> _dbFactory;
        private readonly ILogger<AiShared> _logger;

        public AiShared(
            IMemoryCache cache,
            IDbContextFactory<PulseDbContext> dbFactory,
            ILogger<AiShared> logger)
        {
            _cache = cache;
            _dbFactory = dbFactory;
            _logger = logger;
        }

        /// <summary>
        /// Gets detailed schema for AI query generation. Uses caching for performance.
        /// </summary>
        public async Task<string> GetDetailedSchemaAsync(CancellationToken ct = default)
        {
            return await _cache.GetOrCreateAsync(CacheKeys.Schema, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SchemaCacheDuration;

                var schemas = await GetSchemaFromDbContextAsync(ct);
                if (schemas.Count == 0)
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                    _logger.LogWarning("Schema fetch returned 0 tables");
                    return "";
                }

                var schemaBuilder = new StringBuilder();
                foreach (var schema in schemas)
                {
                    AppendSchemaEntity(schemaBuilder, schema);
                }

                _logger.LogInformation("Schema cached: {TableCount} tables", schemas.Count);
                return schemaBuilder.ToString();
            }) ?? "";
        }

        public async Task<string> GetAiExamplesAsync(CancellationToken ct = default)
        {
            return await _cache.GetOrCreateAsync(CacheKeys.Examples, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = ExamplesCacheDuration;

                try
                {
                    await using var dbContext = await _dbFactory.CreateDbContextAsync(ct);

                    var aiQueries = await dbContext.AiSavedQueries.ToListAsync<AiQuery>(ct);
                    

                    if (aiQueries.Count == 0)
                        return "";

                    _logger.LogInformation("Examples cached: {Count} queries", aiQueries.Count);
                    return "\nExamples:\n" + string.Join("\n", aiQueries.Select(q =>
                        $"Q: {q.Question}\nSQL: {q.SqlQuery}"));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch examples");
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                    return "";
                }
            }) ?? "";
        }

        /// <summary>
        /// Gets list of schema DTOs for AI processing.
        /// </summary>
        public async Task<List<SchemaDto>> GetSchemaListAsync(CancellationToken ct = default)
        {
            return await GetSchemaFromDbContextAsync(ct);
        }

        private async Task<List<SchemaDto>> GetSchemaFromDbContextAsync(CancellationToken ct = default)
        {
            try
            {
                // Create a new DbContext instance from the factory
                await using var dbContext = await _dbFactory.CreateDbContextAsync(ct);

                var schemas = dbContext.Model.GetEntityTypes()
                    .Select(et => new SchemaDto
                    {
                        EntityType = et.ClrType.Name,
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
                                Cardinality = fk.IsUnique ? "OneToOne" : "OneToMany"
                            })
                            .ToList()
                    })
                    .ToList();

                return schemas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch schema from DbContext");
                return new List<SchemaDto>();
            }
        }

        private void AppendSchemaEntity(StringBuilder builder, SchemaDto schema)
        {
            builder.AppendLine($"Table: {schema.TableName} (Entity: {schema.EntityType})");
            builder.AppendLine($"Primary Keys: {string.Join(", ", schema.PrimaryKeys)}");
            builder.AppendLine("Columns:");
            foreach (var col in schema.Columns)
            {
                builder.AppendLine($"  - {col.Name} ({col.DataType}) {(col.IsNullable ? "NULL" : "NOT NULL")}");
            }
            if (schema.Relationships.Any())
            {
                builder.AppendLine("Relationships:");
                foreach (var rel in schema.Relationships)
                {
                    builder.AppendLine($"  - {rel.NavigationName} -> {rel.RelatedTableName} ({rel.Cardinality})");
                }
            }
            builder.AppendLine();
        }

        /// <summary>
        /// Invalidates the schema cache (useful after migrations or schema changes).
        /// </summary>
        public void InvalidateSchemaCache()
        {
            _cache.Remove(CacheKeys.Schema);
            _logger.LogInformation("Schema cache invalidated");
        }
    }
}
