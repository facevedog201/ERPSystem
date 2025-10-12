using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPSystem.Models
{
    public class ProviderInvoice
    {
        [Key]
        public int ProviderInvoiceId { get; set; }

        [Required]
        public int ProviderId { get; set; }

        [ForeignKey("ProviderId")]
        public Provider Provider { get; set; }
        [Required]
        public DateTime InvoiceNumber { get; set; }
        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PaidAmount { get; set; }

        [Required]
        public string Status { get; set; } // Por ejemplo: "Pendiente", "Pagada", "Parcial"

        public string Description { get; set; }
        [Required]
        public DateTime CreatedAt{ get; set; }
    }
}
