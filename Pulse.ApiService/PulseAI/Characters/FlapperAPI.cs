using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using Pulse.Models.AI.Flapper;
using System.Text.Json;

namespace Pulse.ApiService.PulseAI.Characters;

// Pulse.ApiService/PulseAI/Characters/FlapperAPI.cs   (or FlapperCharacter)
public class FlapperAPI
{
    private readonly IOllamaApiClient _flapperClient;
    private readonly ILogger<FlapperAPI> _logger;

    public FlapperAPI(IOllamaApiClient client, ILogger<FlapperAPI> logger)
    {
        _flapperClient = client;
        _logger = logger;
    }

    public async Task<FlapperResponse> ProcessMessageAsync(FlapperConversation conv, string userMessage)
    {
        try
        {
            // Build full history for context (this keeps the session alive)
            var history = conv.Messages.Select(m => new Message
            {
                Role = m.Sender == "User" ? "user" : "assistant",
                Content = m.Content
            }).ToList();

            var systemPrompt = """
            You are Flapper, a helpful, friendly, and precise AI assistant in the Pulse system.

            IMPORTANT RULES:
            - If the user's request is clear, respond with a normal helpful answer **in HTML**.
            - If the request is ambiguous or you need clarification (yes/no or short answer), 
              respond with **EXACTLY** this JSON and **nothing else**:
              {"type": "clarification", "question": "Your clear yes/no question here?"}

            Examples:
            User: Should I approve this order?
            Assistant: {"type": "clarification", "question": "Do you want to approve this order?"}

            User: Tell me about the client sales.
            Assistant: <p>Here is the client sales summary...</p>

            Keep responses concise and professional.
            """;

            var messages = new List<Message>
        {
            new Message { Role = "system", Content = systemPrompt }
        };
            messages.AddRange(history);
            messages.Add(new Message { Role = "user", Content = userMessage });

            var request = new ChatRequest
            {
                Model = "gpt-oss:latest",                    // ← Use a model you actually have in your Ollama container
                Messages = messages,
                Stream = false,                        // Important: non-streaming for structured output
                Options = new RequestOptions
                {
                    Temperature = 0.3f,
                    NumPredict = 1024
                }
            };

            // === CORRECT WAY TO CALL ChatAsync in OllamaSharp ===
            var response = await _flapperClient.ChatAsync(request).FirstAsync();   // This fixes the awaiter error

            var rawReply = response?.Message?.Content?.Trim() ?? string.Empty;

            // Detect clarification JSON
            if (rawReply.StartsWith("{", StringComparison.Ordinal) &&
                rawReply.Contains("\"type\": \"clarification\"", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var clar = JsonSerializer.Deserialize<ClarificationResponse>(rawReply);
                    if (clar?.Type == "clarification" && !string.IsNullOrWhiteSpace(clar.Question))
                    {
                        return new FlapperResponse
                        {
                            Success = true,
                            RequiresClarification = true,
                            ClarificationQuestion = clar.Question.Trim(),
                            RawContent = clar.Question
                        };
                    }
                }
                catch (JsonException)
                {
                    // Fall through - treat as normal response if JSON parse fails
                }
            }

            // Normal response
            return new FlapperResponse
            {
                Success = true,
                RequiresClarification = false,
                Content = rawReply,
                RawContent = rawReply
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flapper ProcessMessageAsync failed");
            return new FlapperResponse
            {
                Success = false,
                Content = "Sorry, I encountered an error while processing your request."
            };
        }
    }
}

    
