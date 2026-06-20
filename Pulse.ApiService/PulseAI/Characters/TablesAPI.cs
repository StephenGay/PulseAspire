using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using Pulse.ApiService.Hubs;
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
    private readonly IHubContext<MessageHub> _hubContext;
    // Session management - stores chat history per session
    private static readonly Dictionary<string, Chat> _sessions = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object _sessionLock = new object();
    private const string DefaultModel = "gpt-oss:latest";

    public TablesAPI(
        AiShared aiShared,
        IPulseAiClientFactory aiClientFactory,
        //HttpClient tablesClient,
        IDbContextFactory<PulseDbContext> dbFactory,
        IHubContext<MessageHub> hubContext,
        ILogger<TablesAPI> logger)
    {
        _aiShared = aiShared;
        _aiClientFactory = aiClientFactory;
        //_tablesClient = tablesClient;
        _dbFactory = dbFactory;
        _hubContext = hubContext;
        _logger = logger;

    }

    /// <summary>
    /// Main entry point for asking Tables a question.
    /// </summary>
    /// <summary>
    /// Main entry point for asking Tables a question.
    /// Includes self-correction retry logic on validation failures.
    /// </summary>
    public async Task<ApiResponse<List<Dictionary<string, object>>>> AskTablesAsync(TablesRequest request)
    {
        var response = new TablesResponse
        {
            ModelUsed = request.ModelName == "Default" ? DefaultModel : request.ModelName,
            SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow
        };

        const int MaxAttempts = 5;

        try
        {
            _logger.LogInformation("Tables AI request: {Question}, Session: {SessionId}",
                request.UserRequest, response.SessionId);

            var chat = GetOrCreateChatSession(response.SessionId, response.ModelUsed);
            string currentPrompt = request.UserRequest;
            string errMsg = string.Empty;

            if (request.UserId == "Flapper")
            {
                await SendProgressAsync(response.SessionId, "FlapperStart", $"Hey Tables, can you get this data for me?\n{currentPrompt}\n");
                errMsg = $"<strong>Hey Flapper!</strong>\nNo problem, just warming up the brain cells...";
            }
            else
            {
                errMsg = $"<strong>Request Received:</strong>\nI have your request right here, just warming up the brain cells...";
            }

            await SendProgressAsync(response.SessionId, "Received", errMsg);
            if (chat.Messages.Count == 0)
            {
                string systemPrompt = await GetTablesSystemMessageAsync();
                await foreach (var chunk in chat.SendAsync(systemPrompt, CancellationToken.None)) { }
                _logger.LogInformation("System prompt initialized for session {SessionId}", response.SessionId);
            }
            
            

            

            
            string finalSqlQuery = string.Empty;
            string finalAiResponse = string.Empty;
            AiQueryResponse<List<Dictionary<string, object>>>? finalQueryResult = null;
            int attemptsUsed = 0;
            
            

            for (attemptsUsed = 1; attemptsUsed <= MaxAttempts; attemptsUsed++)
            {
                var tMsg = string.Empty;
                if(attemptsUsed == 1)
                {
                    tMsg = "\n<strong>Generating SQL Statement:</strong>";
                }
                else
                {
                    tMsg = $"<span style=\"color: rgb(255, 0, 0);\"><strong>FAILED</strong></span>\n<span style=\"font-style: italic;\">{errMsg}</span>\n\n";
                }

                switch (attemptsUsed)
                {
                    case 1:
                        tMsg = "Ok, watch me work my magic and generate the perfect SQL Statement..." + tMsg;
                        break;
                    case 2:
                        tMsg = tMsg + "Ummm, well that didn't work...Let me check why and try again...\n<strong>Generating SQL Statement:</strong>\n";
                        break;
                    case MaxAttempts:
                        tMsg = tMsg + "This is the last time I am permitted to try, hold thumbs...\n<strong>Generating SQL Statement:</strong>\n";
                        break;
                    default:
                        tMsg = tMsg + $"Trying Again (Attempt {attemptsUsed}/{MaxAttempts})...\n<strong>Generating SQL Statement:</strong>\n";
                        break;
                }

                await SendProgressAsync(response.SessionId, "Generating", $"{tMsg}");


                errMsg = string.Empty;
                var sqlBuilder = new StringBuilder();
                await foreach (var chunk in chat.SendAsync(currentPrompt, CancellationToken.None))
                {
                    sqlBuilder.Append(chunk);
                }
                finalAiResponse = sqlBuilder.ToString();

                var sqlQuery = ExtractSqlFromResponse(finalAiResponse);

                if (string.IsNullOrWhiteSpace(sqlQuery))
                {
                    errMsg = "A SQL Statement could not be extracted from the response you sent.";
                    //await SendProgressAsync(response.SessionId, "Failed", "Failed to extract SQL from response.", attemptsUsed);
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
                    break;
                }

                _logger.LogInformation("Attempt {Attempt} - Generated SQL: {Sql}", attemptsUsed, sqlQuery);
                tMsg = $"SQL Statement generated.\n\n<strong>Validate & Execute:</strong>\nValidating the generated SQL Statement against the database schema to ensure columns and fields are valid.\n";
                tMsg += $"If invalid, self correct and start the procedure again.\n";
                tMsg += $"If valid, execute the query and return the results.\n";
                await SendProgressAsync(response.SessionId, "Validating",
                    tMsg, attemptsUsed, sqlQuery);

                await using var dbContext = await _dbFactory.CreateDbContextAsync();
                var connectionString = dbContext.Database.GetConnectionString();
                var tablesSQL = new TablesSQL(null, dbContext);

                finalQueryResult = await tablesSQL.ExecuteValidatedQuery(connectionString, sqlQuery);

                if (finalQueryResult.Success || (finalQueryResult.ValidationErrors == null || !finalQueryResult.ValidationErrors.Any()))
                {
                    finalSqlQuery = sqlQuery;
                    await SendProgressAsync(response.SessionId, "Success",
                        $"Query executed successfully. Retrieved {finalQueryResult.ExecutionDetails?.RowCount ?? 0} row(s).",
                        attemptsUsed, finalSqlQuery, null);
                    break;
                }

                // Self-correction path
                var errorSummary = string.Join(" | ", finalQueryResult.ValidationErrors!.Select(e => $"{e.ErrorType}: {e.Message}"));
                errMsg = errorSummary;
                await SendProgressAsync(response.SessionId, "SelfCorrecting",
                    $"Validation failed. Self-correcting...", attemptsUsed, null, errorSummary);

                currentPrompt = $"The previous SQL had these validation errors: {errorSummary}. Please provide a corrected SQL query. Output ONLY the SQL in a ```sql block.";
            }

            // Final response building (same as before, plus AttemptsUsed)
            response.AttemptsUsed = attemptsUsed;
            response.GeneratedSql = finalSqlQuery;
            response.IsSuccess = finalQueryResult?.Success ?? false;
            response.ValidationErrors = finalQueryResult?.ValidationErrors;
            response.ExecutionDetails = finalQueryResult?.ExecutionDetails;

            if (!string.IsNullOrWhiteSpace(finalSqlQuery) && finalQueryResult?.Success == true)
            {
                response.Data = finalQueryResult.Data;
                response.Content = FormatTablesResponse(finalAiResponse, finalQueryResult);
                if (attemptsUsed > 1) response.Content += $"\n\n🔄 Self-corrected after {attemptsUsed} attempts.";

                return ApiResponse<List<Dictionary<string, object>>>.SuccessResponse(response.Data);
            }

            return ApiResponse<List<Dictionary<string, object>>>.ErrorResponse(
                $"Tables was unable to generate a valid SQL query after {attemptsUsed} attempt(s).",
                statusCode: 400);
        }
        catch (Exception ex)
        {
            await SendProgressAsync(response.SessionId, "Failed", $"Error: {ex.Message}");
            _logger.LogError(ex, "Error processing Tables AI request");
            response.IsSuccess = false;
            response.Content = $"I encountered an error: {ex.Message}";
            return ApiResponse<List<Dictionary<string, object>>>.ErrorResponse("An error occurred.", statusCode: 500);
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
                _logger.LogInformation("Chat session found: {SessionId}", sessionId);
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

    private async Task SendProgressAsync(
    string sessionId,
    string status,
    string message,
    int attempt = 1,
    string? generatedSql = null,
    string? errorSummary = null)
    {
        if (string.IsNullOrWhiteSpace(sessionId)) return;

        var update = new TablesProgressUpdate(
            SessionId: sessionId,
            Status: status,
            Message: message,
            CurrentAttempt: attempt,
            MaxAttempts: 3,
            GeneratedSql: generatedSql,
            ErrorSummary: errorSummary
        );

        try
        {
            await _hubContext.Clients.Group(sessionId)
                .SendAsync("TablesProgressUpdate", update);

            _logger.LogDebug("Tables progress → {Status} (Session: {SessionId}, Attempt: {Attempt})",
                status, sessionId, attempt);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send Tables SignalR progress for session {SessionId}", sessionId);
        }
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
