using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pulse.Models.AI
{
    public class ContextualPrompt
    {
        [Key]
        public int ContextualPromptId { get; set; }
        [Required]
        public int ContextualAreaId { get; set; }
        [Required]
        [MaxLength(50)]
        public string ContextualPromptTitle { get; set; }
        [Required]
        [MaxLength(150)]
        public string ContextualPromptDescription { get; set; }
        [Required]
        [MaxLength(2000)]
        public string Prompt { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        [DefaultValue(0)]
        public int Likes { get; set; }= 0;

        public ContextualArea ContextualArea { get; set; }
    }
}
