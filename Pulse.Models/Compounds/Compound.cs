using Pulse.Models.Customers;
using Pulse.Models.Industries;
using Pulse.Models.Production;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Compounds
{
    public class Compound
    {
        [Key]
        [Required]
        [MaxLength(4)]
        public required string CompoundCode { get; set; }
        [MaxLength(3)]
        public string? RevisionCode { get; set; }
        [Required]
        [MaxLength(50)]
        public required string CompoundDescription { get; set; }
        public int? CompoundRangeId { get; set; }
        [MaxLength(50)]
        public string? PolymerName { get; set; }
        public int? HardnessMeasurement { get; set; }
        public int? HardnessTolerance { get; set; }
        [MaxLength(50)]
        public string? HardnessType { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal CalculatedSpecificGravity { get; set; }
        [DefaultValue(false)]
        public bool OverRideSpecificGravity { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal SpecificGravity { get; set; }
        [MaxLength(100)]
        public string? Colour { get; set; }
        [Required]
        [MaxLength(20)]
        public required string CompoundType { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal CalculatedCostPerKg { get; set; }
        [DefaultValue(false)]
        public bool OverRideCostPerKg { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal CostingCostPerKg { get; set; }
        [DefaultValue(0)]
        public decimal RoyaltyCharge { get; set; }
        [DefaultValue(0)]
        public decimal CarbonBlackCharge { get; set; }
        [DefaultValue(false)]
        public bool IsImported { get; set; }
        [DefaultValue(0)]
        public decimal CustomSaleFactor { get; set; }
        [Required]
        [MaxLength(20)]
        public string State { get; set; }
        [Required]
        public DateTime RevisionDate { get; set; }
        [Required]
        [MaxLength(100)]
        public string RevisedBy { get; set; }
        [Required]
        [MaxLength(255)]
        public string RevisionReason { get; set; }
        public DateTime? DateCostUpdated { get; set; }
        public CompoundRange? CompoundRange { get; set; }
        public ICollection<IndustryRecommendedCover>? IndustryRecommendedCovers { get; set; } = new HashSet<IndustryRecommendedCover>();
        public ICollection<ClientRollerSpecification>? ClientRollerSpecifications { get; set; } = new HashSet<ClientRollerSpecification>();
        public ICollection<WorksOrder>? WorksOrders { get; set; } = new HashSet<WorksOrder>();



    }
}
