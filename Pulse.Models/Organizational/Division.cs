using Pulse.Models.Production.WorkTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Organizational
{
    public class Division
    {
        [Key]
        [MaxLength(5)]
        [Required]
        public required string DivisionID { get; set; }
        [Required]
        [MaxLength(100)]
        public required string DivisionName { get; set; }
        public bool IsActive { get; set; } = true;
        [Required]
        public required int CompanyID { get; set; }
        [Required]
        public required int BranchID { get; set; }
        
        public Company? Company { get; set; }
        
        public Branch? Branch { get; set; }
        public ICollection<DivisionWorkType> DivisionWorkTypes { get; set; } = new List<DivisionWorkType>();
    }

}

