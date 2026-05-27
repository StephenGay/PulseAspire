// Pulse.ApiService/PulseAI/Characters/FlapperAPI.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using Pulse.ApiService.Hubs;
using Pulse.ApiService.PulseAI.Services;
using Pulse.Models.AI;
using Pulse.Models.AI.Flapper;
using Pulse.Models.AI.Tools;
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
using static OllamaSharp.Models.Chat.Message;
using static Pulse.Models.AI.Tables.TablesChatStructures;
using static Pulse.Models.Api.ApiEndpoints.PulseAi;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Emojis = Microsoft.FluentUI.AspNetCore.Components.Emojis;

namespace Pulse.ApiService.PulseAI.Characters;


public class FlapperOllamaAPI
{
    private const int MAX_CHUNK_SIZE = 4096;
    private const int MAX_DATABASE_RESULT_SIZE = 100000;
    private const string TRUNCATION_INDICATOR = "\n... [result truncated due to size limits] ...";

    private readonly IOllamaApiClient _chatClient;
    private readonly ILogger<FlapperOllamaAPI> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private TablesAPI _tablesAPI;
    //private readonly Flapper flapper = new();

    private readonly string _ollamaApiKey;
    private readonly Uri _localBaseAddress;
    private readonly Uri _cloudBaseAddress;
    private readonly JsonSerializerOptions _jsonOptions;
    private string convId;
    private string? chartJSON;

    public FlapperOllamaAPI(IOllamaApiClient chatClient, ILogger<FlapperOllamaAPI> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _chatClient = chatClient;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _ollamaApiKey = configuration["OllamaApi:Key"] ?? string.Empty;
        _localBaseAddress = new Uri(configuration["OllamaApi:EndpointHttp"] ?? "http://localhost:11434");
        _cloudBaseAddress = new Uri("https://ollama.com");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<OllamaWebFetchResponse?> FetchWebPage(string url, CancellationToken ct = default)
    {
        var searchUrl = "https://ollama.com/api/web_fetch";

        ArgumentNullException.ThrowIfNull(url);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, searchUrl);
            var requestPayload = new { url };
            var jsonPayload = JsonSerializer.Serialize(requestPayload, _jsonOptions);
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

    private async Task ChunkAndSendAsync(
        string content,
        Func<string, Task> onChunkReceived,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(content))
            return;

        if (content.Length > MAX_DATABASE_RESULT_SIZE)
        {
            content = content.Substring(0, MAX_DATABASE_RESULT_SIZE) + TRUNCATION_INDICATOR;
            _logger.LogWarning("Large result truncated to {MaxSize} characters", MAX_DATABASE_RESULT_SIZE);
        }

        for (int i = 0; i < content.Length; i += MAX_CHUNK_SIZE)
        {
            var chunk = content.Substring(i, Math.Min(MAX_CHUNK_SIZE, content.Length - i));
            await onChunkReceived(chunk);

            if (i + MAX_CHUNK_SIZE < content.Length)
            {
                await Task.Delay(10, ct);
            }
        }
    }

    public async Task<OllamaWebSearchResponse?> SearchWeb(string query, CancellationToken ct = default)
    {
        var searchUrl = "https://ollama.com/api/web_search";

        ArgumentNullException.ThrowIfNull(query);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, searchUrl);
            var requestPayload = new { query };
            var jsonPayload = JsonSerializer.Serialize(requestPayload, _jsonOptions);
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
            //messages.Add(new OllamaMessage { Role = "user", Content = userMessage });

            
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
                    PresencePenalty = (float?)flapperDTO.Options.PresencePenalty,
                    
                    // Add any other options you want to set globally
                }
            };
            var fullResponse = new StringBuilder();
            bool inThinking = false;
            List<PulseToolCall> toolCalls = new();

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
                        toolCalls.Add(new PulseToolCall 
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

        //tools.Add(new
        //{
        //    type = "function",
        //    function = new
        //    {
        //        name = ToolNames.AskUser,
        //        description = "Ask the user a Yes/No question.",
        //        parameters = new
        //        {
        //            type = "object",
        //            properties = new { question = new { type = "string", description = "NL question" } },
        //            required = new[] { "question" }
        //        }
        //    }
        //});

        //tools.Add(new
        //{
        //    type = "function",
        //    function = new
        //    {
        //        name = ToolNames.ExecuteSql,
        //        description = "Execute a SELECT SQL query against the dbPulse database and return results.",
        //        parameters = new
        //        {
        //            type = "object",
        //            properties = new { sql = new { type = "string", description = "The SQL query to execute" } },
        //            required = new[] { "sql" }
        //        }
        //    }
        //});

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

    public async Task<string> ProcessToolResultAsync(
        string toolName,
        Dictionary<string, object> parameters,
        Func<string, Task> onChunkReceived,
        CancellationToken ct = default)
    {
        try
        {
            return toolName switch
            {
                ToolNames.WebSearch => await ExecuteWebSearchAsync(parameters, onChunkReceived, ct),
                ToolNames.WebFetch => await ExecuteWebFetchAsync(parameters, onChunkReceived, ct),
                ToolNames.AskTables => await ExecuteAskTablesAsync(parameters, onChunkReceived, ct),
                _ => $"Error: Unknown tool '{toolName}'"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing tool: {ToolName}", toolName);
            return $"Error executing tool '{toolName}': {ex.Message}";
        }
    }

    private async Task<string> ExecuteWebSearchAsync(
        Dictionary<string, object> parameters,
        Func<string, Task> onChunkReceived,
        CancellationToken ct = default)
    {
        if (!parameters.TryGetValue("query", out var queryObj) || queryObj is not string query)
            return "Error: Missing 'query' parameter for web search";

        try
        {
            var result = await SearchWeb(query, ct);
            if (result != null)
            {
                var jsonResult = JsonSerializer.Serialize(result, _jsonOptions);
                await ChunkAndSendAsync(jsonResult, onChunkReceived, ct);
                return $"Web search completed for query: '{query}'";
            }

            return $"No web search results for: '{query}'";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Web search failed");
            return $"Web search error: {ex.Message}";
        }
    }

    private async Task<string> ExecuteWebFetchAsync(
        Dictionary<string, object> parameters,
        Func<string, Task> onChunkReceived,
        CancellationToken ct = default)
    {
        if (!parameters.TryGetValue("url", out var urlObj) || urlObj is not string url)
            return "Error: Missing 'url' parameter for web fetch";

        try
        {
            var result = await FetchWebPage(url, ct);
            if (result != null)
            {
                var jsonResult = JsonSerializer.Serialize(result, _jsonOptions);
                await ChunkAndSendAsync(jsonResult, onChunkReceived, ct);
                return $"Web page fetched: {url}";
            }

            return $"Failed to fetch: {url}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Web fetch failed");
            return $"Web fetch error: {ex.Message}";
        }
    }

    private async Task<string> ExecuteAskTablesAsync(
        Dictionary<string, object> parameters,
        Func<string, Task> onChunkReceived,
        CancellationToken ct = default)
    {
        if (!parameters.TryGetValue("query", out var queryObj) || queryObj is not string query)
            return "Error: Missing 'query' parameter";

        try
        {
            var result = $"Tables query executed: {query}\n[Database query results would be inserted here]\n[Size-limited to prevent truncation]";
            await ChunkAndSendAsync(result, onChunkReceived, ct);
            return "Database query completed";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tables query failed");
            return $"Tables error: {ex.Message}";
        }
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

    public async Task<FlapperResponse> FlapperStreamingIChat(
        FlapperChatRequest req,
        AiShared _aiShared,
        TablesAPI tablesApi,
        PulseDbContext db,
        IHubContext<MessageHub> hubContext,
        CancellationToken cancellationToken)
    {
        IChatClient iFlapperClient = new OllamaApiClient(_localBaseAddress.ToString(), "Flapper:latest");
        iFlapperClient = ChatClientBuilderChatClientExtensions
            .AsBuilder(iFlapperClient)
            .UseFunctionInvocation()
            .Build();
        convId = req.ConversationId.ToString();
        var conversation = await db.FlapperConversations
                .FirstOrDefaultAsync(c => c.Id == req.ConversationId && c.UserId == req.UserId);

        if (conversation == null)
        {
            conversation = new FlapperConversation
            {
                Id = req.ConversationId,
                UserId = req.UserId,
                StartedAt = DateTime.Now,
                Title = req.Message.Length > 50 ? req.Message.Substring(0, 50) + "..." : req.Message
            };
            db.FlapperConversations.Add(conversation);
            await db.SaveChangesAsync();
        }

        // 2. Save user message
        db.FlapperMessages.Add(new FlapperMessage
        {
            ConversationId = conversation.Id,
            Sender = req.IsClarificationResponse ? "tool" : "User",
            ContentType = req.IsClarificationResponse ? "UserAnswer" : "UserQuery",
            Content = req.Message,
            SentAt = DateTime.Now
        });

        conversation.LastActivity = DateTime.Now;
        db.Update(conversation);
        await db.SaveChangesAsync();

        conversation.Messages = await db.FlapperMessages
                    .Where(c => c.ConversationId == conversation.Id)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync(cancellationToken);

        var history = conversation.Messages.Select(m => new ChatMessage(
                role: m.Sender == "User" ? Microsoft.Extensions.AI.ChatRole.User : (m.Sender == "tool" ? Microsoft.Extensions.AI.ChatRole.Tool : Microsoft.Extensions.AI.ChatRole.Assistant),
                content: m.Content
            )).ToList();

        var messages = new List<ChatMessage>();
        messages.AddRange(history);
        //messages.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, userMessage));
        ChatOptions chatOptions = new ChatOptions
        {
            ConversationId = convId,
            Tools =
            [
                AIFunctionFactory.Create(WebIClientSearch,"WebSearch", "Search the web for current information."),
                AIFunctionFactory.Create(WebIClientFetch,"WebFetch", "Fetch the content of a web page."),
                AIFunctionFactory.Create(IClientAskTables,"AskTables", "Use this tool when you need real data from the Pulse database. Provide a clear natural language description of the data required."),
                AIFunctionFactory.Create(CreateIClientChart,"CreateChart","Creates a visual chart for the user. Use this tool whenever the user asks for a chart, graph, visualization, trend, comparison, or 'show me' data. Always use real data (call the Tables tool first if you need to query the database).")
       
            ]
        };
        _tablesAPI = tablesApi;
        var accumulatedText = new StringBuilder();
        var accumulatedThinking = string.Empty;
        var accumulatedAnswer = string.Empty;
        bool inThinking = false;
        var promptTokens = 0L;
        var outputTokens = 0L;
        string? doneReason = null;
        string? error = null;
        

        await foreach (var update in iFlapperClient.GetStreamingResponseAsync(messages, chatOptions, cancellationToken))
        {
            var streamTxt = string.Empty;
            var FlapperChunk = new FlapperResponse();
            if (update.RawRepresentation != null)
            {
                var ollama = ((OllamaSharp.Models.Chat.ChatResponseStream)update.RawRepresentation);

                if (!string.IsNullOrEmpty(ollama.Message?.Thinking))
                {
                    var thinkingText = ollama.Message.Thinking;
                    if (!inThinking)
                    {
                        inThinking = true;

                        thinkingText = $"Thinking:\n{thinkingText}";
                    }

                    FlapperChunk.Thinking = thinkingText;
                    accumulatedThinking += ollama.Message?.Thinking;
                }
                if (!string.IsNullOrEmpty(ollama.Message?.Content))
                {
                    if (inThinking)
                    {
                        inThinking = false;
                        streamTxt += "</thinking>";
                    }
                    streamTxt += ollama.Message.Content;
                    FlapperChunk.Content = ollama.Message.Content;
                    accumulatedAnswer += ollama.Message.Content;
                }



                FlapperChunk.Done = ollama.Done;
            }
        FlapperChunk.EndReason = update.FinishReason?.ToString().ToLowerInvariant();
        FlapperChunk.PromptTokens = update.AdditionalProperties?.TryGetValue("prompt_eval_count", out var ptc) == true ? Convert.ToInt64(ptc) : 0;
        FlapperChunk.OutputTokens = update.AdditionalProperties?.TryGetValue("eval_count", out var etc) == true ? Convert.ToInt64(etc) : 0;

        if (update.FinishReason != null)
        {
            doneReason = update.FinishReason?.ToString().ToLowerInvariant();
        }
        if (update.AdditionalProperties?.TryGetValue("done_reason", out var dr) == true)
        {
            doneReason = dr?.ToString()?.ToLowerInvariant();
        }

        if (update.AdditionalProperties?.TryGetValue("prompt_eval_count", out var pt) == true)
            promptTokens = Convert.ToInt64(pt);

        if (update.AdditionalProperties?.TryGetValue("eval_count", out var et) == true)
            outputTokens = Convert.ToInt64(et);

  

				accumulatedText.Append(streamTxt);



    await hubContext.Clients.User(req.UserId.ToString()).SendAsync("ReceiveFlapperResponseChunk", FlapperChunk);
    //await hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveFlapperChunk", streamMessage);

			}

			var rawReply = accumulatedText.ToString().Trim();
var thinking = accumulatedThinking;
var finalContent = accumulatedAnswer;

var endReason = doneReason switch
{
    "length" => "max_tokens_reached",
    "stop" or "eos" => "natural_stop",
    "context" => "context_length_exceeded",
    _ => doneReason ?? "unknown"
};
        db.FlapperMessages.Add(new FlapperMessage
        {
            ConversationId = conversation.Id,
            Sender = "Flapper",
            Content = finalContent ?? "No response generated.",
            SentAt = DateTime.Now,
            ContentType = string.IsNullOrEmpty(chartJSON) ? "AiResponse" : "Chart", /*isClarification ? "AiQuestion" : "AiResponse",*/
            RawContent = chartJSON
            //IsClarificationQuestion = isClarification
        });

        await db.SaveChangesAsync();
        var chJ = chartJSON;
        chartJSON = string.Empty;

        return new FlapperResponse
        {
            Success = true,
            Thinking = string.IsNullOrWhiteSpace(thinking) ? null : thinking,
    
            Content = finalContent,
            RequiresClarification = false,
            EndReason = endReason,
            PromptTokens = promptTokens,
            OutputTokens = outputTokens,
            RawContent = chJ
            //RawContent = rawReply
        };
		
}
    private async Task<string> CreateIClientChart(string chartString)
    {
        chartJSON = chartString;
        return chartString;
    }
    private async Task<string> WebIClientSearch(string query, CancellationToken ct = default)
    {
        
        if (string.IsNullOrEmpty(query))
            return "Error: Query is empty";

        var searchUrl = "https://ollama.com/api/web_search";

        ArgumentNullException.ThrowIfNull(query);
        OllamaWebSearchResponse? responsec = null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, searchUrl);
            var requestPayload = new { query };
            var jsonPayload = JsonSerializer.Serialize(requestPayload, _jsonOptions);
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

            responsec = JsonSerializer.Deserialize<OllamaWebSearchResponse>(responseContent, _jsonOptions);
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
        

        if (responsec?.Results is not { Count: > 0 })
            return $"No search results found for '{query}' related to user question"; //: {context.OriginalUserQuery}";

        var results = string.Join(" | ", responsec.Results.Select(r =>
            $"[{r.Title}]({r.Url}): {r.Snippet}"));

        // ✅ Include original user query in the tool result
        //var contextualResult = $"**User asked**: {context.OriginalUserQuery}\n\n" +
        var contextualResult = $"**Web search results for '{query}'**:\n{results}";

        return contextualResult.Length > 15000 ? contextualResult[..15000] + "..." : contextualResult;
        //if (!parameters.TryGetValue("query", out var queryObj) || queryObj is not string query)
        //    return "Error: Missing 'query' argument";

        //if (string.IsNullOrEmpty(query))
        //    return "Error: Query is empty";

        //Placeholder implementation -integrate with your actual web search service
        //For now, return a stub response
        //return $"Web search results for '{query}': [Integration pending with external search service]";
    }
   

    private async Task<string> WebIClientFetch(string url, CancellationToken ct = default)
    {



        if (string.IsNullOrEmpty(url))
            return "Error: URL is empty";

        var searchUrl = "https://ollama.com/api/web_fetch";

        ArgumentNullException.ThrowIfNull(url);
        OllamaWebFetchResponse? responseC;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, searchUrl);
            var requestPayload = new { url };
            var jsonPayload = JsonSerializer.Serialize(requestPayload, _jsonOptions);
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

            responseC = JsonSerializer.Deserialize<OllamaWebFetchResponse>(responseContent, _jsonOptions);
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
        
        //Placeholder implementation -integrate with your actual web fetch service
        var content = responseC?.Content ?? "";

        // ✅ Include original user query and URL context in the tool result
        //var contextualResult = $"**Original user question**: {context.OriginalUserQuery}\n\n" +
        var contextualResult = $"**Content fetched from {url}**:\n{content}";

        return contextualResult.Length > 12000 ? contextualResult[..12000] + "..." : contextualResult;
    }

    private async Task<string> IClientAskTables(string query)
    {
        if (string.IsNullOrEmpty(query))
            return "Error: Missing query parameter";

        
        //Call your existing TablesAPI
        TablesRequest tablesRequest = new TablesRequest
        {
            UserId = "Flapper", // You can pass actual user ID if needed for logging
            UserRequest = query,
            UserEmail = "flapper@example.com",
            ModelName = "Default",
            SessionId = convId
        };
        var result = await _tablesAPI.AskTablesAsync(tablesRequest);
        if (result.Success)
        {
            //originalQuery = originalQuery.Replace("[REPORT]", "").Replace("[SPEECH]", "").Trim();
            var tblResponse = $"**Data from database**:\n{JsonSerializer.Serialize(result.Data)}";
            return tblResponse;
        }
        else
        {
            return $"Error: {result.Message ?? "Unknown error"}";
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