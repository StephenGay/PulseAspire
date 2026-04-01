using Pulse.Models.CustomComponents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.AI
{
    public class SystemMessageData
    {
        public string? DbStructure { get; set; }
        public string? DbSchema { get; set; }
        public string? DbExamples { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyInformation { get; set; }
        public string? SqlGuidelines { get; set; }
        public string? UserName { get; set; }
        public List<OllamaMessage>? ConversationHistory { get; set; }
        public string? Tools { get; set; }
        public string? UserQuery { get; set; }
        public string? ResponseFormat { get; set; }
        public string? Error { get; set; }
    }
}
