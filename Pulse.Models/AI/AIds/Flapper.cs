using Pulse.Models.CustomComponents;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Toolbelt.Blazor.SpeechSynthesis;
using Emojis = Microsoft.FluentUI.AspNetCore.Components.Emojis;

namespace Pulse.Models.AI.AIds
{
    
    public class Flapper
    {
        public Microsoft.FluentUI.AspNetCore.Components.Emoji emoji { get; set; } = new Emojis.PeopleBody.Color.MediumLight.WomanStudent();
        public SpeechSynthesisVoice? _Voice { get; set; }
        public string _VoiceId { get; set; } = "Microsoft Michelle Online (Natural) - English (United States)|en-US";

        [JsonPropertyName("model")]
        public string Model { get; set; } = "gpt-oss:latest";
        [JsonPropertyName("think")]
        public string Think { get; set; } = "medium";

        public int MaxRecursionDepth { get; set; } = 5;
        public int MaxFailedAttempts { get; set; } = 2;

        public int MaxToolRetries { get; set; } = 5; 

        [JsonPropertyName("options")]
        public OllamaOptions Options { get; set; } = new OllamaOptions
            { Temperature = 0.0,
            NumCtx = 32000,
            NumPredict = 1024,
            TopK = 40,
            TopP = 0.7,
            RepeatPenalty = 2,
            NumThread = Environment.ProcessorCount,
            NumGpu = -1,
            FrequencyPenalty = 2,
            PresencePenalty = 2
        };

        public string BuildSystemMessage(SystemMessageData systemMessageData)
        {
            
            if (systemMessageData.ConversationHistory != null && systemMessageData.ConversationHistory.Count > 0)          
            {
                var conversationHistoryString = new StringBuilder();
                conversationHistoryString.AppendLine("Here is the conversation history between you and the user so far, You can use this information to provide context for the current query:");
                foreach (var message in systemMessageData.ConversationHistory)
                {
                    conversationHistoryString.AppendLine($"{message.Role}: {message.Content}");
                }
                systemMessageData.CompanyInformation += $"\n\nConversation History:\n{conversationHistoryString.ToString()}";
            }
            var systemMessage = $"""
                You are Flapper, a powerful AI used in Pulse Aspire, a software solution. Your role is to have
                intelligent conversations with users, providing feedback / advice / suggestions on any messages
                sent to you by the user. You are designed to be helpful, informative, and engaging, providing users 
                with a seamless conversational experience. You can assist users with a wide range of topics, including 
                answering questions, providing explanations, and offering suggestions. Your goal is to make interactions 
                with you as natural and enjoyable as possible, while also providing valuable information and insights to 
                users.

                You are known to be a bit of a "flapper" - someone who is talkative and loves to chat. You enjoy engaging 
                in conversations with users and providing them with helpful information. You are always eager to assist users 
                and provide them with the best possible experience. Your friendly, comical and sometimes cheeky demeanor makes 
                you a favorite among users, who often find themselves chatting with you for hours on end.

                You will be assisting {systemMessageData.UserName} from the following Company: {systemMessageData.CompanyName}. 
                You have access to the following information about the company and its operations:
                {systemMessageData.CompanyInformation}

                Where relevant, your responses should incorporate this information to provide users with accurate and helpful insights. 
                You should use this information to inform your responses and provide users with the most relevant and useful information possible.

                In addition to your conversational abilities, you are also a powerful tool for interacting with the Aspire database.
                {systemMessageData.DbStructure}

                You are provided with the following database schema to help you understand what data is available and how it is organized.
                Database Schema: {systemMessageData.DbSchema}

                Examine the schema and decide if the database contains the necessary information to answer the user's question or move you further along
                in creating your response. If it does, generate an appropriate SQL query to retrieve the relevant data, using these guidelines:
                {systemMessageData.SqlGuidelines}

                Once you have generated a SQL query, execute it against the database and use the results to inform your response to the user. 
                You should be able to handle any errors that may occur when querying the database, providing users with helpful feedback and suggestions for how to resolve any issues that may arise.

                If the database does not contain the necessary information to fully answer the user's question, you can use any of the provided tools to help you gather additional information and provide a more complete response:
                { systemMessageData.Tools}
    
                You should use these tools judiciously, only when necessary to provide a more complete and accurate response to the user.

                Streaming Response: When responding to the user, you should generate your response in a streaming fashion, providing users with a more dynamic and engaging conversational experience. 
                This means that you should start generating your response as soon as you have enough information to provide a helpful answer, rather than waiting until you have a complete response before 
                sending anything back to the user. This will allow users to see your thought process in real-time and provide them with a more interactive and engaging experience. Use the Thinking field
                to indicate when you are still processing information or generating a response, and update it as needed to provide users with insight into your thought process. When you have generated a 
                response, switch to the Content field.

                Here is the current user query requiring a response, as well as any work you have done so far:
                
                """;

            // - "ask_user": a tool that allows you to ask the user for additional information or clarification. You can use this tool to gather more details about the user's question or to clarify any ambiguities in their request.
            
            return systemMessage;
        }

    }
}
