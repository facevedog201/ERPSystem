using ERPSystem.Models;

public class Payment
{
    public int PaymentId { get; set; } = 0;
    public int InvoiceId { get; set; } = 0;
    public Invoice Invoice { get; set; } 
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public decimal Amount { get; set; } = 0;
    public string? Notes { get; set; } = ""; // <- Acepta null y asigna cadena vacía
}
