using System.ComponentModel.DataAnnotations;

namespace ERPSystem.ViewModels
{
    public class ProviderPaymentViewModel
    {
        public int ProviderInvoiceId { get; set; }
        public int ProviderId { get; set; }
        public string? ProviderName { get; set; }
        public string? InvoiceNumber { get; set; }
        public decimal Outstanding { get; set; }

        [Required, Display(Name = "Monto a pagar")]
        public decimal Amount { get; set; }

        [Required, DataType(DataType.Date), Display(Name = "Fecha de pago")]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Nota")]
        public string? Note { get; set; }
    }
}
