// Pulse.ApiService/PulseAI/Characters/FlapperAPI.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using Pulse.ApiService.Hubs;
using Pulse.ApiService.PulseAI.Services;
using Pulse.Models.AI;
using Pulse.Models.AI.Flapper;
using Emojis = Microsoft.FluentUI.AspNetCore.Components.Emojis;
using Pulse.Models.Communication;
using Pulse.Models.CustomComponents;
using Pulse.Models.PulseContext;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Pulse.Models.Api.ApiEndpoints.PulseAi;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Pulse.ApiService.PulseAI.Characters;


public class FlapperOllamaAPI
{
    private readonly IOllamaApiClient _chatClient;
    private readonly ILogger<FlapperOllamaAPI> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    //private readonly Flapper flapper = new();

    private readonly string _ollamaApiKey;
    private readonly Uri _localBaseAddress;
    private readonly Uri _cloudBaseAddress;
    private readonly JsonSerializerOptions _jsonOptions;

    public FlapperOllamaAPI(IOllamaApiClient chatClient, ILogger<FlapperOllamaAPI> logger, IHttpClientFactory httpClientFactory)
    {
        _chatClient = chatClient;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _ollamaApiKey = "76c15b3643ba413aac7429bbb64e119a._BO65S6KcVBYl9PpsPwSc5WI"; // configuration["OllamaApi:Key"] ?? string.Empty;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        // Overall timeout

    }

    public async Task<OllamaWebFetchResponse?> FetchWebPage(object searchFor, CancellationToken ct = default)
    {
        var searchUrl = "https://ollama.com/api/web_fetch";


        ArgumentNullException.ThrowIfNull(searchFor);

        try
        {
            
            using var request = new HttpRequestMessage(HttpMethod.Post, searchUrl);
            var jsonPayload = JsonSerializer.Serialize(searchFor, _jsonOptions);
            request.Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
            if (!string.IsNullOrEmpty(_ollamaApiKey))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ollamaApiKey);
                _logger.LogDebug("Added API key header for cloud endpoint");
            }
            else
            {
                _logger.LogWarning("Cloud endpoint requested but no API key configured");
            }

            _logger.LogDebug("POST request to {RequestUri}", searchUrl);
            using var flapperClient = _httpClientFactory.CreateClient("FlapperApiClient");
            using var response = await flapperClient.SendAsync(request, ct);

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync(ct);

            return JsonSerializer.Deserialize<OllamaWebFetchResponse>(responseContent, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for {RequestUri}: {StatusCode}",
                searchUrl, ex.StatusCode);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response from {RequestUri}", searchUrl);
            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Request to {RequestUri} was cancelled", searchUrl);
            throw;
        }
    }

    public async Task<OllamaWebSearchResponse?> SearchWeb(object searchFor, CancellationToken ct = default)
    {
        var searchUrl = "https://ollama.com/api/web_search";

        
        ArgumentNullException.ThrowIfNull(searchFor);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, searchUrl);
            var jsonPayload = JsonSerializer.Serialize(searchFor, _jsonOptions);
            request.Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
            if (!string.IsNullOrEmpty(_ollamaApiKey))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ollamaApiKey);
                _logger.LogDebug("Added API key header for cloud endpoint");
            }
            else
            {
                _logger.LogWarning("Cloud endpoint requested but no API key configured");
            }

            _logger.LogDebug("POST request to {RequestUri}", searchUrl);
            using var flapperClient = _httpClientFactory.CreateClient("FlapperApiClient");
            using var response = await flapperClient.SendAsync(request, ct);

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync(ct);

            return JsonSerializer.Deserialize<OllamaWebSearchResponse>(responseContent, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for {RequestUri}: {StatusCode}",
                searchUrl, ex.StatusCode);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response from {RequestUri}", searchUrl);
            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Request to {RequestUri} was cancelled", searchUrl);
            throw;
        }
    }
    

    public async Task<FlapperResponse> OllamaProcessWithToolsAsync(
        FlapperConversation conv,
        FlapperDTO flapperDTO,
        string userMessage,

        List<object> tools,
        Func<string, Task> onChunkReceived,
        CancellationToken ct = default)
    {
        try
        {
            // Build conversation history
            var history = conv.Messages.Select(m => new OllamaMessage
            {
                // If the message is from a tool, we want to preserve that role for Ollama to understand
                // Otherwise, we map "User" → "user" and everything else (Flapper) → "assistant"
                Role = m.Sender == "User" ? "user" : (m.Sender == "tool" ? "tool" : "assistant"),
                Content = m.Content,
                 ToolName = m.ToolName // Include tool name if this message is a tool response
            }).ToList();

            var messages = new List<OllamaMessage>();
            //{
            //    new OllamaMessage { Role = "system", Content = systemPrompt }
            //};
            messages.AddRange(history);
            messages.Add(new OllamaMessage { Role = "user", Content = userMessage });

            
            var request = new ChatRequest
            {
                Model = flapperDTO.Model,
                Messages = messages.Select(m => new OllamaSharp.Models.Chat.Message
                {
                    Role = m.Role switch
                    {
                        "system" => OllamaSharp.Models.Chat.ChatRole.System,
                        "user" => OllamaSharp.Models.Chat.ChatRole.User,
                        "assistant" => OllamaSharp.Models.Chat.ChatRole.Assistant,
                        "tool" => OllamaSharp.Models.Chat.ChatRole.Tool, // Custom role for tools
                        _ => OllamaSharp.Models.Chat.ChatRole.User
                    },
                    Content = m.Content ?? string.Empty
                }).ToList(),
                KeepAlive = "-1m",
                Think = flapperDTO.Think,
                Tools = tools, // Note: Ollama expects tools in the request body, not just system prompt
                Options = new OllamaSharp.Models.RequestOptions
                {
                    Temperature = (float?)flapperDTO.Options.Temperature,
                    NumCtx = flapperDTO.Options.NumCtx,
                    NumPredict = flapperDTO.Options.NumPredict,
                    RepeatPenalty = (float?)flapperDTO.Options.RepeatPenalty,
                    PresencePenalty = (float?)flapperDTO.Options.PresencePenalty
                    // Add any other options you want to set globally
                }
            };
            var fullResponse = new StringBuilder();
            bool inThinking = false;
            List<FlapperToolCall> toolCalls = new();

            await foreach (var chunk in _chatClient.ChatAsync(request).WithCancellation(ct))
            {
                if (!string.IsNullOrEmpty(chunk?.Message?.Content))
                {
                    var content = chunk.Message.Content;
                    if(inThinking)
                    {
                        content = $"</thinking>{content}";
                        inThinking = false;
                    }
                    fullResponse.Append(content);
                    await onChunkReceived(content);   // Send chunk to client
                }
                else if (chunk?.Message?.ToolCalls != null && chunk.Message.ToolCalls.Any())
                {
                    // For simplicity, we append tool calls as JSON strings in the thinking stream
                    var toolInfo = JsonSerializer.Serialize(chunk.Message.ToolCalls);
                    foreach (var tc in chunk.Message.ToolCalls)
                    {
                        toolCalls.Add(new FlapperToolCall 
                        {
                            ToolName = tc.Function.Name,
                            Parameters = (Dictionary<string, object>)tc.Function.Arguments 
                        });
                    }
                        
                }
                else if (!string.IsNullOrEmpty(chunk?.Message?.Thinking))
                {
                    var thinkingText = chunk.Message.Thinking;
                    if (!inThinking)
                    {
                        inThinking = true;
                        thinkingText = $"<thinking>Thinking:\n {thinkingText}";
                    }
                    fullResponse.Append(thinkingText);
                    await onChunkReceived(thinkingText);
                }
            }

            var rawReply = fullResponse.ToString().Trim();
            //// Extract thinking (if Flapper used <thinking> tags)
            var thinking = ExtractBetween(rawReply, "<thinking>", "</thinking>");
            var finalContent = rawReply
                .Replace($"<thinking>{thinking}</thinking>", "")
                .Trim();

            return new FlapperResponse
            {
                Success = true,
                Thinking = string.IsNullOrWhiteSpace(thinking) ? null : thinking,
                ToolCalls = toolCalls.Count > 0 ? toolCalls : null,
                Content = finalContent,
                RequiresClarification = false,
                RawContent = rawReply
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flapper ProcessWithToolsAsync failed for user message: {Message}", userMessage);
            return new FlapperResponse
            {
                Success = false,
                Content = "Sorry, I encountered an error processing your request."
            };
        }
    }

    public List<object> BuildFlapperOllamaTools()
    {
        var tools = new List<object>();

        tools.Add(new
        {
            type = "function",
            function = new
            {
                name = ToolNames.WebSearch,
                description = "Search the web for current information.",
                parameters = new
                {
                    type = "object",
                    properties = new { query = new { type = "string", description = "Search query" } },
                    required = new[] { "query" }
                }
            }
        });

        tools.Add(new
        {
            type = "function",
            function = new
            {
                name = ToolNames.WebFetch,
                description = "Fetch full content from a URL.",
                parameters = new
                {
                    type = "object",
                    properties = new { url = new { type = "string", description = "URL to fetch" } },
                    required = new[] { "url" }
                }
            }
        });

        tools.Add(new
        {
            type = "function",
            function = new
            {
                name = ToolNames.AskTables,
                description = "Ask Tables for information from the database.",
                parameters = new
                {
                    type = "object",
                    properties = new { query = new { type = "string", description = "NL query" } },
                    required = new[] { "query" }
                }
            }
        });

        tools.Add(new
        {
            type = "function",
            function = new
            {
                name = ToolNames.AskUser,
                description = "Ask the user a Yes/No question.",
                parameters = new
                {
                    type = "object",
                    properties = new { question = new { type = "string", description = "NL question" } },
                    required = new[] { "question" }
                }
            }
        });

        tools.Add(new
        {
            type = "function",
            function = new
            {
                name = ToolNames.ExecuteSql,
                description = "Execute a SELECT SQL query against the dbPulse database and return results.",
                parameters = new
                {
                    type = "object",
                    properties = new { sql = new { type = "string", description = "The SQL query to execute" } },
                    required = new[] { "sql" }
                }
            }
        });

        //if (AdminMode)
        //{
        //    tools.Add(new
        //    {
        //        type = "function",
        //        function = new
        //        {
        //            name = ToolNames.ExecuteActionSql,
        //            description = "Execute an UPDATE, INSERT or DELETE SQL query against the dbPulse database and return results.",
        //            parameters = new
        //            {
        //                type = "object",
        //                properties = new { sql = new { type = "string", description = "The SQL query to execute" } },
        //                required = new[] { "sql" }
        //            }
        //        }
        //    });
        //}

        return tools;
    }

    /// <summary>
    /// Extracts content between two tags.
    /// </summary>
    private string ExtractBetween(string text, string startTag, string endTag)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var startIndex = text.IndexOf(startTag, StringComparison.Ordinal);
        if (startIndex == -1)
            return string.Empty;

        startIndex += startTag.Length;

        var endIndex = text.IndexOf(endTag, startIndex, StringComparison.Ordinal);
        if (endIndex == -1)
            return string.Empty;

        return text.Substring(startIndex, endIndex - startIndex).Trim();
    }

    /// <summary>
    /// Parses tool parameters from raw JSON string.
    /// </summary>
    private Dictionary<string, object> ParseToolParameters(string rawArgs)
    {
        try
        {
            if (string.IsNullOrEmpty(rawArgs))
                return new Dictionary<string, object>();

            return JsonSerializer.Deserialize<Dictionary<string, object>>(rawArgs)
                ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse tool parameters");
            return new Dictionary<string, object>();
        }
    }

    
}

#region Old Code

// public async Task<IResult> FlapperOllamaChat(
//    FlapperChatRequest req,
//    AiShared _aiShared,
//    FlapperAPI flapperApi,
//    TablesAPI tablesApi,           // Your SQL expert
//    PulseDbContext db,
//    IHubContext<MessageHub> hubContext)
// {
//    // 1. Load or create conversation
//    var conversation = db.FlapperConversations
//        .Where(c => c.Id == req.ConversationId && c.UserId == req.UserId)
//        .FirstOrDefault();

//    if (conversation == null)
//    {
//        conversation = new FlapperConversation
//        {
//            Id = req.ConversationId,
//            UserId = req.UserId,
//            Title = "Flapper Chat"
//        };
//        db.FlapperConversations.Add(conversation);
//    }

//    // 2. Save user message
//    db.FlapperMessages.Add(new FlapperMessage
//    {
//        ConversationId = conversation.Id,
//        Sender = "User",
//        Content = req.Message,
//        SentAt = DateTime.UtcNow
//    });
//    await db.SaveChangesAsync();

//    conversation.LastActivity = DateTime.UtcNow;

//    // 3. Build rich system prompt (with schema)
//    var systemMessageData = new SystemMessageData
//    {
//        CompanyName = "H&M Rollers",
//        CompanyInformation = AiPromptHelperService.GetTempCoInfo(),
//        UserName = req.PreferredUserName,
//        DbSchema = await _aiShared.GetDetailedSchemaAsync(),
//        // Add any other fields your BuildSystemMessagev2 needs
//    };

//    var systemPrompt = flapper.BuildSystemMessagev2(systemMessageData);

//    // 4. Build available tools
//    var tools = BuildFlapperOllamaTools();

//    // 5. Multi-turn reasoning loop (Flapper can call tools multiple times)
//    const int MaxTurns = 6;
//    int turn = 0;
//    FlapperResponse result;

//    do
//    {
//        turn++;

//        result = await OllamaProcessWithToolsAsync(
//            conversation,
//            req.Message,
//            systemPrompt,
//            tools, async chunk =>
//            {
//                // Stream thinking/content chunks to UI in real-time
//                var streamMessage = new PulseMessage
//                {
//                    SenderUserName = "Flapper",
//                    RecipientUserId = req.UserId,
//                    Role = "Flapper",
//                    Subject = "Flapper is thinking...",
//                    ContentType = "HTML",
//                    Content = chunk,
//                    SentAt = DateTime.UtcNow
//                };
//                await hubContext.Clients.User(req.UserId.ToString())
//                    .SendAsync("ReceiveMessage", streamMessage);
//            });

//        // If no tool calls → we're done
//        if (result.ToolCalls == null || !result.ToolCalls.Any())
//            break;

//        // Execute each tool call
//        foreach (var toolCall in result.ToolCalls)
//        {
//            string toolResult = toolCall.ToolName switch
//            {
//                "AskTables" => await ExecuteAskTablesAsync(tablesApi, toolCall.Parameters, req.Message),
//                "WebSearch" => await ExecuteWebSearchAsync(toolCall.Parameters),
//                "WebFetch" => await ExecuteWebFetchAsync(toolCall.Parameters),
//                _ => $"Unknown tool: {toolCall.ToolName}"
//            };

//            // Save tool result back into conversation so Flapper can reason again
//            db.FlapperMessages.Add(new FlapperMessage
//            {
//                ConversationId = conversation.Id,
//                Sender = "tool",
//                Content = toolResult,
//                SentAt = DateTime.UtcNow,
//                ToolName = toolCall.ToolName
//            });
//        }

//        await db.SaveChangesAsync();

//    } while (turn < MaxTurns);

//    // 6. Save Flapper's final response
//    db.FlapperMessages.Add(new FlapperMessage
//    {
//        ConversationId = conversation.Id,
//        Sender = "Flapper",
//        Content = result.Content ?? result.Thinking ?? "No response generated.",
//        SentAt = DateTime.UtcNow,
//        ContentType = "HTML"
//    });

//    await db.SaveChangesAsync();

//    // 7. Send final message to UI via your existing hub
//    var finalMessage = new PulseMessage
//    {
//        SenderUserName = "Flapper",
//        RecipientUserId = req.UserId,
//        Role = "Flapper",
//        Subject = "Flapper Reply",
//        ContentType = "HTML",
//        Content = result.Content ?? result.Thinking ?? "",
//        SentAt = DateTime.UtcNow
//    };

//    await hubContext.Clients.User(req.UserId.ToString())
//        .SendAsync("ReceiveMessage", finalMessage);

//    return Results.Ok(result);
// }

/// <summary>
/// Processes a user message with tool support using the chat client.
/// </summary>
// private static async Task<string> ExecuteAskTablesAsync(TablesAPI tablesApi, Dictionary<string, object> parameters, string originalQuery)
// {
//    if (!parameters.TryGetValue("query", out var queryObj) || queryObj is not string query)
//        return "Error: Missing query parameter";

//    //Call your existing TablesAPI
//    PulseAiRequest tablesRequest = new PulseAiRequest
//    {
//        UserId = "Flapper", // You can pass actual user ID if needed for logging
//        AiName = AiName.Tables,
//        UserRequest = new OllamaChatMessage { Content = query },
//        UserEmail = "flapper@example.com",
//        ModelName = "gpt-oss:latest"
//    };
//    var result = await tablesApi.AskTablesAsync(tablesRequest);

//    return result.Success
//        ? $"Database query results for '{query}':\n{result.Data}"
//        : $"Tables error: {result.Message}";
// }

/// <summary>
/// Processes a user message with tool support using the chat client.
/// </summary>
// private static async Task<string> ExecuteWebSearchAsync(Dictionary<string, object> parameters)
// {
//    if (!parameters.TryGetValue("query", out var queryObj) || queryObj is not string query)
//        return "Error: Missing 'query' argument";

//    if (string.IsNullOrEmpty(query))
//        return "Error: Query is empty";

//    //Placeholder implementation -integrate with your actual web search service
//    //For now, return a stub response
//    return $"Web search results for '{query}': [Integration pending with external search service]";
// }

/// <summary>
/// Processes a user message with tool support using the chat client.
/// </summary>
// private static async Task<string> ExecuteWebFetchAsync(Dictionary<string, object> parameters)
// {
//    if (!parameters.TryGetValue("url", out var urlObj) || urlObj is not string url)
//        return "Error: Missing 'url' argument";

//    if (string.IsNullOrEmpty(url))
//        return "Error: URL is empty";

//    //Placeholder implementation -integrate with your actual web fetch service
//    return $"Web content from '{url}': [Integration pending with external fetch service]";
// }

/// <summary>
/// Processes a user message with tool support using the chat client.
/// </summary>
//public async Task<FlapperResponse> ProcessWithToolsAsync(
//    FlapperConversation conv,
//    string userMessage,
//    string systemPrompt,
//    List<object> tools,
//    CancellationToken ct = default)
//{
//    try
//    {
//        // Build message history
//        var messages = new List<ChatMessage>();

//        // Add system prompt
//        messages.Add(new ChatMessage(ChatRole.System, systemPrompt));

//        // Add conversation history
//        foreach (var msg in conv.Messages)
//        {
//            var role = msg.Sender == "User" ? ChatRole.User : ChatRole.Assistant;
//            messages.Add(new ChatMessage(role, msg.Content));
//        }

//        // Add current user message
//        messages.Add(new ChatMessage(ChatRole.User, userMessage));

//        // Create options with tools
//        var options = new ChatOptions
//        {
//            ModelId = "gpt-oss:latest",
//            Temperature = 0.2f,
//            MaxOutputTokens = 2000
//        };

//        // Note: Tools are included in the system prompt context rather than ChatOptions.Tools
//        // to allow flexible tool descriptions without requiring AITool/AIFunction implementations

//        _logger.LogInformation("Processing message with {ToolCount} tools", tools?.Count() ?? 0);

//        // ✅ Correct method for IChatClient (.NET 10)
//        var response = await _chatClient.GetResponseAsync<FlapperResponse>(
//            messages: messages,
//            options: options,
//            cancellationToken: ct);

//        if (response is null )
//        {
//            return new FlapperResponse
//            {
//                Success = false,
//                Content = "No response from the AI model."
//            };
//        }

//        var completion = response.Result;// .Completions[0];
//        var content = completion.Content ?? string.Empty;

//        // Extract thinking blocks
//        var thinking = ExtractBetween(content, "<thinking>", "</thinking>");

//        // Extract tool calls from the response
//        var toolCalls = new List<FlapperToolCall>();

//        if (completion.ToolCalls is not null)
//        {
//            foreach (var toolCall in completion.ToolCalls)
//            {
//                toolCalls.Add(new FlapperToolCall
//                {
//                    ToolName = toolCall.ToolName,
//                    Parameters = ParseToolParameters(toolCall.Parameters.ToString() ?? string.Empty)
//                });
//            }
//        }

//        // Clean up content
//        //var finalContent = content
//        //    .Replace($"<thinking>{thinking}</thinking>", "", StringComparison.Ordinal);
//            //.Trim();

//        return new FlapperResponse
//        {
//            Success = true,
//            Thinking = string.IsNullOrWhiteSpace(thinking) ? null : thinking,
//            ToolCalls = toolCalls.Any() ? toolCalls : null,
//            Content = content, // finalContent,
//            RequiresClarification = false,
//            RawContent = content
//        };
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Flapper ProcessWithToolsAsync failed");
//        return new FlapperResponse
//        {
//            Success = false,
//            Content = "Sorry, I encountered an error processing your request."
//        };
//    }
//}

/// <summary>
/// Simple streaming version for when you need real-time responses.
/// </summary>
//public async IAsyncEnumerable<string> ProcessStreamingAsync(
//    List<ChatMessage> messages,
//    ChatOptions? options = null,
//    CancellationToken ct = default)
//{

//        options ??= new ChatOptions { Temperature = 0.2f };

//        //await foreach (var chunk in _chatClient.GetChatCompletionAsStreamAsync(messages, options, ct))
//        //{
//        //    if (!string.IsNullOrEmpty(chunk.Content))
//        //    {
//        //        yield return chunk.Content;
//        //    }
//        //}

//}

#endregion