using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Customers
{
    public record ClientCurrentStats
    {
        [Key]
        [MaxLength(10)]
        public string? FullClientID { get; set; }
        public bool isSet { get; set; } = false;
        public int countWipWOs { get; set; } = 0;
        public int countOpenQuotes { get; set; } = 0;
    }
}
