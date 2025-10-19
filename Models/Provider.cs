using System;
using System.ComponentModel.DataAnnotations;

namespace ERPSystem.Models
{
    public class Provider
    {
        [Key]
        public int ProviderId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(100), EmailAddress]
        public string Email { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}
