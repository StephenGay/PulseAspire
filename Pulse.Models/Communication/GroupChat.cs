using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Pulse.Models.Communication;

public class GroupChat
{
    [Required]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int GroupID { get; set; }
    [MaxLength(50)]
    [Required]
    public string GroupName { get; set; }
    [MaxLength(256)]
    public string? Description { get; set; }
    [Required]
    [MaxLength(5)]
    public string DivisionID { get; set; } = "Any";
    [Required]
    public string? OwnerID { get; set; }
    public bool IsPrivateGroup { get; set; } = false;
}
