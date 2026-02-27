using Pulse.Models.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Users
{
    public class UserFavouriteQry
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public int QueryId { get; set; }

        public AiQuery? AiQuery { get; set; }

    }
}
