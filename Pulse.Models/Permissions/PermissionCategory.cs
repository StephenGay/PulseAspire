using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pulse.Models.Permissions;

public class PermissionCategory
{
    [Key]
    [MaxLength(100)]
    public required string Category { get; set; }
}
