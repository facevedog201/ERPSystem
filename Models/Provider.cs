using System;
using System.ComponentModel.DataAnnotations;

namespace ERPSystem.Models
{
    public class Provider
    {
        [Key]
        public int ProviderId { get; set; }

        [Required, StringLength(100)]
        public string? Name { get; set; } = string.Empty;

        [StringLength(100), EmailAddress]
        public string? Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Address { get; set; } = string.Empty;

        [StringLength(255)]
        public string? RUC { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}
