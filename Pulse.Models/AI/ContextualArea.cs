using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Pulse.Models.AI
{
    public class ContextualArea
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ContextAreaId { get; set; }
        [Required]
        [MaxLength(100)]
        public string AreaDescription { get; set; }

        public ICollection<ContextualPrompt> ContextualPrompts { get; set; }
    }
}
