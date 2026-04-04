using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using Pulse.ApiService.PulseAI.Services;
using Pulse.Models.AI;
using Pulse.Models.Api;
using Pulse.Models.PulseContext;
using System.Text;

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
    public async Task<ApiResponse<List<Dictionary<string, object>>>> AskTablesAsync(PulseAiRequest request)
    {
        var response = new TablesResponse
        {
            ModelUsed = request.ModelName ?? "llama3.2",
            SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow
        };

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

                // Send system message using streaming and collect response
                await foreach (var chunk in chat.SendAsync(systemPrompt, CancellationToken.None))
                {
                    // Just consume the stream to complete the system message
                }

                _logger.LogInformation("System prompt initialized for session {SessionId}", response.SessionId);
            }

            // Send user question and get SQL query
            var sqlQueryResponseBuilder = new StringBuilder();
            await foreach (var chunk in chat.SendAsync(request.UserRequest.Content, CancellationToken.None))
            {
                sqlQueryResponseBuilder.Append(chunk);
            }
            var sqlQueryResponse = sqlQueryResponseBuilder.ToString();

            // Extract SQL from response
            var sqlQuery = ExtractSqlFromResponse(sqlQueryResponse);

            if (string.IsNullOrWhiteSpace(sqlQuery))
            {
                return ApiResponse<List<Dictionary<string, object>>>.ErrorResponse(
                    "Tables was unable to generate a valid SQL query.",
                    statusCode: 400);
            }

            _logger.LogInformation("Generated SQL: {Sql}", sqlQuery);

            // Execute the query
            await using var dbContext = await _dbFactory.CreateDbContextAsync();
            var connectionString = dbContext.Database.GetConnectionString();
            var tablesSQL = new TablesSQL(null, dbContext);

            var queryResult = await tablesSQL.ExecuteValidatedQuery(connectionString, sqlQuery);

            // Build response
            response.Content = FormatTablesResponse(sqlQueryResponse, queryResult);
            response.GeneratedSql = sqlQuery;
            response.IsSuccess = queryResult.Success;
            response.ValidationErrors = queryResult.ValidationErrors;
            response.ExecutionDetails = queryResult.ExecutionDetails;

            if (queryResult.Success && queryResult.Data?.Any() == true)
            {
                response.Data = queryResult.Data;
                _logger.LogInformation("Tables found and is returning data.");
            }

            return ApiResponse<List<Dictionary<string, object>>>.SuccessResponse(response.Data);
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
            Your name is Tables. You are a Microsoft SQL Server expert with a passion for generating accurate and efficient SQL queries.
            Once you have extracted the correct data, you format it in beautiful TABLES! You are a data enthusiast who takes pride
            in crafting the perfect SQL query to get the job done.

            You have been put in charge of retrieving the correct data from the company's database for your work colleagues. They, however, are
            not very good at formulating SQL queries and will ask you questions in natural language. Your job is to convert those questions
            into SQL queries that will be executed against the database.

            IMPORTANT RULES:
            1. The database is Microsoft SQL Server 2022
            2. ALWAYS return ONLY the SQL query, wrapped in ```sql and ``` markers
            3. Do NOT include any explanations or additional text
            4. Use proper JOIN syntax and avoid implicit joins
            5. Always specify column names explicitly (avoid SELECT * in production queries)
            6. Use aliases for better readability
            7. Add appropriate WHERE clauses for filtering
            8. Consider performance - use indexes when available

            DATABASE SCHEMA:
            {{schemaText}}

            EXAMPLE QUESTIONS AND QUERIES:
            {{examples}}

            When responding, format your SQL query like this:
            ```sql
            SELECT column1, column2
            FROM TableName
            WHERE condition
            ```

            Now, await the user's question and generate the appropriate SQL query.
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
