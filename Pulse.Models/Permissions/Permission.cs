using System.ComponentModel.DataAnnotations;

namespace Pulse.Models.Permissions
{
    public class Permission
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Category { get; set; } = string.Empty; // e.g., "User Management", "Reports", "Settings"
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual PermissionCategory? CategoryNavigation { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    public class RolePermission
    {
        public int Id { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public int PermissionId { get; set; }
        
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        
        // Navigation
        public virtual Permission Permission { get; set; } = null!;
    }

    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class RolePermissionDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<PermissionDto> Permissions { get; set; } = new();
    }

    public class CreatePermissionModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string Category { get; set; } = string.Empty;
    }

    public class AssignPermissionModel
    {
        [Required]
        public string RoleId { get; set; } = string.Empty;
        
        [Required]
        public int PermissionId { get; set; }
    }

    public class RoleWithIdDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
