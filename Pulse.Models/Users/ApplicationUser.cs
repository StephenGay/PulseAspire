using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.Users
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }

    // Models/ApplicationDbContext.cs
   
}
