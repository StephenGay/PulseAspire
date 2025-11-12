using Pulse.Models.Misc;
using Pulse.Models.Organizational;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Production
{
    public class EquipmentItem
    {
        [Key]
        [MaxLength(10)]
        public string EquipmentItemID { get; set; }
        [MaxLength(20)]
        public string? FixedAssetNo { get; set; }
        [MaxLength(5)]
        [Required]
        public string DivisionID { get; set; }
        [MaxLength(4)]
        public string? EquipmentCategoryID { get; set; }
        [MaxLength(50)]
        public string? EquipmentItemDescription { get; set; }
        [MaxLength (50)]
        public string? ManufacturerName { get; set; }
        [Required]
        [DefaultValue(1)]
        public decimal EquipmentDifficultyMultiplier { get; set; } = 1;
        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;
        public EquipmentCategory? EquipmentCategory { get; set; }
        public Division? Division { get; set; }
        public ICollection<EquipmentCapability>? EquipmentCapabilities { get; set; }
    }
}
