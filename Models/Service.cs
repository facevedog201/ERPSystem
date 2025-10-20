using System;
using System.ComponentModel.DataAnnotations;

namespace ERPSystem.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; } 

        [StringLength(100)]
        public string Name { get; set; }  // se agrego la propiedad required

        [StringLength(500)]
        public string Description { get; set; }  //se agrego la propiedad required

        public decimal Price { get; set; } = 0m;

        public ServiceType Type { get; set; }  // Tipo de servicio: transporte, logística, otro

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
