using Pulse.Models.Production;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Misc
{
    public class EquipmentCategory
    {
        [Key]
        [MaxLength(4)]
        public string EquipmentCategoryID { get; set; }
        [Required]
        [MaxLength(50)]
        public string EquipmentCategoryName { get; set; }
        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;
        public ICollection<EquipmentItem>? EquipmentItems { get; set; }
    }
}
