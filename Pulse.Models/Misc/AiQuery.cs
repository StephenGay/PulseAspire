using Pulse.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Misc
{
    public class AiQuery
    {
        [Key]
        [Required]
        public int AiQueryID { get; set; }
        [Required]
        public required string Question { get; set; }
        [Required]
        public required string SqlQuery { get; set; }
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        [DefaultValue(0)]
        public int UpVote { get; set; } = 0;
        [DefaultValue(0)]
        public int DownVote { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public ICollection<UserFavouriteQry>? UserFavouriteQueries { get; set; }
    }
}
