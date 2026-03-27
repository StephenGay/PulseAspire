using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pulse.Models.Api;
using Pulse.Models.PulseContext;


namespace Pulse.ApiService.PulseAI.Functions;

public class SQL
{
    private readonly ILogger<SQL>? _logger;
    private readonly PulseDbContext? _dbContext;

    public SQL(ILogger<SQL>? logger = null, PulseDbContext? dbContext = null)
    {
        _logger = logger;
        _dbContext = dbContext;
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
    /// Validates the SQL query against the database schema.
    /// </summary>
    private async Task<List<AiQueryValidationError>> ValidateQuery(string query, string connectionString)
    {
        var errors = new List<AiQueryValidationError>();

        // Validate it's a SELECT query
        if (!query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(new AiQueryValidationError
            {
                ErrorType = "InvalidQueryType",
                Message = "Only SELECT queries are allowed for AI query execution.",
                Suggestion = "Regenerate the query as a SELECT statement."
            });
            return errors;
        }

        // Extract tables and columns from query
        var tables = ExtractTableNames(query);
        var columns = ExtractColumnNames(query);

        // Validate tables exist
        var schemaInfo = await GetSchemaInformation(connectionString);

        foreach (var table in tables)
        {
            if (!schemaInfo.Tables.ContainsKey(table))
            {
                var similarTables = FindSimilarNames(table, schemaInfo.Tables.Keys);
                errors.Add(new AiQueryValidationError
                {
                    ErrorType = "TableNotFound",
                    Message = $"Table '{table}' does not exist in the database.",
                    ObjectName = table,
                    Suggestion = similarTables.Any()
                        ? $"Did you mean one of these tables?"
                        : "Check the table name spelling or verify it exists in the database.",
                    AvailableAlternatives = similarTables.Any() ? similarTables : null
                });
            }
        }

        // Validate columns exist in their respective tables
        foreach (var (table, columnList) in columns)
        {
            if (!schemaInfo.Tables.ContainsKey(table)) continue; // Already flagged as error

            var tableColumns = schemaInfo.Tables[table];
            foreach (var column in columnList)
            {
                if (column == "*") continue; // Skip wildcard

                if (!tableColumns.Contains(column, StringComparer.OrdinalIgnoreCase))
                {
                    var similarColumns = FindSimilarNames(column, tableColumns);
                    errors.Add(new AiQueryValidationError
                    {
                        ErrorType = "ColumnNotFound",
                        Message = $"Column '{column}' does not exist in table '{table}'.",
                        ObjectName = $"{table}.{column}",
                        Suggestion = similarColumns.Any()
                            ? "Did you mean one of these columns?"
                            : $"Available columns in {table}: {string.Join(", ", tableColumns.Take(10))}",
                        AvailableAlternatives = similarColumns.Any() ? similarColumns : null
                    });
                }
            }
        }

        // Validate SQL functions
        var functions = ExtractSqlFunctions(query);
        var invalidFunctions = ValidateSqlFunctions(functions);
        errors.AddRange(invalidFunctions);

        return errors;
    }

    /// <summary>
    /// Gets schema information (tables and columns) from the database.
    /// </summary>
    private async Task<DatabaseSchema> GetSchemaInformation(string connectionString)
    {
        var schema = new DatabaseSchema();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Get all tables and their columns
        var query = @"
            SELECT 
                t.TABLE_SCHEMA,
                t.TABLE_NAME,
                c.COLUMN_NAME
            FROM INFORMATION_SCHEMA.TABLES t
            INNER JOIN INFORMATION_SCHEMA.COLUMNS c 
                ON t.TABLE_NAME = c.TABLE_NAME 
                AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
            WHERE t.TABLE_TYPE = 'BASE TABLE'
            ORDER BY t.TABLE_NAME, c.ORDINAL_POSITION";

        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var schemaName = reader.GetString(0);
            var tableName = reader.GetString(1);
            var columnName = reader.GetString(2);

            var fullTableName = schemaName == "dbo" ? tableName : $"{schemaName}.{tableName}";

            if (!schema.Tables.ContainsKey(fullTableName))
            {
                schema.Tables[fullTableName] = new List<string>();
            }

            schema.Tables[fullTableName].Add(columnName);
        }

        return schema;
    }

    /// <summary>
    /// Extracts table names from SQL query.
    /// </summary>
    private List<string> ExtractTableNames(string query)
    {
        var tables = new List<string>();

        // Match FROM and JOIN clauses
        var pattern = @"(?:FROM|JOIN)\s+(?:\[?(\w+)\]?\.)?(\[?\w+\]?)(?:\s+(?:AS\s+)?(\w+))?";
        var matches = Regex.Matches(query, pattern, RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            var schema = match.Groups[1].Value;
            var table = match.Groups[2].Value.Trim('[', ']');

            if (!string.IsNullOrEmpty(schema))
            {
                tables.Add($"{schema}.{table}");
            }
            else
            {
                tables.Add(table);
            }
        }

        return tables.Distinct().ToList();
    }

    /// <summary>
    /// Extracts column names grouped by table from SQL query.
    /// </summary>
    private Dictionary<string, List<string>> ExtractColumnNames(string query)
    {
        var columns = new Dictionary<string, List<string>>();

        // Extract SELECT clause columns
        var selectPattern = @"SELECT\s+(.*?)\s+FROM";
        var selectMatch = Regex.Match(query, selectPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (selectMatch.Success)
        {
            var selectClause = selectMatch.Groups[1].Value;
            var columnPattern = @"(?:(\w+)\.)?(\w+|\*)";
            var columnMatches = Regex.Matches(selectClause, columnPattern);

            foreach (Match match in columnMatches)
            {
                var table = match.Groups[1].Value;
                var column = match.Groups[2].Value;

                if (string.IsNullOrEmpty(table))
                {
                    // Assign to all tables if no table specified
                    var tables = ExtractTableNames(query);
                    foreach (var t in tables)
                    {
                        if (!columns.ContainsKey(t))
                            columns[t] = new List<string>();
                        columns[t].Add(column);
                    }
                }
                else
                {
                    if (!columns.ContainsKey(table))
                        columns[table] = new List<string>();
                    columns[table].Add(column);
                }
            }
        }

        return columns;
    }

    /// <summary>
    /// Extracts SQL functions from query.
    /// </summary>
    private List<string> ExtractSqlFunctions(string query)
    {
        var functions = new List<string>();
        var pattern = @"\b([A-Z_]+)\s*\(";
        var matches = Regex.Matches(query, pattern, RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            functions.Add(match.Groups[1].Value.ToUpper());
        }

        return functions.Distinct().ToList();
    }

    /// <summary>
    /// Validates SQL functions against known SQL Server functions.
    /// </summary>
    private List<AiQueryValidationError> ValidateSqlFunctions(List<string> functions)
    {
        var errors = new List<AiQueryValidationError>();
        var validFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "COUNT", "SUM", "AVG", "MIN", "MAX", "CAST", "CONVERT", "GETDATE", "DATEADD",
            "DATEDIFF", "YEAR", "MONTH", "DAY", "ISNULL", "COALESCE", "SUBSTRING", "LEN",
            "UPPER", "LOWER", "TRIM", "LTRIM", "RTRIM", "REPLACE", "CONCAT", "LEFT", "RIGHT",
            "CHARINDEX", "PATINDEX", "STUFF", "REPLICATE", "REVERSE", "FORMAT", "ROW_NUMBER",
            "RANK", "DENSE_RANK", "NTILE", "LAG", "LEAD", "FIRST_VALUE", "LAST_VALUE"
        };

        foreach (var function in functions)
        {
            if (!validFunctions.Contains(function))
            {
                var similar = FindSimilarNames(function, validFunctions);
                errors.Add(new AiQueryValidationError
                {
                    ErrorType = "InvalidFunction",
                    Message = $"Function '{function}' is not a recognized SQL Server function or may not be supported.",
                    ObjectName = function,
                    Suggestion = similar.Any() ? "Did you mean one of these functions?" : "Verify the function name or use a different approach.",
                    AvailableAlternatives = similar.Any() ? similar : null
                });
            }
        }

        return errors;
    }

    /// <summary>
    /// Finds similar names using Levenshtein distance.
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
    /// Parses SQL exception and provides helpful error message.
    /// </summary>
    private string ParseSqlError(SqlException ex)
    {
        return ex.Number switch
        {
            208 => $"Invalid object name. {ex.Message}",
            207 => $"Invalid column name. {ex.Message}",
            102 => $"Incorrect syntax near '{ex.Message.Split('\'')[1]}'. Check your SQL syntax.",
            156 => $"Incorrect syntax near the keyword. {ex.Message}",
            _ => $"SQL Error {ex.Number}: {ex.Message}"
        };
    }

    private class DatabaseSchema
    {
        public Dictionary<string, List<string>> Tables { get; set; } = new();
    }
}
