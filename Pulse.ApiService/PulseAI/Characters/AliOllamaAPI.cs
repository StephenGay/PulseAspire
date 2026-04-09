using Markdig;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Identity.Client;
using OllamaSharp;
using OllamaSharp.Models.Chat;

using Pulse.ApiService.PulseAI.Services;
using Pulse.Models.AI;
using Pulse.Models.AI.Ali;
using Pulse.Models.Api;
using Pulse.Models.CustomComponents;
using Pulse.Models.PulseContext;
using System.Text;

namespace Pulse.ApiService.PulseAI.Characters
{
    public class AliOllamaAPI
    {

        private readonly IOllamaApiClient _ollamaClient;
        private readonly ILogger<AliOllamaAPI> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public AliOllamaAPI(
            IOllamaApiClient ollamaClient,
            ILogger<AliOllamaAPI> logger,
            IHttpClientFactory httpClientFactory)
        {
            _ollamaClient = ollamaClient;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

        }

        public async Task<ApiResponse<string>> AnalyseWithAliAsync(ContextualArea context, object entity, string userQuery)
        {
            //string SessionId = Guid.NewGuid().ToString();
            try
            {
                //var client = _aiClientFactory.GetClient("FlapperAliClient");
                // Get or create session
                //Chat chat = new Chat(client)
                //{
                //    Model = "gpt-oss:latest"
                //};
                //lock (_sessionLock)
                //{
                //    _sessions[SessionId] = chat;
                //}
                var prompt = $"{context.AreaAiPrompt}\n\nEntity Data: {System.Text.Json.JsonSerializer.Serialize(entity)}\n\nRespond with a concise answer to the following user query based on the provided context and entity data.\n\nUser Query: {userQuery}";

                try
                {
                    List<OllamaSharp.Models.Chat.Message> cMessages = new List<OllamaSharp.Models.Chat.Message>();
                    cMessages.Add(
                        new OllamaSharp.Models.Chat.Message
                        {

                            Role = "system",
                            Content = context.AreaAiPrompt
                        });

                    cMessages.Add(new OllamaSharp.Models.Chat.Message
                    {
                        Role = "user",
                        Content = $"Entity Data: {System.Text.Json.JsonSerializer.Serialize(entity)}\n\nUser Query: {userQuery}"
                    });

                    var chat = new ChatRequest
                    {
                        Model = "gpt-oss:latest",   // or pull from config
                        Messages = cMessages
                    };

                    //await foreach (var chunk in _ollamaClient.ChatAsync(chat))
                    //{
                    //    // Just consume the stream to complete the system message
                    //}

                    //_logger.LogInformation("System prompt initialized for session {SessionId}", SessionId);

                    var ResponseBuilder = new StringBuilder();

                    await foreach (var chunk in _ollamaClient.ChatAsync(chat))
                    {
                        // The actual generated text is usually in chunk.Message?.Content or chunk.Response / chunk.Delta
                        if (chunk?.Message?.Content is not null)
                        {
                            ResponseBuilder.Append(chunk.Message.Content);
                        }
                       
                    }

                    var rawResponse = ResponseBuilder.ToString().Trim();
                    var aliResponse = ParseToHtml(rawResponse);

                    //// Log the interaction (you can expand this to log more details as needed)
                    //_logger.LogInformation("Session {SessionId}: User: {UserQuery} | Ali: {AliResponse}",
                    //    SessionId, userQuery, aliResponse);
                    return new ApiResponse<string> { Success = true, Data = aliResponse, Timestamp = DateTime.UtcNow };
                    //var response = await _ollamaClient.ChatAsync(chat).FirstAsync();
                    //var resultText = string.Join("", response?.Message?.Content ?? string.Empty);

                    //return new ApiResponse<string> { Data = resultText, Success = true };

                    //List<OllamaMessage> cMessages = new List<OllamaMessage>();
                    //cMessages.Add(
                    //    new OllamaMessage
                    //    {

                    //        Role = "system",
                    //        Content = context.AreaAiPrompt
                    //    });

                    //cMessages.Add(new OllamaMessage
                    //{
                    //    Role = "user",
                    //    Content = $"Entity Data: {System.Text.Json.JsonSerializer.Serialize(entity)}\n\nUser Query: {userQuery}"
                    //});

                    //// Build messages using the modern IChatClient format
                    //var cReq = new OllamaRequest
                    // {
                    //    Model = "gpt-oss:latest",
                    //    Prompt = prompt
                    //};

                    //var options = new ChatOptions
                    //{
                    //    Temperature = 0.3f,
                    //    // You can add Tools here later when we add tool calling
                    //};

                    // ✅ Correct call for IChatClient
                    //using var aliClient = _httpClientFactory.CreateClient("AliApiClient");
                    //var response = aliClient.PostAsync<OllamaGenerateResponse>("api/generate", cReq);
                    //response.EnsureSuccessStatusCode();
                    //var resultText = response.Content.ReadAsStringAsync().Result?.Trim() ?? string.Empty;

                    //return new ApiResponse<string>
                    //{
                    //    Data = resultText,
                    //    Success = true
                    //};
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Analysis failed");
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in AnalyseWithAliAsync");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please try again later."
                };
            }
        }

        private string ParseToHtml(string markdownContent)
        {
            if (string.IsNullOrEmpty(markdownContent))
            {
                return string.Empty;
            }
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build(); // Customize extensions (e.g., syntax highlighting)
            var html = Markdig.Markdown.ToHtml(markdownContent, pipeline);

            html = System.Text.RegularExpressions.Regex.Replace(
                html,
                @"<a\s+href=""([^""]*)""",
                @"<a href=""$1"" target=""_blank"" rel=""noopener noreferrer"""
            );

            return AddMUcss(html);
        }
        private string AddMUcss(string html)
        {
            return html
                .Replace("<table>", "<table class=\"table table-striped table-hover table-bordered\">")
                .Replace("<th>", "<th style=\"background-color: #d9d9d9 ; color: #660000; border: 1px solid #660000;\">")
                .Replace("</table>", "</table><br />")
                .Replace("<hr />", "")
                .Replace("<h3", "<hr /><h3")
                .Replace("</table><br /><hr />", "</table><hr />")
                .Replace("h2", "h5")
             .Replace("h3", "h5");
        }
    }
}
