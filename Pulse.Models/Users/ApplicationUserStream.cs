using Pulse.Models.Communication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pulse.Models.Users;

public class ApplicationUserStream
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string ApplicationUserID { get; set; }
    [Required]
    public StreamType StreamType { get; set; } = StreamType.ChatGroup;
    [Required]
    public int StreamID { get; set; }
    [MaxLength(5)]
    [Required]    
    public string StreamDivisionID { get; set; }
    [MaxLength(50)]
    public string? StreamName { get; set; }
}
public enum StreamType
{
    System,
    ChatGroup
}