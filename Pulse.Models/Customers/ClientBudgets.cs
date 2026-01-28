using Pulse.Models.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Pulse.Models.Customers
{
    public class ClientBudgets
    {
        [Key]
        public int ClientBudgetId { get; set; }

        [Required]
        [MaxLength(10)]
        public string FullClientId { get; set; }

        [Required]
        public int PeriodID { get; set; }
        [Required, MaxLength(4)]
        public string FinancialYear { get; set; }

        [Required]
        [DefaultValue(0)]
        public decimal BudgetedSales { get; set; } = 0;

        [Required]
        [DefaultValue(0)]
        public decimal AIForecast { get; set; } = 0;

        public Customer? customer { get; set; }
        public Period? period { get; set; }
    }

    public class BlankClientBudget
    {
        public string FullClientId { get; set; }

        public int PeriodID { get; set; }
        public string Month { get; set; }
        public string CalenderYear { get; set; }

        public string FinancialYear { get; set; }

        [Required]
        [DefaultValue(0)]
        public decimal BudgetedSales { get; set; } = 0;
        [Required]
        [DefaultValue(0)]
        public decimal AIForecast { get; set; } = 0;
    }
}
