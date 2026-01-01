using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pulse.Models.Users
{
    //public class LoginModel
    //{
    //    [Required]
    //    [EmailAddress]
    //    public string Email { get; set; }

    //    [Required]
    //    [MinLength(6)]
    //    public string Password { get; set; }
    //}
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        // Add custom properties from ApplicationUser
        public string FullName { get; set; }
    }

    public class CreateRoleModel
    {
        [Required]
        public string Name { get; set; }
    }

    //public class AssignRoleModel
    //{
    //    [Required(ErrorMessage = "The RoleName field is required.")]
    //    public string RoleName { get; set; }
    //}

    public class AssignRoleModel
    {
        public string UserId { get; set; }
        public string RoleName { get; set; }
    }
}
