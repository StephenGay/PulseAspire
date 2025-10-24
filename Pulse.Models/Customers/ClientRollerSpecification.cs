using Pulse.Models.Compounds;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Customers
{
    public class ClientRollerSpecification
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ClientRollerSpecificationID { get; set; }
        [Required]
        [MaxLength(10)]
        public string FullClientID { get; set; } = string.Empty;

        // General Details

        [MaxLength(100)]
        public string? Description { get; set; }
        public int? RollerType { get; set; }
        public string? RollerFunction { get; set; }
        public int? ProcessID { get; set; }
        [MaxLength(100)]
        public string? ProcessName { get; set; }
        [MaxLength(100)]
        public string? MachineType { get; set; }

        // Reference Details
        [MaxLength(50)]
        public string? ArticleNumber { get; set; }
        [MaxLength(50)]
        public string? DrawingNumber { get; set; }

        // Shell Details
        [DefaultValue(0)]
        public decimal? ShellDiameter { get; set; } = 0;
        [DefaultValue(0)]
        public decimal? ShellLength { get; set; } = 0;
        [DefaultValue(0)]
        public decimal? ShellWeight { get; set; } = 0;
        [DefaultValue(0)]
        public decimal? ShellMinimumDiameter { get; set; } = 0;
        public int? ShellTypeID { get; set; }
        [MaxLength(100)]
        public string? ShellTypeName { get; set; }

        // Cover Details
        [MaxLength(4)]
        public string? CompoundCode { get; set; }
        public int? HardnessRequired { get; set; }
        [DefaultValue(0)]
        public int? HardnessToleranceAllowed { get; set; }
        [MaxLength(50)]
        public string? HardnessTypeRequired { get; set; }
        [DefaultValue(0)]
        public decimal? CoverLength { get; set; } = 0;
        [DefaultValue(0)]
        public decimal? CoverDiameter { get; set; } = 0;
        [DefaultValue(0)]
        public decimal? CoverLeftOffset { get; set; } = 0;
        [DefaultValue(0)]
        public decimal? CoverMinimumDiameter { get; set; } = 0;
        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        public Customer? Customer { get; set; }
        public List<ClientRoller>? ClientRollers { get; set; }
        public Compound? Compound { get; set; }

    }
}
