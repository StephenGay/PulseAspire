using Pulse.Models.Compounds;
using Pulse.Models.Customers;
using Pulse.Models.Misc;
using Pulse.Models.Organizational;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Production
{
    public class WorksOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WorksOrderNo { get; set; }
        [Required]
        [MaxLength(10)]
        public required string FullClientID { get; set; }
        [Required]
        public required int PeriodID { get; set; }
        public DateTime DateStarted { get; set; }
        public string? Description { get; set; }
        [Required]
        public required int WorkTypeID { get; set; }
        [Required]
        public required decimal Quantity { get; set; }
        [Required]
        [MaxLength(5)]
        public string DivisionID { get; set; }
        public int? ClientRollerSpecificationID { get; set; }
        public int? ClientRollerID { get; set; }
        [MaxLength(50)]
        public string? ClientRollNo { get; set; }
        [MaxLength(4)]
        public string? CoverCompoundCode { get; set; }
        public string? ClientOrderNo { get; set; }
        public string? ClientPRNo { get; set; }
        public string? ClientRFQNo { get; set; }
        public decimal? ShellLength { get; set; }
        public decimal? ShellDiameter { get; set; }
        public decimal? CoverDiameter { get; set; }
        public decimal UndelQty { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal SellPrice { get; set; }
        public string? Status { get; set; }

        public ClientRollerSpecification? ClientRollerSpecification { get; set; }
        public ClientRoller? ClientRoller { get; set; }
        public Customer? Customer { get; set; }
        public Compound? Compound { get; set; }
        public WorkType? WorkType { get; set; }
        public Period? Period { get; set; }
        public Division? Division { get; set; }
    }
}
