using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Pulse.Models.Api;
using Pulse.Models.PulseContext;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pulse.ApiService.PulseAI.Services;

public class TablesSQL
{
    private readonly ILogger<TablesSQL>? _logger;
    private readonly PulseDbContext? _dbContext;
    private readonly TSql160Parser _parser; // SQL Server 2022 parser

    public TablesSQL(ILogger<TablesSQL>? logger = null, PulseDbContext? dbContext = null)
    {
        _logger = logger;
        _dbContext = dbContext;
        _parser = new TSql160Parser(true); // true = quoted identifiers enabled
    }

    /// <summary>
    /// Executes a SELECT query with validation and returns structured response.
    /// </summary>
    public async Task<AiQueryResponse<List<Dictionary<string, object>>>> ExecuteValidatedQuery(
        string connectionString,
        string query,
        int timeout = 30)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Step 1: Validate query structure
            var validationErrors = await ValidateQuery(query, connectionString);
            if (validationErrors.Any())
            {
                return AiQueryResponse<List<Dictionary<string, object>>>.ValidationErrorResponse(validationErrors);
            }

            // Step 2: Execute query
            var results = new List<Dictionary<string, object>>();

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection)
            {
                CommandTimeout = timeout,
                CommandType = CommandType.Text
            };

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetValue(i);
                    row[reader.GetName(i)] = value == DBNull.Value ? null : value;
                }
                results.Add(row);
            }

            stopwatch.Stop();

            _logger?.LogInformation("Query executed successfully. Returned {Count} rows in {ElapsedMs}ms",
                results.Count, stopwatch.ElapsedMilliseconds);

            return AiQueryResponse<List<Dictionary<string, object>>>.SuccessResponse(
                results,
                $"Query executed successfully. Retrieved {results.Count} row(s).",
                results.Count,
                stopwatch.ElapsedMilliseconds);
        }
        catch (SqlException sqlEx)
        {
            stopwatch.Stop();
            _logger?.LogError(sqlEx, "SQL error executing query");

            var errorMessage = ParseSqlError(sqlEx);
            return AiQueryResponse<List<Dictionary<string, object>>>.ExecutionErrorResponse(
                errorMessage,
                sqlEx);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger?.LogError(ex, "Error executing query");

            return AiQueryResponse<List<Dictionary<string, object>>>.ExecutionErrorResponse(
                "An unexpected error occurred while executing the query.",
                ex);
        }
    }

    /// <summary>
    /// Validates the SQL query against the database schema using ScriptDom parser.
    /// Properly handles T-SQL syntax including aggregate functions, CTEs, window functions, etc.
    /// </summary>
    private async Task<List<AiQueryValidationError>> ValidateQuery(string query, string connectionString)
    {
        var errors = new List<AiQueryValidationError>();
        try
        {

        

        // Parse the SQL query
        using var reader = new StringReader(query);
        var fragment = _parser.Parse(reader, out IList<ParseError> parseErrors);

        // Check for parse errors (syntax errors)
        if (parseErrors.Any())
        {
            foreach (var parseError in parseErrors)
            {
                errors.Add(new AiQueryValidationError
                {
                    ErrorType = "SyntaxError",
                    Message = $"T-SQL syntax error at line {parseError.Line}, column {parseError.Column}: {parseError.Message}",
                    Suggestion = "Fix the T-SQL syntax error. Check for missing commas, parentheses, or incorrect keywords."
                });
            }
            return errors; // Don't continue if there are syntax errors
        }

        // Ensure it's a SELECT statement
        if (fragment is not TSqlScript script || 
            !script.Batches.Any() || 
            script.Batches[0].Statements.FirstOrDefault() is not SelectStatement selectStatement)
        {
            errors.Add(new AiQueryValidationError
            {
                ErrorType = "InvalidQueryType",
                Message = "Only SELECT queries are allowed for AI query execution.",
                Suggestion = "Regenerate the query as a SELECT statement."
            });
            return errors;
        }

        // Extract tables and columns using visitor pattern
        var visitor = new SqlObjectVisitor();
        fragment.Accept(visitor);

        // Get database schema
        var schemaInfo = await GetSchemaInformation(connectionString);

        // Validate tables exist
        foreach (var table in visitor.Tables)
        {
            var tableName = GetTableName(table);
            if (!schemaInfo.Tables.ContainsKey(tableName))
            {
                var similarTables = FindSimilarNames(tableName, schemaInfo.Tables.Keys);
                errors.Add(new AiQueryValidationError
                {
                    ErrorType = "TableNotFound",
                    Message = $"Table '{tableName}' does not exist in the database.",
                    ObjectName = tableName,
                    Suggestion = similarTables.Any()
                        ? "Did you mean one of these tables?"
                        : "Check the table name spelling or verify it exists in the database.",
                    AvailableAlternatives = similarTables.Any() ? similarTables : null
                });
            }
        }

        // Validate columns exist (excluding those inside aggregate functions)
        foreach (var columnRef in visitor.Columns)
        {
            // Skip wildcard selections (e.g., *, table.*)
            if (columnRef.ColumnType == ColumnType.Wildcard) //  .MultiPartIdentifier.Identifiers.Last().Value == "*")
                continue;

            var columnName = columnRef.MultiPartIdentifier.Identifiers.Last().Value;
            string? tableName = null;

            // Determine which table this column belongs to
            if (columnRef.MultiPartIdentifier.Identifiers.Count > 1)
            {
                // Qualified column reference (e.g., Orders.OrderId)
                var tableIdentifier = columnRef.MultiPartIdentifier.Identifiers[^2].Value;
                
                // Check if it's an alias or actual table name
                tableName = visitor.TableAliases.TryGetValue(tableIdentifier, out var actualTable) 
                    ? actualTable 
                    : tableIdentifier;
            }
            else
            {
                // Unqualified column - find which table it belongs to
                tableName = FindTableForColumn(columnName, visitor.Tables, schemaInfo);
            }

            // Validate column exists in the identified table
            if (tableName != null && schemaInfo.Tables.TryGetValue(tableName, out var tableColumns))
            {
                if (!tableColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase))
                {
                    var similarColumns = FindSimilarNames(columnName, tableColumns);
                    errors.Add(new AiQueryValidationError
                    {
                        ErrorType = "ColumnNotFound",
                        Message = $"Column '{columnName}' does not exist in table '{tableName}'.",
                        ObjectName = $"{tableName}.{columnName}",
                        Suggestion = similarColumns.Any()
                            ? "Did you mean one of these columns?"
                            : $"Available columns in {tableName}: {string.Join(", ", tableColumns.Take(10))}",
                        AvailableAlternatives = similarColumns.Any() ? similarColumns : null
                    });
                }
            }
        }

        return errors;
        }
        catch(Exception ex)
        {
            errors.Add(new AiQueryValidationError
            {
                ErrorType = "General",
                Message = ex.Message,
                ObjectName = "Unknown",
                Suggestion = "None",
                AvailableAlternatives = null
            });
            return errors;
        }
    }

    /// <summary>
    /// Visitor class to extract SQL objects from the parse tree.
    /// Uses ScriptDom's visitor pattern to accurately traverse the AST.
    /// </summary>
    private class SqlObjectVisitor : TSqlFragmentVisitor
    {
        public HashSet<NamedTableReference> Tables { get; } = new();
        public List<ColumnReferenceExpression> Columns { get; } = new();
        public Dictionary<string, string> TableAliases { get; } = new(); // alias -> actual table name
        
        private bool _insideFunctionCall = false;

        public override void Visit(NamedTableReference node)
        {
            Tables.Add(node);
            
            // Track table aliases
            if (node.Alias != null)
            {
                var tableName = GetTableName(node);
                TableAliases[node.Alias.Value] = tableName;
            }
            
            base.Visit(node);
        }

        public override void Visit(FunctionCall node)
        {
            // Mark that we're inside a function call
            // This prevents extracting column references from inside aggregate functions
            _insideFunctionCall = true;
            base.Visit(node);
            _insideFunctionCall = false;
        }

        public override void Visit(ColumnReferenceExpression node)
        {
            // Only collect column references that are NOT inside function calls
            // This solves the SUM(*.*) problem - we don't validate columns inside aggregates
            if (!_insideFunctionCall)
            {
                Columns.Add(node);
            }
            
            base.Visit(node);
        }
    }

    /// <summary>
    /// Extracts the full table name from a NamedTableReference.
    /// Handles schema-qualified names (e.g., dbo.Orders, Production.Products).
    /// </summary>
    private static string GetTableName(NamedTableReference table)
    {
        var parts = table.SchemaObject.Identifiers;
        
        if (parts.Count == 1)
        {
            // Just table name (assumes dbo schema)
            return parts[0].Value;
        }
        else if (parts.Count == 2)
        {
            // Schema.Table
            var schema = parts[0].Value;
            var tableName = parts[1].Value;
            return schema.Equals("dbo", StringComparison.OrdinalIgnoreCase) 
                ? tableName 
                : $"{schema}.{tableName}";
        }
        else if (parts.Count == 3)
        {
            // Database.Schema.Table
            var schema = parts[1].Value;
            var tableName = parts[2].Value;
            return schema.Equals("dbo", StringComparison.OrdinalIgnoreCase) 
                ? tableName 
                : $"{schema}.{tableName}";
        }
        
        return parts.Last().Value;
    }

    /// <summary>
    /// Finds which table an unqualified column belongs to by checking all tables in the query.
    /// </summary>
    private string? FindTableForColumn(
        string columnName, 
        HashSet<NamedTableReference> tables, 
        DatabaseSchema schema)
    {
        foreach (var table in tables)
        {
            var tableName = GetTableName(table);
            if (schema.Tables.TryGetValue(tableName, out var columns))
            {
                if (columns.Contains(columnName, StringComparer.OrdinalIgnoreCase))
                {
                    return tableName;
                }
            }
        }
        
        return null;
    }

    /// <summary>
    /// Gets schema information (tables and columns) from the database using INFORMATION_SCHEMA.
    /// </summary>
    private async Task<DatabaseSchema> GetSchemaInformation(string connectionString)
    {
        var schema = new DatabaseSchema();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Query to get all tables and their columns
        var query = @"
            SELECT 
                t.TABLE_SCHEMA,
                t.TABLE_NAME,
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.IS_NULLABLE
            FROM INFORMATION_SCHEMA.TABLES t
            INNER JOIN INFORMATION_SCHEMA.COLUMNS c 
                ON t.TABLE_NAME = c.TABLE_NAME 
                AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
            WHERE t.TABLE_TYPE = 'BASE TABLE'
            ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION";

        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var schemaName = reader.GetString(0);
            var tableName = reader.GetString(1);
            var columnName = reader.GetString(2);

            var fullTableName = schemaName.Equals("dbo", StringComparison.OrdinalIgnoreCase) 
                ? tableName 
                : $"{schemaName}.{tableName}";

            if (!schema.Tables.ContainsKey(fullTableName))
            {
                schema.Tables[fullTableName] = new List<string>();
            }

            schema.Tables[fullTableName].Add(columnName);
        }

        _logger?.LogDebug("Loaded schema: {TableCount} tables", schema.Tables.Count);
        return schema;
    }

    /// <summary>
    /// Finds similar names using Levenshtein distance for helpful suggestions.
    /// </summary>
    private List<string> FindSimilarNames(string target, IEnumerable<string> candidates, int maxDistance = 3)
    {
        return candidates
            .Where(c => LevenshteinDistance(target.ToLower(), c.ToLower()) <= maxDistance)
            .OrderBy(c => LevenshteinDistance(target.ToLower(), c.ToLower()))
            .Take(5)
            .ToList();
    }

    /// <summary>
    /// Calculates Levenshtein distance between two strings.
    /// Used for fuzzy matching to suggest similar table/column names.
    /// </summary>
    private int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        var distance = new int[source.Length + 1, target.Length + 1];

        for (int i = 0; i <= source.Length; i++) distance[i, 0] = i;
        for (int j = 0; j <= target.Length; j++) distance[0, j] = j;

        for (int i = 1; i <= source.Length; i++)
        {
            for (int j = 1; j <= target.Length; j++)
            {
                var cost = target[j - 1] == source[i - 1] ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[source.Length, target.Length];
    }

    /// <summary>
    /// Parses SQL exception and provides helpful error messages.
    /// </summary>
    private string ParseSqlError(SqlException ex)
    {
        return ex.Number switch
        {
            208 => $"Invalid object name. {ex.Message}",
            207 => $"Invalid column name. {ex.Message}",
            102 => $"Incorrect syntax near '{ex.Message.Split('\'')[1]}'. Check your SQL syntax.",
            156 => $"Incorrect syntax near the keyword. {ex.Message}",
            515 => $"Cannot insert NULL value. {ex.Message}",
            547 => $"Foreign key constraint violation. {ex.Message}",
            2627 => $"Duplicate key violation. {ex.Message}",
            _ => $"SQL Error {ex.Number}: {ex.Message}"
        };
    }

    /// <summary>
    /// Internal class to hold database schema information.
    /// </summary>
    private class DatabaseSchema
    {
        public Dictionary<string, List<string>> Tables { get; set; } = new();
    }
}
