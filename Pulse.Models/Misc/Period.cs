using Pulse.Models.Customers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Misc
{
    public class Period
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required int PeriodID { get; set; }
        [Required]
        [MaxLength(15)]
        public required string Month { get; set; }
        [Required]
        [MaxLength(4)]
        public required string CalendarYear { get; set; }
        [Required]
        [MaxLength(4)]
        public required string FinancialYear { get; set; }
        [Required]
        public required DateTime StartDate { get; set; }
        public ICollection<ClientSale>? ClientSales { get; set; }

    }
}
