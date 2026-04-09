// Pulse.ApiService/PulseAI/Characters/FlapperAPI.cs
using Microsoft.Extensions.AI;
using Pulse.Models.AI.Flapper;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pulse.ApiService.PulseAI.Characters;

public class FlapperAPI
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<FlapperAPI> _logger;

    public FlapperAPI(IChatClient chatClient, ILogger<FlapperAPI> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<FlapperResponse> ProcessWithToolsAsync(
        FlapperConversation conv,
        string userMessage,
        string systemPrompt,
        IEnumerable<AITool> tools,
        CancellationToken ct = default)
    {
        try
        {
            // Build conversation history
            var history = conv.Messages.Select(m => new ChatMessage(
                role: m.Sender == "User" ? ChatRole.User : ChatRole.Assistant,
                content: m.Content
            )).ToList();

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, systemPrompt)
            };
            messages.AddRange(history);
            messages.Add(new ChatMessage(ChatRole.User, userMessage));

            var options = new ChatOptions
            {
                Temperature = 0.2f,
                Tools = tools?.ToList() ?? new List<AITool>()
            };

            // Correct IChatClient call
            var response = await _chatClient.GetResponseAsync(messages, options, ct);

            var rawReply = response.Text?.Trim() ?? string.Empty;

            // Extract thinking (if Flapper used <thinking> tags)
            var thinking = ExtractBetween(rawReply, "<thinking>", "</thinking>");

            // Extract tool calls (native from IChatClient)
            //var toolCalls = response.ToolCalls?.Select(tc => new FlapperToolCall
            //{
            //    ToolName = tc.Name,
            //    Parameters = tc.Arguments ?? new Dictionary<string, object>()
            //}).ToList() ?? new List<FlapperToolCall>();

            // Clean final content
            var finalContent = rawReply
                .Replace($"<thinking>{thinking}</thinking>", "")
                .Trim();

            return new FlapperResponse
            {
                Success = true,
                Thinking = string.IsNullOrWhiteSpace(thinking) ? null : thinking,
                //ToolCalls = toolCalls.Any() ? toolCalls : null,
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