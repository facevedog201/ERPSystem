using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPSystem.Models
{
    public class InvoiceDetail
    {
        [Key]
        public int InvoiceDetailId { get; set; }

        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; }

        public int Quantity { get; set; } = 1;

         public decimal Price { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal Total { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
