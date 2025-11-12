using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Production
{
    public class WorkCentreFunctions
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order =0)]
        public int WorkCentreID { get; set; }
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key, Column(Order = 1)]
        public int ProductionStageID { get; set; }

        public WorkCentre? WorkCentre { get; set; }
        public ProductionStage? ProductionStage { get; set; }
    }
}
