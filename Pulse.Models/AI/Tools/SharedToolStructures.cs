using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.AI.Tools
{
    public class PulseToolCall
    {
        public string ToolName { get; set; } = string.Empty;
        public Dictionary<string, object>? Parameters { get; set; }
    }
    public class PulseToolResult
    {
        public string ToolName { get; set; } = string.Empty;
        public object? Result { get; set; }
    }
}
