using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.CustomComponents
{
    public record CalendarEvent
    {
        public int Id { get; init; }          // WorksOrderNo or any unique key
        public string Title { get; init; } = string.Empty;       // e.g. Client + Order
        public DateTime Start { get; init; }
        public DateTime? End { get; init; }
        public string? BackgroundColor { get; init; }
        //public string? Url { get; init; }                       // optional link to detail page
    }
}
