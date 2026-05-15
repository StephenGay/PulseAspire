using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Pulse.Models.Users
{
    public class ApplicationUserSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }
        [Required]
        public string PreferredUserName { get; set; }

        [DefaultValue(false)]
        public bool AIHasVoice { get; set; } = false;
        [MaxLength(300)]
        public string? AIVoiceID { get; set; } = string.Empty;
        
        [MaxLength(50)]
        [Required]
        [DefaultValue("pulse")]
        public string UserTheme { get; set; } = "pulse";
        //[DefaultValue(false)]
        //public bool UseClientSpeedDial { get; set; } = false;

    }
}
