using Pulse.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Pulse.Models.Communication;

public class SystemDataStream
{
    [Required]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int StreamID { get; set; }
    [Required]
    [MaxLength(50)]
    public string? StreamName { get; set; }
    [MaxLength(256)]
    public string? Description { get; set; }

}
