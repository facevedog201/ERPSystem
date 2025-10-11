namespace ERPSystem.ViewModels
{
    public class FacturaViewModel
    {
        public int InvoiceId { get; set; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string RUC { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int? InvoiceNumber { get; set; }
        public int? Type { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string Reference { get; set; }
        public decimal? CIF { get; set; }
        public string Shipping { get; set; }
        public string AmountInWords { get; set; }
        public int Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? Total { get; set; }
        public decimal? TotalInvoice { get; set; }
        public decimal? SubTotalInvoice { get; set; }
        public decimal? TaxInvoice { get; set; }
        public string InvoiceObservations { get; set; }
        public int ServiceId { get; set; }
        public int InvoiceDetailId { get; set; }
    }
}
