using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Models.Users
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
