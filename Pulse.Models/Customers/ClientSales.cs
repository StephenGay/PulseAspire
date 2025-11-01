using Pulse.Models.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Customers
{
    public class ClientSales
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(10)]
        public required string FullClientID  { get; set; }
        [Required]
        public required int PeriodID { get; set; }
        [Required]
        public required int CompanyID { get; set; }
        [Required]
        [DefaultValue(0)]
        public required decimal Amount { get; set; }
        public Customer? Customer { get; set; }
        public Period? Period { get; set; }

    }
}
