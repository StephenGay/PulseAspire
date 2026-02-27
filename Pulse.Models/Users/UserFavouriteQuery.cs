using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pulse.Models.Users
{
    public class UserFavouriteQuery
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public int QueryId { get; set; }
    }
}
