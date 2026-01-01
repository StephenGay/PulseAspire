using Pulse.Models.CustomComponents;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Toolbelt.Blazor.SpeechSynthesis;
using Emojis = Microsoft.FluentUI.AspNetCore.Components.Emojis;

namespace Pulse.Models.AI.AIds
{
    public class Tables
    {
        public Microsoft.FluentUI.AspNetCore.Components.Emoji emoji { get; set; } = new Emojis.PeopleBody.Color.MediumLight.ManTechnologist();
        public SpeechSynthesisVoice _Voice { get; set; }
        public string _VoiceId { get; set; } = "Microsoft Guy Online (Natural) - English (United States)|en-US";
    }
    
    public class Ali
    {
        public Microsoft.FluentUI.AspNetCore.Components.Emoji emoji { get; set; } = new Emojis.PeopleBody.Color.MediumLight.Detective();
        public SpeechSynthesisVoice _Voice { get; set; }
        public string _VoiceId { get; set; } = "Microsoft Steffan Online (Natural) - English (United States)|en-US";
    }
    public class Flapper
    {
        public Microsoft.FluentUI.AspNetCore.Components.Emoji emoji { get; set; } = new Emojis.PeopleBody.Color.MediumLight.WomanStudent();
        public SpeechSynthesisVoice _Voice { get; set; }
        public string _VoiceId { get; set; } = "Microsoft Michelle Online (Natural) - English (United States)|en-US";

        [JsonPropertyName("model")]
        public string Model { get; set; } = "gpt-oss:latest";
        [JsonPropertyName("think")]
        public string Think { get; set; } = "medium";

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
            
    }
}
