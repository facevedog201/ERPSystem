using ERPSystem.Models;

public class Payment
{
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public decimal Amount { get; set; }
    public string? Notes { get; set; } = ""; // <- Acepta null y asigna cadena vacía
}
