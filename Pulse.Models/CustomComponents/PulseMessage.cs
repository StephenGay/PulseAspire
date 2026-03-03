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
        [MaxLength(256)]
        public string? RecipientUserName { get; set; }
        public string? SenderUserId { get; set; }
        [MaxLength(256)]
        public string? SenderUserName { get; set; }
        [MaxLength(200)]
        public string? Subject { get; set; }
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "user"; // "user", "pulseai", "stream" or notification
        [Required]
        public string Content { get; set; } = string.Empty;
        [Required]
        [MaxLength(10)]
        public string ContentType { get; set; } = "HTML"; // "HTML","JSON","MARKUP" or "TEXT"
        public DateTime? SentAt { get; set; } = DateTime.UtcNow;
        public bool? Delivered { get; set; } = false;
    }
}
