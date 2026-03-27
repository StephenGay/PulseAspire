using Pulse.Models.Organizational;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Pulse.Models.Production.WorkTypes;

public class DivisionWorkType
{
        [Key, Column(Order = 0)]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WorkTypeID { get; set; }

        [Key, Column(Order = 1)]
        [Required]
        [MaxLength(5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required string DivisionId { get; set; }
        
        [DefaultValue(0)]
        public int TargetWorkingDays { get; set; }
        
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public WorkType? WorkType { get; set; }
        public Division? Division { get; set; }

        // New back-navigation to ProductionStages
        public ICollection<ProductionStage> ProductionStages { get; set; } = new List<ProductionStage>();
}
