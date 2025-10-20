using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPSystem.Models
{
    [Table("Clients")]
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [StringLength(100)]
        [Display(Name = "Nombre del Cliente")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico no válido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Dirección")]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "RUC / NIT / Identificación Fiscal")]
        public string? RUC { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Fecha de Creación")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Activo")]
        public bool? IsActive { get; set; } = true;
    }
}
