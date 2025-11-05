using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Production
{
    public class WorkType
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WorkTypeID { get; set; }
        [MaxLength(100)]
        [Required]
        public string WorkTypeName { get; set; }
        [DefaultValue(0)]
        public int TargetWorkingDays { get; set; }
        [MaxLength(50)]
        public string WorkTypeMaterial { get; set; }
        [MaxLength(3)]
        public string? LockToProductCode { get; set; }
        public int SortOrder { get; set; }
        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

    }
}
