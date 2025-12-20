using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pulse.Models.Production
{
    public class WorkOrderUpdateDto
    {
        public string? Status { get; set; }
        public int? ProductionStageID { get; set; }
        public DateTime? ProgressChange { get; set; }
        public string? ProgressComment { get; set; }
        public string? ProductionStageName { get; set; }
    }
}
