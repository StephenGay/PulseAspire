using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.Tokens;
using OllamaSharp;
using OllamaSharp.Tools;
using Pulse.ApiService.Hubs;
using Pulse.ApiService.PulseAI.Characters;
using Pulse.ApiService.PulseAI.Services;
using Emojis = Microsoft.FluentUI.AspNetCore.Components.Emojis;
using Pulse.Models.AI;
using Pulse.Models.AI.Flapper;
using Pulse.Models.Communication;
using Pulse.Models.CustomComponents;
using Pulse.Models.PulseContext;
using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static OllamaSharp.Models.Chat.Message;
using Pulse.Models.Api;

namespace Pulse.ApiService.PulseAI.Endpoints;

 //Pulse.ApiService/PulseAI/Endpoints/FlapperEndpoints.cs or in Program.cs
public static class FlapperEndpoints
{
    public static void MapFlapperEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/PulseAI/Flapper").WithTags("FlapperEndpoints");

        group.MapPost("/ChatSession", FlapperChat)
            .WithName("FlapperChat")
            .WithDescription("Send a message to Flapper and receive a response. Supports streaming responses and clarification questions.")
            .Produces<FlapperResponse>(200);

        group.MapPost("/OllamaChatSession", FlapperOllamaChat)
            .WithName("FlapperOllamaChat")
            .WithDescription("Send a message to Flapper and receive a response. Supports streaming responses and clarification questions.")
            .Produces<FlapperResponse>(200);

        group.MapGet("/Conversations/UserHistory/{userId}", GetUserConversationHistory)
            .WithName("GetUserConversationHistory")
            .WithDescription("Get the conversation history for a specific user.")
            .Produces<ApiResponse<List<FlapperConversation>>>(200);

        group.MapGet("/Conversations/GetMessages/{conversationId}", GetConversationMessages)
            .WithName("GetConversationMessages")
            .WithDescription("Get the messages for a specific conversation.")
            .Produces<ApiResponse<List<FlapperMessageDto>>>(200);

    }

    private static async Task<IResult> FlapperChat(
        FlapperChatRequest req,
        AiShared _aiShared,
        FlapperAPI flapperApi,
        TablesAPI tablesApi,           // Your SQL expert
        PulseDbContext db,
        IHubContext<MessageHub> hubContext)
    {
        // 1. Load or create conversation
        var conversation = await db.FlapperConversations
            .FirstOrDefaultAsync(c => c.Id == req.ConversationId && c.UserId == req.UserId);

        if (conversation == null)
        {
            conversation = new FlapperConversation
            {
                Id = req.ConversationId,
                UserId = req.UserId,
                Title = "Flapper Chat"
            };
            db.FlapperConversations.Add(conversation);
        }

        // 2. Save user message
        db.FlapperMessages.Add(new FlapperMessage
        {
            ConversationId = conversation.Id,
            Sender = "User",
            Content = req.Message,
            SentAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        conversation.LastActivity = DateTime.UtcNow;

        // 3. Build rich system prompt (with schema)
        var systemMessageData = new SystemMessageData
        {
            CompanyName = "H&M Rollers",
            CompanyInformation = AiPromptHelperService.GetTempCoInfo(),
            UserName = req.PreferredUserName,
            DbSchema = await _aiShared.GetDetailedSchemaAsync(),
            // Add any other fields your BuildSystemMessagev2 needs
        };

        var systemPrompt = new Flapper().BuildSystemMessagev2(systemMessageData);

        // 4. Build available tools
        var tools = BuildFlapperTools();

        // 5. Multi-turn reasoning loop (Flapper can call tools multiple times)
        const int MaxTurns = 6;
        int turn = 0;
        FlapperResponse result;

        do
        {
            turn++;

            result = await flapperApi.ProcessWithToolsAsync(
                conversation,
                req.Message,
                systemPrompt,
                tools);

            // If no tool calls → we're done
            if (result.ToolCalls == null || !result.ToolCalls.Any())
                break;

            // Execute each tool call
            foreach (var toolCall in result.ToolCalls)
            {
                string toolResult = toolCall.ToolName switch
                {
                    "AskTables" => await ExecuteAskTablesAsync(tablesApi, toolCall.Parameters, req.Message),
                    //"WebSearch" => await ExecuteWebSearchAsync(toolCall.Parameters, flapperOllamaApi),
                    //"WebFetch" => await ExecuteWebFetchAsync(toolCall.Parameters, flapperOllamaApi),
                    _ => $"Unknown tool: {toolCall.ToolName}"
                };

                // Save tool result back into conversation so Flapper can reason again
                db.FlapperMessages.Add(new FlapperMessage
                {
                    ConversationId = conversation.Id,
                    Sender = "tool",
                    Content = toolResult,
                    SentAt = DateTime.UtcNow,
                    ToolName = toolCall.ToolName
                });
            }

            await db.SaveChangesAsync();

        } while (turn < MaxTurns);

        // 6. Save Flapper's final response
        db.FlapperMessages.Add(new FlapperMessage
        {
            ConversationId = conversation.Id,
            Sender = "Flapper",
            Content = result.Content ?? result.Thinking ?? "No response generated.",
            SentAt = DateTime.UtcNow,
            ContentType = "HTML"
        });

        await db.SaveChangesAsync();

        // 7. Send final message to UI via your existing hub
        var finalMessage = new PulseMessage
        {
            SenderUserName = "Flapper",
            RecipientUserId = req.UserId,
            Role = "Flapper",
            Subject = "Flapper Reply",
            ContentType = "HTML",
            Content = result.Content ?? result.Thinking ?? "",
            SentAt = DateTime.UtcNow
        };

        await hubContext.Clients.User(req.UserId.ToString())
            .SendAsync("ReceiveMessage", finalMessage);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetConversationMessages(Guid conversationId, PulseDbContext db)
    {
        var messages = await db.FlapperMessages
            .Where(m => m.ConversationId == conversationId && m.Sender != "tool")
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        var messageDtos = messages.Select(m => new FlapperMessageDto
        {
            Sender = m.Sender,
            Content = m.Content,
            IsThinking = m.Content.StartsWith("<thinking>") ? true : false,
        }).ToList();

        return Results.Ok(new ApiResponse<List<FlapperMessageDto>>(messageDtos));
    }

    // Place this in your FlapperEndpoints.cs or as a static helper method
    // Add this method in FlapperEndpoints.cs or a static helper
    private static IEnumerable<AIFunction> BuildFlapperTools()
    {
        return new List<AIFunction>
        {
            AIFunctionFactory.Create(
                name: "AskTables",
                description: "Use this tool when you need real data from the Pulse database. Provide a clear natural language description of the data required.",
                method: (string query) => Task.FromResult("")
            ),

            AIFunctionFactory.Create(
                name: "WebSearch",
                description: "Search the internet for current information not available in our database.",
                method: (string query) => Task.FromResult("")
            ),

            AIFunctionFactory.Create(
                name: "WebFetch",
                description: "Fetch the full content of a specific webpage.",
                method: (string url) => Task.FromResult("")
            )
        };
    }

    public static async Task<IResult> FlapperOllamaChat(
        FlapperChatRequest req,
        AiShared _aiShared,
        FlapperOllamaAPI flapperOllamaApi,
        TablesAPI tablesApi,           // Your SQL expert
        PulseDbContext db,
        IHubContext<MessageHub> hubContext,
    CancellationToken cancellationToken)
    {
        try
        {


            // 1. Load or create conversation
            var conversation = db.FlapperConversations
                .Where(c => c.Id == req.ConversationId && c.UserId == req.UserId)
                .FirstOrDefault();

            if (conversation == null)
            {
                conversation = new FlapperConversation
                {
                    Id = req.ConversationId,
                    UserId = req.UserId,
                    Title = req.Message.Length > 50 ? req.Message.Substring(0, 50) + "..." : req.Message
                };
                db.FlapperConversations.Add(conversation);
            }

            // 2. Save user message
            db.FlapperMessages.Add(new FlapperMessage
            {
                ConversationId = conversation.Id,
                Sender = "User",
                Content = req.Message,
                SentAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            conversation.LastActivity = DateTime.UtcNow;

            // 3. Build rich system prompt (with schema)
            var systemMessageData = new SystemMessageData
            {
                CompanyName = "H&M Rollers",
                CompanyInformation = AiPromptHelperService.GetTempCoInfo(),
                UserName = req.PreferredUserName,
                DbSchema = await _aiShared.GetDetailedSchemaAsync(),
                UserQuery = req.Message
                // Add any other fields your BuildSystemMessagev2 needs
            };

            Flapper flapper = new Flapper();
            string? systemPrompt;
            var uMsg = req.Message;

            if (req.flapperDTO.SpokenResponses)
            {
                //systemPrompt = flapper.BuildSystemMessagev3(systemMessageData);
                uMsg = $"[SPEECH] {uMsg}";
            }
            else
            {
                //systemPrompt = flapper.BuildSystemMessagev2(systemMessageData);
                uMsg = $"[REPORT] {uMsg}";

            }

            // 4. Build available tools
            var tools = flapperOllamaApi.BuildFlapperOllamaTools();

            // 5. Multi-turn reasoning loop (Flapper can call tools multiple times)
            const int MaxTurns = 6;
            int turn = 0;
            FlapperResponse result;
            bool isClarification = false;
            string curMsg = string.Empty;
            do
            {
                turn++;
                cancellationToken.ThrowIfCancellationRequested();
                curMsg = string.Empty;
                conversation.Messages = await db.FlapperMessages
                    .Where(c => c.ConversationId == conversation.Id)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync(cancellationToken);

                result = await flapperOllamaApi.OllamaProcessWithToolsAsync(
                    conversation,
                    req.flapperDTO,
                    uMsg,

                    tools, async chunk =>
                    {
                        if (chunk.StartsWith("[") && string.IsNullOrEmpty(curMsg))
                        {
                            isClarification = true;
                            //turn = MaxTurns;
                        }
                        curMsg += chunk;

                        if (!isClarification)
                        {
                            // Stream thinking/content chunks to UI in real-time
                            var streamMessage = new PulseMessage
                            {
                                SenderUserName = "Flapper",
                                RecipientUserId = req.UserId,
                                Role = "Flapper",
                                Subject = "Flapper is thinking...",
                                ContentType = "HTML",
                                Content = chunk,
                                SentAt = DateTime.UtcNow
                            };

                            await hubContext.Clients.User(req.UserId.ToString())
                            .SendAsync("ReceiveFlapperChunk", streamMessage);
                        }
                    }, cancellationToken);

                // If no tool calls → we're done
                if (result.ToolCalls == null || !result.ToolCalls.Any() || isClarification)
                    break;

                db.FlapperMessages.Add(new FlapperMessage
                {
                    ConversationId = conversation.Id,
                    Sender = "Flapper",
                    Content = curMsg,
                    SentAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();

                // Execute each tool call
                foreach (var toolCall in result.ToolCalls)
                {
                    string toolResult = toolCall.ToolName switch
                    {
                        "ask_tables" => await ExecuteAskTablesAsync(tablesApi, toolCall.Parameters, req.Message),
                        "web_search" => await ExecuteWebSearchAsync(toolCall.Parameters, flapperOllamaApi),
                        "web_fetch" => await ExecuteWebFetchAsync(toolCall.Parameters, flapperOllamaApi),
                        _ => $"Unknown tool: {toolCall.ToolName}"
                    };

                    // Save tool result back into conversation so Flapper can reason again
                    db.FlapperMessages.Add(new FlapperMessage
                    {
                        ConversationId = conversation.Id,
                        Sender = "tool",
                        Content = toolResult,
                        SentAt = DateTime.UtcNow,
                        ToolName = toolCall.ToolName
                    });
                }

                await db.SaveChangesAsync();

            } while (turn < MaxTurns);

            if (result.Content.StartsWith("[CLARIFICATION] ["))
            {
                isClarification = true;
            }
            // 6. Save Flapper's final response
            db.FlapperMessages.Add(new FlapperMessage
            {
                ConversationId = conversation.Id,
                Sender = "Flapper",
                Content = result.Content ?? result.Thinking ?? "No response generated.",
                SentAt = DateTime.UtcNow,
                ContentType = isClarification ? "Clarification" : "HTML",
                IsClarificationQuestion = isClarification
            });

            await db.SaveChangesAsync();

            // 7. Send final message to UI via your existing hub
            var finalMessage = new PulseMessage
            {
                SenderUserName = "Flapper",
                RecipientUserId = req.UserId,
                Role = "Flapper",
                Subject = isClarification ? "Clarification" : "Flapper Reply",
                ContentType = "HTML",
                Content = isClarification ? curMsg.Replace("[CLARIFICATION] [", "").TrimEnd("]").ToString() : result.Content ?? result.Thinking ?? "",
                SentAt = DateTime.UtcNow
            };

            await hubContext.Clients.User(req.UserId.ToString())
                .SendAsync("ReceiveMessage", finalMessage);

            return Results.Ok(result);
        
        }
        catch (OperationCanceledException)
        {
            return Results.Ok(new FlapperResponse
            {
        Success = false,
                Content = "The operation was cancelled."
            });
        }
        catch (Exception ex)
        {
            return Results.Ok(new FlapperResponse
            {
    Success = false,
                Content = $"An error occurred: {ex.Message}"
            });
        }
    }

    private static async Task<string> ExecuteAskTablesAsync(TablesAPI tablesApi, Dictionary<string, object> parameters, string originalQuery)
    {
        if (!parameters.TryGetValue("query", out var queryObj))
            return "Error: Missing query parameter";

        var query = queryObj.ToString();
        //Call your existing TablesAPI
        PulseAiRequest tablesRequest = new PulseAiRequest
        {
            UserId = "Flapper", // You can pass actual user ID if needed for logging
            AiName = AiName.Tables,
            UserRequest = new OllamaChatMessage { Content = query },
            UserEmail = "flapper@example.com",
            ModelName = "gpt-oss:latest"
        };
        var result = await tablesApi.AskTablesAsync(tablesRequest);
        if (result.Success)
        {
            return $"**Data from database**:\n{JsonSerializer.Serialize(result.Data)}";
        }
        else
        {
            return $"Error: {result.Message ?? "Unknown error"}";
        }
    }

    private static async Task<string> ExecuteWebSearchAsync(Dictionary<string, object> parameters,FlapperOllamaAPI flapperOllamaAPI)
    {
        if (!parameters.TryGetValue("query", out var queryProp))
            return "Error: Missing 'query' argument";

        var query = queryProp.ToString();
        if (string.IsNullOrEmpty(query))
            return "Error: Query is empty";

        var response = await flapperOllamaAPI.SearchWeb(new { query } );

        if (response?.Results is not { Count: > 0 })
            return $"No search results found for '{query}' related to user question"; //: {context.OriginalUserQuery}";

        var results = string.Join(" | ", response.Results.Select(r =>
            $"[{r.Title}]({r.Url}): {r.Snippet}"));

        // ✅ Include original user query in the tool result
        //var contextualResult = $"**User asked**: {context.OriginalUserQuery}\n\n" +
        var contextualResult = $"**Web search results for '{query}'**:\n{results}";

        return contextualResult.Length > 8000 ? contextualResult[..8000] + "..." : contextualResult;
        //if (!parameters.TryGetValue("query", out var queryObj) || queryObj is not string query)
        //    return "Error: Missing 'query' argument";

        //if (string.IsNullOrEmpty(query))
        //    return "Error: Query is empty";

        //Placeholder implementation -integrate with your actual web search service
        //For now, return a stub response
        //return $"Web search results for '{query}': [Integration pending with external search service]";
    }

    private static async Task<string> ExecuteWebFetchAsync(Dictionary<string, object> parameters, FlapperOllamaAPI flapperOllamaAPI)
    {
        

        
        if (!parameters.TryGetValue("url", out var urlObj))
            return "Error: Missing 'url' argument";

        var url = urlObj.ToString();
        if (string.IsNullOrEmpty(url))
            return "Error: URL is empty";

        var response = await flapperOllamaAPI.FetchWebPage(new { url });
        //Placeholder implementation -integrate with your actual web fetch service
        var content = response?.Content ?? "";

        // ✅ Include original user query and URL context in the tool result
        //var contextualResult = $"**Original user question**: {context.OriginalUserQuery}\n\n" +
        var contextualResult = $"**Content fetched from {url}**:\n\n{content}";

        return contextualResult.Length > 12000 ? contextualResult[..12000] + "..." : contextualResult;
    }

    private static async Task<IResult> GetUserConversationHistory(string userId, PulseDbContext db)
    {
        var conversations = await db.FlapperConversations
            .Where(c => c.UserId == userId)
            //.Include(c => c.Messages.OrderBy(m => m.SentAt))
            .OrderByDescending(c => c.LastActivity)
            .ToListAsync();

        return Results.Ok(new ApiResponse<List<FlapperConversation>>
        {
            Success = true,
            Data = conversations,
            Message = $"Retrieved {conversations.Count} conversations for user {userId}"
        });
    }
}

#region Old Flapper Endpoint Logic (for reference, not included in final code)

//private static async Task<IResult> FlapperChat(
//        FlapperChatRequest req,
//        AiShared _aiShared,
//        FlapperAPI flapperApi,
//        TablesAPI tablesApi,
//        PulseDbContext db,
//        IHubContext<MessageHub> hubContext)
//    {
//        var conversation = await db.FlapperConversations
//            .FirstOrDefaultAsync(c => c.Id == req.ConversationId && c.UserId == req.UserId);
//        if (conversation == null)
//        {
//            conversation = new FlapperConversation
//            {
//                Id = req.ConversationId,
//                UserId = req.UserId,
//                Title = "Flapper Chat"
//            };
//            db.FlapperConversations.Add(conversation);
//        }
//        var userMessage = new FlapperMessage
//        {
//            ConversationId = conversation.Id,
//            Sender = "User",
//            Content = req.Message,
//            SentAt = DateTime.UtcNow
//        };
//        db.FlapperMessages.Add(userMessage);
//        await db.SaveChangesAsync();
//        conversation.LastActivity = DateTime.UtcNow;


//        SystemMessageData systemMessageData = new SystemMessageData
//        {
//            CompanyName = "H&M Rollers",
//            CompanyInformation = AiPromptHelperService.GetTempCoInfo(),
//            UserName = req.PreferredUserName,
//            DbSchema = await _aiShared.GetDetailedSchemaAsync()
//        };
//        Flapper flapper = new Flapper();

//        var flapperSystemPrompt = flapper.BuildSystemMessagev2(systemMessageData);

//        //Define available tools(without AskUser - you already have clarification handled separately)
//        var tools = BuildFlapperTools();

//        int maxTurns = 5;           // Safety limit for multi-turn loops
//        int currentTurn = 0;
//        FlapperResponse finalResult = null!;

//        while (currentTurn < maxTurns)
//        {
//            currentTurn++;

//            //Let Flapper reason and possibly call tools
//            finalResult = await flapperApi.ProcessWithToolsAsync(
//                conversation, req.Message, flapperSystemPrompt, tools);

//            //If no tool calls, we're done
//            if (finalResult.ToolCalls == null || !finalResult.ToolCalls.Any())
//                break;

//            //Execute all tool calls
//            foreach (var toolCall in finalResult.ToolCalls)
//            {
//                string toolResult = toolCall.ToolName switch
//                {
//                    "AskTables" => await ExecuteAskTablesAsync(tablesApi, toolCall.Parameters, req.Message),
//                    "WebSearch" => await ExecuteWebSearchAsync(toolCall.Parameters),
//                    "WebFetch" => await ExecuteWebFetchAsync(toolCall.Parameters),
//                    _ => $"Unknown tool: {toolCall.ToolName}"
//                };

//                //Add tool result back into conversation history so Flapper can continue reasoning
//                db.FlapperMessages.Add(new FlapperMessage
//                {
//                    ConversationId = conversation.Id,
//                    Sender = "tool",
//                    Content = toolResult,
//                    SentAt = DateTime.UtcNow,

//                    ToolName = toolCall.ToolName
//                });
//            }

//            await db.SaveChangesAsync();
//        }

//        //Save Flapper's final message
//        db.FlapperMessages.Add(new FlapperMessage
//        {
//            ConversationId = conversation.Id,
//            Sender = "Flapper",
//            Content = finalResult.Content ?? finalResult.Thinking ?? "No response generated.",
//            SentAt = DateTime.UtcNow,
//            ContentType = "HTML"
//        });

//        await db.SaveChangesAsync();

//        //Send final response to UI
//        var finalMessage = new PulseMessage
//        {
//            SenderUserName = "Flapper",
//            RecipientUserId = req.UserId,
//            Role = "Flapper",
//            ContentType = "HTML",
//            Content = finalResult.Content ?? finalResult.Thinking ?? "",
//            SentAt = DateTime.UtcNow

//        };

//        await hubContext.Clients.User(req.UserId.ToString())
//            .SendAsync("ReceiveMessage", finalMessage);

//        return Results.Ok(finalResult);


//    }

//private static List<object> BuildFlapperTools()
//{
//    return new List<object>
//    {
//        new
//        {
//            type = "function",
//            function = new
//            {
//                name = "AskTables",
//                description = "Ask the SQL expert (Tables) to generate and run a query against the database. Use this when you need real data from the Pulse database.",
//                parameters = new
//                {
//                    type = "object",
//                    properties = new
//                    {
//                        query = new { type = "string", description = "Natural language description of the data you need" }
//                    },
//                    required = new[] { "query" }
//                }
//            }
//        },
//        new
//        {
//            type = "function",
//            function = new
//            {
//                name = "WebSearch",
//                description = "Search the internet for current information not available in the database.",
//                parameters = new
//                {
//                    type = "object",
//                    properties = new { query = new { type = "string", description = "Search query" } },
//                    required = new[] { "query" }
//                }
//            }
//        },
//        new
//        {
//            type = "function",
//            function = new
//            {
//                name = "WebFetch",
//                description = "Fetch the full content of a specific webpage.",
//                parameters = new
//                {
//                    type = "object",
//                    properties = new { url = new { type = "string", description = "URL to fetch" } },
//                    required = new[] { "url" }
//                }
//            }
//        }
//    };
//}


//        bool isClarification = false;
//    string clarificationQuestion = "";
//    string finalResponse = string.Empty;

//        if (!req.IsStreaming)
//        {
//            var result = await flapperApi.ProcessMessageAsync(conversation, req.Message, flapperSystemPrompt);
//isClarification = result.RequiresClarification;
//            finalResponse = isClarification? result.ClarificationQuestion! : result.Content!;
//        }
//        else
//{
//    await flapperApi.StreamResponseAsync(conversation, req.Message, flapperSystemPrompt, async chunk =>
//    {

//        // Check for clarification marker
//        //if (chunk.StartsWith("[CLARIFICATION]"))
//        if (chunk.StartsWith("[") && string.IsNullOrEmpty(finalResponse))
//        {
//            isClarification = true;
//        }

//        finalResponse += chunk;

//        if (!isClarification)
//        {
//            var pulseMessage = new PulseMessage
//            {
//                SenderUserName = "Flapper",
//                RecipientUserId = req.UserId,
//                Role = "Flapper",
//                Subject = "Flapper Reply",
//                ContentType = "HTML",
//                Content = chunk,
//                SentAt = DateTime.UtcNow
//            };
//            await hubContext.Clients.User(req.UserId.ToString())
//            .SendAsync("ReceiveFlapperChunk", pulseMessage);
//        }
//    });
//}

//if (finalResponse.StartsWith("[CLARIFICATION] [", StringComparison.Ordinal))
//{
//    finalResponse = finalResponse.Replace("[CLARIFICATION] [", "");
//    finalResponse = finalResponse.Substring(0, finalResponse.Length - 1);
//}

//FlapperMessage fmsg = new FlapperMessage
//{
//    ConversationId = conversation.Id,
//    Sender = "Flapper",
//    Content = finalResponse,
//    SentAt = DateTime.UtcNow,
//    ContentType = isClarification ? "Clarification" : "HTML",
//    IsClarificationQuestion = isClarification,
//};
//db.FlapperMessages.Add(fmsg);
//await db.SaveChangesAsync();

//if (isClarification || !req.IsStreaming)
//{

//    var clarMessage = new PulseMessage
//    {
//        SenderUserName = "Flapper",
//        RecipientUserId = req.UserId,
//        SenderUserId = isClarification ? fmsg.Id.ToString() : null,
//        Role = "Flapper",
//        Subject = "Clarification Needed",
//        ContentType = "HTML",
//        Content = finalResponse,
//        SentAt = DateTime.UtcNow
//    };
//    await hubContext.Clients.User(req.UserId.ToString())
//        .SendAsync("ReceiveMessage", clarMessage);
//}
//FlapperResponse response = new FlapperResponse
//{
//    Success = true,
//    RequiresClarification = isClarification,
//    ClarificationQuestion = isClarification ? clarificationQuestion : null
//};
//return Results.Ok(response);
//    }
//}

// Simple request DTO

#endregion
