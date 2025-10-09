using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ERPSystem.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        public decimal Total { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }
    }
}
