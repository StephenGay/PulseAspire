using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.CustomComponents
{
    public class EmailConfiguration
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Pulse Aspire";
        public bool EnableSsl { get; set; } = true;
    }
}
