using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pulse.Models.CustomComponents
{
    public class PulseMessage
    {
        [Key]
        public int Id { get; set; }
        public string? RecipientUserId { get; set; }
        public string? SenderUserId { get; set; }
        public string? SenderUserName { get; set; }
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "user"; // "user" or "assistant" or "group"
        [Required]
        public string Content { get; set; } = string.Empty;
        [Required]
        [MaxLength(10)]
        public string ContentType { get; set; } = "HTML"; // "HTML","JSON"
        public DateTime? SentAt { get; set; } = DateTime.UtcNow;
        public bool? Delivered { get; set; } = false;
    }
}
