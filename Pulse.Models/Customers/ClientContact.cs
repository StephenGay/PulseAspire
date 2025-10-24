using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Models.Customers
{
    public class ClientContact
    {
        [Key]
        [Required]

        public int ClientContactId { get; set; }
        [Required]
        [MaxLength(10)]
        public required string FullClientID { get; set; }
        [Required]
        [MaxLength(100)]
        public required string FullName { get; set; }
        [MaxLength(50)]
        public string? FirstName { get; set; }
        [MaxLength(50)]
        public string? LastName { get; set; }
        [MaxLength(100)]
        public string? ContactTitle { get; set; }
        [MaxLength(255)]
        public string? ContactEmail { get; set; }
        [MaxLength(20)]
        public string? ContactPhone { get; set; }
        [MaxLength(20)]
        public string? ContactFax { get; set; }
        [MaxLength(20)]
        public string? ContactMobile { get; set; }
        [MaxLength(100)]
        public string? ContactDepartment { get; set; }
        public DateTime? Birthday { get; set; }
        [MaxLength(6)]
        public string? Gender { get; set; }
        [MaxLength(100)]
        public string? Nickname { get; set; }
        [DefaultValue(true)]
        public bool IsActive { get; set; }
        [MaxLength(5)]
        public string? DivisionId { get; set; }
        public int? DivisionLineId { get; set; }
        [Required]
        public Customer Customer { get; set; }

    }
}
