using System;
using System.ComponentModel.DataAnnotations;

namespace ERPSystem.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [StringLength(50)]
        public string Username { get; set; }

        [StringLength(255)]
        public string Password { get; set; }

        [StringLength(100)]
        public string FullName { get; set; }

        public UserRole Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true; // ✅ Nuevo campo para soft delete
    }


    public enum UserRole
    {
        Admin,
        Recepcion,
        Contador,
        Vendedor,
        Asistente
    }
}
