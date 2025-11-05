using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.CustomComponents
{
    public class PdfRequest
    {
        public string Html { get; set; } = "";
        public string FileName { get; set; } = "PdfExport";
    }
}
