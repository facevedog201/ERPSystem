using System;
using System.ComponentModel.DataAnnotations;

namespace ERPSystem.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public ServiceType Type { get; set; } // Tipo de servicio: transporte, logística, otro

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }

    public enum ServiceType
    {
        Transporte,
        Logistica,
        Aduana,
        Otros
    }
}
