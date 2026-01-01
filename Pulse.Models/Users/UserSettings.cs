using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Pulse.Models.Users
{
    public class UserSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }
        [DefaultValue(false)]
        public bool AIHasVoice { get; set; } = false;
        [MaxLength(300)]
        public string? AIVoiceID { get; set; } = string.Empty;
        [DefaultValue(0)]
        [Required]
        public int AIDefaultPref { get; set; } = 0;
        [MaxLength(50)]
        public string UserTheme { get; set; }
        public User? User { get; set; } = null!;
    }
}
