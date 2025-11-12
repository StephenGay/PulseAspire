using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Production
{
    public class EquipmentCapability
    {
        [Required]
        [MaxLength(10)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order = 0)]
        public string EquipmentItemID { get; set; }
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order = 1)]
        public int ProductionStageID { get; set; }

        public EquipmentItem? EquipmentItem { get; set; }
        public ProductionStage? ProductionStage { get; set; }
    }
}
