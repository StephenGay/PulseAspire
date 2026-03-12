using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.Communication
{
    public class EMailMessage
    {
        public required string ToEMailAddress { get; set; }
        public string? ToEMailName { get; set; }
        public string? Subject { get; set; }
        public required string HtmlBody { get; set; }
    }
}
