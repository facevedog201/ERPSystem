using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPSystem.Models
{
    public class ProviderInvoice
    {
        [Key]
        public int ProviderInvoiceId { get; set; }
        public int ProviderId { get; set; }

        [ForeignKey("ProviderId")]
        [ValidateNever]
        public Provider? Provider { get; set; }
        public String? InvoiceNumber { get; set; }
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);

        public DateTime InvoiceDate { get; set; } = DateTime.Today;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0m;

        public string Status { get; set; } = "Pendiente"; // Por ejemplo: "Pendiente", "Pagada", "Parcial"

        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
