using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pulse.Models.Users
{
    public class ApplicationUserSpeedDial
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string ApplicationUserId { get; set; }
        [Required]
        public string Category { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        public string ItemCode { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; } = string.Empty;
    }
}
