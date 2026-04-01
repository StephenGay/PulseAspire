using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
//using Pulse.ApiService.Queries;  // Should match Functions.cs namespace

public class Functions
{
    //private readonly ILogger<Functions>? /// _logger;

    //public Functions(ILogger<PulseAI.Characters.TablesAPI> logger1, ILogger<Functions>? logger = null)
    //{
    //    /// _logger = logger;
    //}

    /// <summary>
    /// Executes a SELECT query and returns results as a list of dictionaries.
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    /// <param name="query">SQL query to execute (should be SELECT only)</param>
    /// <param name="timeout">Command timeout in seconds (default: 30)</param>
    /// <returns>List of rows as dictionaries, or empty list on error</returns>
    public async Task<List<Dictionary<string, object>>> ExecuteAiQry(
        string connectionString,
        string query,
        int timeout = 30)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            ///// _logger?.LogError("Connection string is null or empty");
            throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            ///// _logger?.LogError("Query is null or empty");
            throw new ArgumentNullException(nameof(query), "Query cannot be null or empty");
        }

        // Validate SELECT-only query for safety
        if (!query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            ///// _logger?.LogWarning("Attempted to execute non-SELECT query: {Query}", query);
            throw new InvalidOperationException("Only SELECT queries are allowed in ExecuteAiQry");
        }

        var results = new List<Dictionary<string, object>>();

        try
        {
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

            ///// _logger?.LogInformation("Query executed successfully. Returned {Count} rows", results.Count);
            return results;
        }
        catch (SqlException sqlEx)
        {
            ///// _logger?.LogError(sqlEx,
            //    "SQL error executing query. ErrorNumber: {ErrorNumber}, LineNumber: {LineNumber}, Query: {Query}",
            //    sqlEx.Number,
            //    sqlEx.LineNumber,
            //    query);
            throw new InvalidOperationException($"Database error: {sqlEx.Message} (Error {sqlEx.Number})", sqlEx);
        }
        catch (InvalidOperationException)
        {
            // Re-throw validation exceptions
            throw;
        }
        catch (Exception ex)
        {
            ///// _logger?.LogError(ex, "Unexpected error executing query: {Query}", query);
            throw new InvalidOperationException($"Error executing query: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Executes an INSERT, UPDATE, or DELETE query and returns the number of affected rows.
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    /// <param name="query">SQL query to execute (INSERT, UPDATE, DELETE)</param>
    /// <param name="timeout">Command timeout in seconds (default: 30)</param>
    /// <returns>Number of rows affected, or -1 on error</returns>
    public async Task<int> ExecuteAiUpdateInsertQry(
        string connectionString,
        string query,
        int timeout = 30)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            /// _logger?.LogError("Connection string is null or empty");
            throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            /// _logger?.LogError("Query is null or empty");
            throw new ArgumentNullException(nameof(query), "Query cannot be null or empty");
        }

        // Validate non-SELECT query
        var trimmedQuery = query.TrimStart();
        var isModifyQuery = trimmedQuery.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase) ||
                           trimmedQuery.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) ||
                           trimmedQuery.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase);

        if (!isModifyQuery)
        {
            /// _logger?.LogWarning("Attempted to execute non-modify query: {Query}", query);
            throw new InvalidOperationException("Only INSERT, UPDATE, or DELETE queries are allowed");
        }

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection)
            {
                CommandTimeout = timeout,
                CommandType = CommandType.Text
            };

            var rowsAffected = await command.ExecuteNonQueryAsync();

            /// _logger?.LogInformation("Query executed successfully. {RowsAffected} rows affected", rowsAffected);
            return rowsAffected;
        }
        catch (SqlException sqlEx)
        {
            /// _logger?.LogError(sqlEx,
                //"SQL error executing query. ErrorNumber: {ErrorNumber}, LineNumber: {LineNumber}, Query: {Query}",
                //sqlEx.Number,
                //sqlEx.LineNumber,
                //query);
            throw new InvalidOperationException($"Database error: {sqlEx.Message} (Error {sqlEx.Number})", sqlEx);
        }
        catch (InvalidOperationException)
        {
            // Re-throw validation exceptions
            throw;
        }
        catch (Exception ex)
        {
            /// _logger?.LogError(ex, "Unexpected error executing query: {Query}", query);
            throw new InvalidOperationException($"Error executing query: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Executes a query with parameterized inputs to prevent SQL injection.
    /// </summary>
    public async Task<List<Dictionary<string, object>>> ExecuteParameterizedQuery(
        string connectionString,
        string query,
        Dictionary<string, object> parameters,
        int timeout = 30)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentNullException(nameof(query));

        var results = new List<Dictionary<string, object>>();

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection)
            {
                CommandTimeout = timeout,
                CommandType = CommandType.Text
            };

            // Add parameters
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                }
            }

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

            /// _logger?.LogInformation("Parameterized query executed. Returned {Count} rows", results.Count);
            return results;
        }
        catch (SqlException sqlEx)
        {
            /// _logger?.LogError(sqlEx, "SQL error executing parameterized query");
            throw new InvalidOperationException($"Database error: {sqlEx.Message}", sqlEx);
        }
        catch (Exception ex)
        {
            /// _logger?.LogError(ex, "Error executing parameterized query");
            throw new InvalidOperationException($"Error executing query: {ex.Message}", ex);
        }
    }
}








#region OldCode
//using System;
//using System.Collections.Generic;
//using System.Data;
//using Microsoft.Data.SqlClient;

//public class Functions
//{
//    public async Task<List<Dictionary<string, object>>> ExecuteAiQry(string connectionString, string query)
//    {

//        await Task.Delay(100);
//        var results = new List<Dictionary<string, object>>();

//        try
//        {
//            using (SqlConnection connection = new SqlConnection(connectionString))
//            {
//                connection.Open();
//                using (SqlCommand command = new SqlCommand(query, connection))
//                using (SqlDataReader reader = command.ExecuteReader())
//                {
//                    while (reader.Read())
//                    {
//                        var row = new Dictionary<string, object>();
//                        for (int i = 0; i < reader.FieldCount; i++)
//                        {
//                            row[reader.GetName(i)] = reader.GetValue(i);
//                        }
//                        results.Add(row);
//                    }
//                }
//            }

//            return results;
//        }
//        catch (Exception ex)
//        {
//            //var errorRow = new Dictionary<string, object>
//            //{
//            //    { "Error", ex.Message }
//            //};
//            //results.Add(errorRow);
//            //return results;
//            //throw new Exception(ex.Message);
//            return null;
//        }
//    }
//    public async Task<int> ExecuteAiUpdateInsertQry(string connectionString, string query)
//    {

//        await Task.Delay(100);
//        var results = 0;

//        try
//        {
//            using (SqlConnection connection = new SqlConnection(connectionString))
//            {
//                connection.Open();
//                using (SqlCommand command = new SqlCommand(query, connection))
//                {
//                    results = command.ExecuteNonQuery();
//                }
//            }

//            return results;
//        }
//        catch (Exception ex)
//        {
//            return results;
//        }
//    }
//}

#endregion