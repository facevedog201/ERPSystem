using System;
using System.Collections.Generic;
using System.Linq;

namespace ERPSystem.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; } = 0;
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        public List<InvoiceDetail> InvoiceDetails { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();

        public void UpdateStatus()
        {
            if (PaidAmount <= 0)
                Status = InvoiceStatus.Pending;
            else if (PaidAmount < Total)
                Status = InvoiceStatus.Partial;
            else
                Status = InvoiceStatus.Paid;
        }
    }

    public enum InvoiceStatus
    {
        Pending,
        Partial,
        Paid
    }
}
