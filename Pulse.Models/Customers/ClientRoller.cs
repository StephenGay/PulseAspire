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
    public class ClientRoller
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ClientRollerID { get; set; }
        [Required]
        public int ClientRollerSpecificationID { get; set; }
        [Required]
        [MaxLength(50)]
        public string ClientRollerNumber { get; set; } = string.Empty;
        [Required]
        public decimal ShellDiameter { get; set; } = 0;
        [Required]
        public decimal ShellLength { get; set; } = 0;
        [Required]
        public decimal ShellWeight { get; set; } = 0;
        [Required]
        public decimal CoverDiameter { get; set; } = 0;
        public string? ShellDefects { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        public ClientRollerSpecification? ClientRollerSpecification { get; set; }

    }
}
