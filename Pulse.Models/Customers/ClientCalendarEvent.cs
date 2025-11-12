using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Customers
{
    public class ClientCalendarEvent
    {
        [Key]
        public int EventId { get; set; }
        [MaxLength(50)]
        public string Title { get; set; } = string.Empty;       // e.g. Client + Order
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        [MaxLength(10)]
        public string? FullClientID { get; set; }
        public int? ClientSpecificationID { get; set; }
        [MaxLength(50)]
        public string? ClientRollerNumber { get; set; } 
        [MaxLength(40)]
        public string? ClientName { get; set; }
        [MaxLength(50)]
        public string? Category { get; set; }
        public Customer? Customer { get; set; }
    }
}
