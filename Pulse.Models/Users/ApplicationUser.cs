using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
    public PresenceStatus PresenceStatus { get; set; } = PresenceStatus.Offline;
}

// Models/ApplicationDbContext.cs

