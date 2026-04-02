using OllamaSharp;
using OllamaSharp.Models.Chat;
using Pulse.Models.AI.Flapper;
using System.Text.Json;

namespace Pulse.ApiService.PulseAI.Characters;

// Pulse.ApiService/PulseAI/Characters/FlapperAPI.cs   (or FlapperCharacter)
public class FlapperAPI
{
    private readonly IOllamaApiClient _ollamaClient;
    private readonly ILogger<FlapperAPI> _logger;

    public FlapperAPI(IOllamaApiClient ollamaClient, ILogger<FlapperAPI> logger)
    {
        _ollamaClient = ollamaClient;
        _logger = logger;
    }

    public async Task<FlapperResponse> ProcessMessageAsync(FlapperConversation conv, string userMessage)
    {
        // Build full history for context
        var messages = conv.Messages.Select(m => new Message
        {
            Role = m.Sender == "User" ? "user" : "assistant",
            Content = m.Content
        }).ToList();

        var systemPrompt = """
            You are Flapper, a helpful, friendly, and precise AI assistant.
            When the user's request is ambiguous or you need clarification:
            - Respond with EXACTLY this JSON and nothing else:
              {"type": "clarification", "question": "Your clear yes/no question?"}
            - Otherwise, give a normal helpful response in HTML.
            Keep responses concise and professional.
            """;

        var chatMessages = new List<Message>
        {
            new Message { Role = "system", Content = systemPrompt }
        };
        chatMessages.AddRange(messages);
        chatMessages.Add(new Message { Role = "user", Content = userMessage });

        var request = new ChatRequest
        {
            Model = "llama3.2",
            Messages = chatMessages,
            Stream = false,
            Options = new RequestOptions { Temperature = 0.4 }
        };

        var response = await _ollamaClient.ChatAsync(request);
        var rawReply = response?.Response?.Trim() ?? "";

        // Detect clarification request
        if (rawReply.StartsWith("{") && rawReply.Contains("\"type\": \"clarification\""))
        {
            try
            {
                var clar = JsonSerializer.Deserialize<ClarificationResponse>(rawReply);
                if (clar?.Type == "clarification" && !string.IsNullOrEmpty(clar.Question))
                {
                    return new FlapperResponse
                    {
                        Success = true,
                        RequiresClarification = true,
                        ClarificationQuestion = clar.Question,
                        RawContent = clar.Question
                    };
                }
            }
            catch { /* fall through to normal response */ }
        }

        return new FlapperResponse
        {
            Success = true,
            RequiresClarification = false,
            Content = rawReply,
            RawContent = rawReply
        };
    }
}

public record ClarificationResponse(string Type, string Question);

public class FlapperResponse
{
    public bool Success { get; set; }
    public string? Content { get; set; }
    public string? RawContent { get; set; }
    public bool RequiresClarification { get; set; }
    public string? ClarificationQuestion { get; set; }
}
