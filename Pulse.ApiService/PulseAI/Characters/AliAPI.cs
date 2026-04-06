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
    public class AliAPI
    {

        //private readonly AiShared _aiShared;
        private readonly IChatClient _chatClient;
        //private readonly IPulseAiClientFactory _aiClientFactory;
        //private readonly IDbContextFactory<PulseDbContext> _dbFactory;
        private readonly ILogger<AliAPI> _logger;
        // Session management - stores chat history per session
        //private static readonly Dictionary<string, Chat> _sessions = new(StringComparer.OrdinalIgnoreCase);
        //private static readonly object _sessionLock = new object();

        public AliAPI(
            //AiShared aiShared,
            IChatClient chatClient,
            //IPulseAiClientFactory aiClientFactory,
            //IDbContextFactory<PulseDbContext> dbFactory,
            ILogger<AliAPI> logger)
        {
            //_aiShared = aiShared;
            //_ollamaClient = ollamaClient;
            ////_dbFactory = dbFactory;
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
                    // Build messages using the modern IChatClient format
                    var messages = new List<ChatMessage>
                     {
                    new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, context.AreaAiPrompt),   // System prompt
                     new ChatMessage(Microsoft.Extensions.AI.ChatRole.User,
                    $"Entity Data: {System.Text.Json.JsonSerializer.Serialize(entity)}\n\nUser Query: {userQuery}")
                    };

                    var options = new ChatOptions
                    {
                        Temperature = 0.3f,
                        // You can add Tools here later when we add tool calling
                    };

                    // ✅ Correct call for IChatClient
                    var response = await _chatClient.GetResponseAsync(messages, options);

                    var resultText = response.RawRepresentation?.ToString()?.Trim() ?? string.Empty;

                    return new ApiResponse<string>
                    {
                        Data = resultText,
                        Success = true
                    };
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
