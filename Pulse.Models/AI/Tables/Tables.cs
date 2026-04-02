using Pulse.Models.CustomComponents;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Toolbelt.Blazor.SpeechSynthesis;
using Emojis = Microsoft.FluentUI.AspNetCore.Components.Emojis;

namespace Pulse.Models.AI.Tables
{
    public class Tables
    {
        public Microsoft.FluentUI.AspNetCore.Components.Emoji emoji { get; set; } = new Emojis.PeopleBody.Color.MediumLight.ManTechnologist();
        public SpeechSynthesisVoice? _Voice { get; set; }
        public string _VoiceId { get; set; } = "Microsoft Guy Online (Natural) - English (United States)|en-US";

        [JsonPropertyName("model")]
        public string Model { get; set; } = "gpt-oss:latest";
    }
    
}
