using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace ERPSystem.Models
{
    public class InvoiceDetail
    {
        [Key]
        public int InvoiceDetailId { get; set; } 

        public int InvoiceId { get; set; } = 0;

        [ValidateNever]
        public Invoice Invoice { get; set; } public InvoiceDetail() { }

        public int ServiceId { get; set; } = 0;

        [ValidateNever]
        public Service Service { get; set; } 

        public int Quantity { get; set; } = 1;

         public decimal Price { get; set; } = 0;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal Total { get; set; } = 0; 

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
