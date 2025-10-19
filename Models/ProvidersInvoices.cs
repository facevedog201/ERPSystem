using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPSystem.Models
{
    public class ProviderInvoices
    {
        [Key]
        public int ProviderInvoiceId { get; set; }
        public int ProviderId { get; set; }

        [ForeignKey("ProviderId")]
        [ValidateNever]
        public Provider? Provider { get; set; }
        [Required(ErrorMessage = "El número de factura es obligatorio.")]
        [StringLength(50)]
        public string InvoiceNumber { get; set; }

        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);

        public DateTime InvoiceDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "El monto total es obligatorio.")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto total no puede ser negativo.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0m;

        [Range(0, double.MaxValue, ErrorMessage = "El monto pagado no puede ser negativo.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0m;

        public string Status { get; set; } = "Pendiente"; // Por ejemplo: "Pendiente", "Pagada", "Parcial"

        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
