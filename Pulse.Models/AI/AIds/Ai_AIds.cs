using Emojis = Microsoft.FluentUI.AspNetCore.Components.Emojis;
using System;
using System.Collections.Generic;
using System.Text;
using Toolbelt.Blazor.SpeechSynthesis;

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
    }
}
