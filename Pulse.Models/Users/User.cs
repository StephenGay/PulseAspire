using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pulse.Models.Users
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int UserID { get; set; }
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? Email { get; set; } = string.Empty;
        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;
        [Required]
        [MaxLength(100)]
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        public ICollection<UserFavouriteQry>? UserFavouriteQueries { get; set; } = new HashSet<UserFavouriteQry>();

        public UserSettings UserSettings { get; set; } = new UserSettings();

    }
}
//public class RegisterModel { public string Email { get; set; } public string Password { get; set; } }
//public class LoginModel { public string Email { get; set; } public string Password { get; set; } }