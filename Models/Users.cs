using System;
using System.ComponentModel.DataAnnotations;

namespace ERPSystem.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; }

        [StringLength(255)]
        public string Password { get; set; }

        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        public UserRole Role { get; set; } // 👈 aquí usamos el enum directamente

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum UserRole
    {
        Admin,
        Recepcion,
        Contador
    }
}
