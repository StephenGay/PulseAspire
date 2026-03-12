using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pulse.Models.Users;

public enum PresenceStatus
{
    Unknown = -1,
    Busy = 0,
    OutOfOffice = 1,
    Away = 2,
    Available = 3,
    Offline = 4,
    DoNotDisturb = 5
}
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }
    public bool RequirePwdChange { get; set; }
    public PresenceStatus PresenceStatus { get; set; } = PresenceStatus.Offline;
}

//public class ApplicationRole : IdentityRole
//{
//    // Add your new field here
//    public string? Description { get; set; }
//}
public class ApplicationUserDto
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }
    public string UserName { get; set; }
    public bool EmailConfirmed { get; set; } = false;
    public string? PhoneNumber { get; set; }
    public bool TwoFactorEnabled { get; set; } = false;
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; } = true;
    public int AccessFailedCount { get; set; } = 0;
    [Required(ErrorMessage = "Full name is required")]
    public string? FullName { get; set; }
    public bool RequirePwdChange { get; set; } = true;
    public List<string> Roles { get; set; } = new();

}
// Models/ApplicationDbContext.cs

