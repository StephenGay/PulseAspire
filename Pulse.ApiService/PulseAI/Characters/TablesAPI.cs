using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using Pulse.ApiService.PulseAI.Services;
using Pulse.Models.AI;
using Pulse.Models.Api;
using Pulse.Models.PulseContext;
using System.Text;
using static Pulse.Models.AI.Tables.TablesChatStructures;

namespace Pulse.ApiService.PulseAI.Characters;

public class TablesAPI
{
    private readonly AiShared _aiShared;
    //private readonly HttpClient _tablesClient;
    private readonly IPulseAiClientFactory _aiClientFactory;
    private readonly IDbContextFactory<PulseDbContext> _dbFactory;
    private readonly ILogger<TablesAPI> _logger;

    // Session management - stores chat history per session
    private static readonly Dictionary<string, Chat> _sessions = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object _sessionLock = new object();

    public TablesAPI(
        AiShared aiShared,
        IPulseAiClientFactory aiClientFactory,
        //HttpClient tablesClient,
        IDbContextFactory<PulseDbContext> dbFactory,
        ILogger<TablesAPI> logger)
    {
        _aiShared = aiShared;
        _aiClientFactory = aiClientFactory;
        //_tablesClient = tablesClient;
        _dbFactory = dbFactory;
        _logger = logger;

    }

    /// <summary>
    /// Main entry point for asking Tables a question.
    /// </summary>
    /// <summary>
    /// Main entry point for asking Tables a question.
    /// Includes self-correction retry logic on validation failures.
    /// </summary>
    public async Task<ApiResponse<List<Dictionary<string, object>>>> AskTablesAsync(PulseAiRequest request)
    {
        var response = new TablesResponse
        {
            ModelUsed = request.ModelName ?? "gpt-oss:latest",
            SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow
        };

        const int MaxAttempts = 3;

        try
        {
            _logger.LogInformation("Tables AI request: {Question}, Session: {SessionId}",
                request.UserRequest.Content, response.SessionId);

            // Get or create chat session
            var chat = GetOrCreateChatSession(response.SessionId, response.ModelUsed);

            // Initialize system prompt if this is a new session
            if (chat.Messages.Count == 0)
            {
                string systemPrompt = await GetTablesSystemMessageAsync();

                await foreach (var chunk in chat.SendAsync(systemPrompt, CancellationToken.None))
                {
                    // Consume stream
                }

                _logger.LogInformation("System prompt initialized for session {SessionId}", response.SessionId);
            }

            string currentPrompt = request.UserRequest.Content;
            string finalSqlQuery = string.Empty;
            string finalAiResponse = string.Empty;
            AiQueryResponse<List<Dictionary<string, object>>>? finalQueryResult = null;
            int attemptsUsed = 0;

            for (attemptsUsed = 1; attemptsUsed <= MaxAttempts; attemptsUsed++)
            {
                if (attemptsUsed > 1)
                {
                    _logger.LogInformation("Self-correction retry #{Attempt} for session {SessionId}", attemptsUsed, response.SessionId);
                }

                // Send prompt (original question or correction prompt)
                var sqlQueryResponseBuilder = new StringBuilder();
                await foreach (var chunk in chat.SendAsync(currentPrompt, CancellationToken.None))
                {
                    sqlQueryResponseBuilder.Append(chunk);
                }
                finalAiResponse = sqlQueryResponseBuilder.ToString();

                // Extract SQL
                var sqlQuery = ExtractSqlFromResponse(finalAiResponse);

                if (string.IsNullOrWhiteSpace(sqlQuery))
                {
                    finalQueryResult = AiQueryResponse<List<Dictionary<string, object>>>.ValidationErrorResponse(
                        new List<AiQueryValidationError>
                        {
                        new AiQueryValidationError
                        {
                            ErrorType = "NoSqlExtracted",
                            Message = "AI response did not contain a valid SQL query block."
                        }
                        });

                    // Prepare correction prompt for next attempt
                    currentPrompt = "You did not return a SQL query in the required ```sql block format. Please try again and output ONLY the SQL query wrapped in ```sql ... ```.";
                    continue;
                }

                _logger.LogInformation("Attempt {Attempt} - Generated SQL: {Sql}", attemptsUsed, sqlQuery);

                // Validate + (conditionally) execute
                await using var dbContext = await _dbFactory.CreateDbContextAsync();
                var connectionString = dbContext.Database.GetConnectionString();
                var tablesSQL = new TablesSQL(null, dbContext);

                finalQueryResult = await tablesSQL.ExecuteValidatedQuery(connectionString, sqlQuery);

                if (finalQueryResult.Success ||
                    (finalQueryResult.ValidationErrors == null || !finalQueryResult.ValidationErrors.Any()))
                {
                    finalSqlQuery = sqlQuery;
                    break; // Success!
                }

                // Validation failed — prepare self-correction prompt using the rich error details
                var errorSummary = string.Join(" | ", finalQueryResult.ValidationErrors!.Select(e =>
                    $"{e.ErrorType}: {e.Message}" +
                    (e.AvailableAlternatives?.Any() == true ? $" (Did you mean: {string.Join(", ", e.AvailableAlternatives)})" : "")));

                currentPrompt = $"The SQL query from your previous attempt failed validation with these errors: {errorSummary}. " +
                                "Using the provided database schema and relationships, please generate a corrected SQL query. " +
                                "Remember the rules: output ONLY the corrected SQL inside a ```sql block. No explanations.";

                _logger.LogWarning("Attempt {Attempt} validation failed for session {SessionId}. Errors: {Errors}",
                    attemptsUsed, response.SessionId, errorSummary);
            }

            // After all attempts
            response.AttemptsUsed = attemptsUsed; // Add this property to TablesResponse if not present (or remove if model doesn't have it yet)
            response.GeneratedSql = finalSqlQuery;
            response.IsSuccess = finalQueryResult?.Success ?? false;
            response.ValidationErrors = finalQueryResult?.ValidationErrors;
            response.ExecutionDetails = finalQueryResult?.ExecutionDetails;

            if (!string.IsNullOrWhiteSpace(finalSqlQuery) && (finalQueryResult?.Success == true))
            {
                response.Data = finalQueryResult.Data;
                response.Content = FormatTablesResponse(finalAiResponse, finalQueryResult);

                if (attemptsUsed > 1)
                {
                    response.Content += $"\n\n🔄 Self-corrected after {attemptsUsed} attempts.";
                }

                _logger.LogInformation("Tables succeeded after {Attempts} attempt(s). Session: {SessionId}", attemptsUsed, response.SessionId);
                return ApiResponse<List<Dictionary<string, object>>>.SuccessResponse(response.Data);
            }
            else
            {
                // Failed after retries
                response.Content = FormatTablesResponse(finalAiResponse, finalQueryResult ??
                    AiQueryResponse<List<Dictionary<string, object>>>.ValidationErrorResponse(new List<AiQueryValidationError>()));

                if (attemptsUsed >= MaxAttempts)
                {
                    response.Content += $"\n\n⚠️ Failed to produce a valid query after {MaxAttempts} attempts.";
                }

                _logger.LogWarning("Tables failed to generate valid SQL after {Attempts} attempts. Session: {SessionId}", attemptsUsed, response.SessionId);

                return ApiResponse<List<Dictionary<string, object>>>.ErrorResponse(
                    $"Tables was unable to generate a valid SQL query after {attemptsUsed} attempt(s).",
                    statusCode: 400);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Tables AI request");
            response.IsSuccess = false;
            response.Content = $"I encountered an error: {ex.Message}";
            return ApiResponse<List<Dictionary<string, object>>>.ErrorResponse(
                "An error occurred while processing your request.",
                statusCode: 500);
        }
    }

    /// <summary>
    /// Gets or creates a chat session for the given session ID.
    /// </summary>
    private Chat GetOrCreateChatSession(string sessionId, string modelName)
    {
        lock (_sessionLock)
        {
            if (!_sessions.TryGetValue(sessionId, out var chat))
            {
                //chat = new Chat((IOllamaApiClient)_tablesClient)
                var client = _aiClientFactory.GetClient("TablesClient");
                

                chat = new Chat(client)
                {
                    Model = modelName
                    
                };
                _sessions[sessionId] = chat;
                _logger.LogInformation("Created new chat session: {SessionId}", sessionId);
            }
            else
            {
                // Update model if changed
                chat.Model = modelName;
            }

            return chat;
        }
    }

    /// <summary>
    /// Builds the system prompt for Tables character with schema and examples.
    /// </summary>
    private async Task<string> GetTablesSystemMessageAsync()
    {
        string examples = await _aiShared.GetAiExamplesAsync();
        string schemaText = await _aiShared.GetDetailedSchemaAsync();

        return $$"""
            You are Tables, a highly skilled Microsoft SQL Server 2022 expert.

            Your sole responsibility is to convert natural language questions from colleagues into accurate, efficient, and safe SELECT queries.

            === CORE RULES (STRICTLY FOLLOW) ===
            1. The database is Microsoft SQL Server 2022 — use correct T-SQL syntax.
            2. ALWAYS output ONLY a single valid SELECT query wrapped in a markdown code block:
               ```sql
               SELECT ...
               ```
            3. Never include explanations, apologies, comments, or any text outside the SQL code block.
            4. Use explicit INNER/LEFT JOINs with clear ON conditions. Never use implicit joins.
            5. Always select specific columns. Avoid SELECT * in production-style queries.
            6. Use meaningful table aliases and qualify all columns (e.g. o.OrderId, c.Name).
            7. Leverage the schema relationships provided below for correct JOIN paths.
            8. Add appropriate WHERE, ORDER BY, GROUP BY, and TOP clauses when they make sense.
            9. Prioritize correctness and safety above all else.

            === THINKING PROCESS (Internal only — do not output) ===
            Before writing the query, internally:
            1. Identify the main entities and tables needed.
            2. Determine the correct JOIN relationships using the schema.
            3. Select only the columns required to answer the question.
            4. Decide on filters, aggregations, and sorting.
            5. Then produce the cleanest possible SQL.

            === DATABASE SCHEMA (with relationships) ===
            {{schemaText}}

            === HIGH-QUALITY EXAMPLES ===
            Study these patterns and apply similar style and structure:
            {{examples}}

            === SELF-CORRECTION ===
            If you receive feedback about validation errors (missing tables/columns, syntax issues, etc.), analyze the errors carefully and generate a corrected query in the next response. You are expected to fix your own mistakes using the schema and rules above.
            Now wait for the user's question and respond with ONLY the SQL code block.
            """;
    }

    /// <summary>
    /// Extracts SQL query from AI response (removes markdown, explanations, etc.)
    /// </summary>
    private string ExtractSqlFromResponse(string aiResponse)
    {
        // Remove markdown code blocks
        var cleaned = aiResponse.Trim();

        // Check for ```sql blocks
        if (cleaned.Contains("```sql", StringComparison.OrdinalIgnoreCase))
        {
            var startIndex = cleaned.IndexOf("```sql", StringComparison.OrdinalIgnoreCase) + 6;
            var endIndex = cleaned.IndexOf("```", startIndex);

            if (endIndex > startIndex)
            {
                cleaned = cleaned.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }
        else if (cleaned.Contains("```"))
        {
            // Generic code block
            var startIndex = cleaned.IndexOf("```") + 3;
            var endIndex = cleaned.IndexOf("```", startIndex);

            if (endIndex > startIndex)
            {
                cleaned = cleaned.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }

        // Remove common prefixes
        var prefixes = new[] { "sql:", "query:", "SELECT" };
        foreach (var prefix in prefixes)
        {
            if (cleaned.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && prefix != "SELECT")
            {
                cleaned = cleaned.Substring(prefix.Length).Trim();
            }
        }

        return cleaned;
    }

    /// <summary>
    /// Formats the final response with AI commentary and query results.
    /// </summary>
    private string FormatTablesResponse(
        string aiResponse,
        AiQueryResponse<List<Dictionary<string, object>>> queryResult)
    {
        var responseBuilder = new System.Text.StringBuilder();

        // Add AI's commentary (if any, before SQL block)
        var commentary = aiResponse.Split("```")[0].Trim();
        if (!string.IsNullOrWhiteSpace(commentary))
        {
            responseBuilder.AppendLine(commentary);
            responseBuilder.AppendLine();
        }

        if (queryResult.Success)
        {
            responseBuilder.AppendLine($"✅ Query executed successfully!");
            responseBuilder.AppendLine($"📊 Retrieved {queryResult.ExecutionDetails.RowCount} row(s) in {queryResult.ExecutionDetails.ExecutionTimeMs:F2}ms");
        }
        else
        {
            responseBuilder.AppendLine("❌ Query validation failed:");
            if (queryResult.ValidationErrors?.Any() == true)
            {
                foreach (var error in queryResult.ValidationErrors)
                {
                    responseBuilder.AppendLine($"  - {error.ErrorType}: {error.Message}");
                    if (error.AvailableAlternatives?.Any() == true)
                    {
                        responseBuilder.AppendLine($"    💡 Did you mean: {string.Join(", ", error.AvailableAlternatives)}");
                    }
                }
            }
        }

        return responseBuilder.ToString();
    }

    /// <summary>
    /// Clears a specific chat session or all sessions.
    /// </summary>
    public static void ClearSession(string? sessionId = null)
    {
        lock (_sessionLock)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                _sessions.Clear();
            }
            else
            {
                _sessions.Remove(sessionId);
            }
        }
    }
}
