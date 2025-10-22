using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;

namespace ERPSystem.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        public int ClientId { get; set; }   // clave foránea (columna en la tabla)

        [ValidateNever]
        public Client Client { get; set; }    // navegación al objeto Client


        public DateTime? InvoiceDate { get; set; } = DateTime.Now;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public decimal? PaidAmount { get; set; } = 0;
        public int? Status { get; set; } = (int)InvoiceStatus.Pending;
        public bool? IsActive { get; set; } = true;
        public int? InvoiceNumber { get; set; } = 0;
        public int? Type { get; set; } = 0; // 0=Credito, 1=Contado, etc.
        public decimal? ExchangeRate { get; set; } = 1;
        public string? Reference { get; set; } = string.Empty;
        public decimal? CIF { get; set; } = 0;
        public string? Shipping { get; set; } = string.Empty;
        public string? AmountInWords { get; set; } = string.Empty;
        public decimal? Total { get; set; } = 0;
        public decimal? SubTotal { get; set; } = 0;
        public decimal? TaxAmount { get; set; } = 0;
        public string? Observations { get; set; } = string.Empty;

        // Relaciones
        public List<InvoiceDetail> InvoiceDetails { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();

        public void UpdateStatus()
        {
            if (IsActive == false)
            {
                Status = (int)InvoiceStatus.Cancelled;
            }
            else if (PaidAmount <= 0)
                Status = (int)InvoiceStatus.Pending;
            else if (PaidAmount < Total)
                Status = (int)InvoiceStatus.Partial;
            else
                Status = (int)InvoiceStatus.Paid;
        }
        public string NumberToWords(decimal number)
        {
            if (number == 0)
                return "cero con cero";

            if (number < 0)
                return "menos " + NumberToWords(Math.Abs(number));

            long integerPart = (long)Math.Floor(number);
            int decimalPart = (int)Math.Round((number - integerPart) * 100);

            string[] units = { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve", "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve" };
            string[] tens = { "", "", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
            string[] hundreds = { "", "cien", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos" };

            Func<long, string> toWords = null;
            toWords = n =>
            {
                if (n == 0)
                    return "cero";
                if (n < 0)
                    return "menos " + toWords(Math.Abs(n));
                if (n < 20)
                    return units[n];
                if (n < 100)
                {
                    if (n % 10 == 0)
                        return tens[n / 10];
                    if (n < 30)
                        return "veinti" + units[n % 10];
                    return tens[n / 10] + " y " + units[n % 10];
                }
                if (n < 1000)
                {
                    if (n == 100)
                        return "cien";
                    if (n % 100 == 0)
                        return hundreds[n / 100];
                    return hundreds[n / 100] + " " + toWords(n % 100);
                }
                if (n < 1000000)
                {
                    if (n / 1000 == 1)
                        return "mil" + (n % 1000 > 0 ? " " + toWords(n % 1000) : "");
                    return toWords(n / 1000) + " mil" + (n % 1000 > 0 ? " " + toWords(n % 1000) : "");
                }
                if (n < 2000000)
                    return "un millón" + (n % 1000000 > 0 ? " " + toWords(n % 1000000) : "");
                if (n < 1000000000000)
                    return toWords(n / 1000000) + " millones" + (n % 1000000 > 0 ? " " + toWords(n % 1000000) : "");
                return n.ToString();
            };

            string result = toWords(integerPart) + " con " + (decimalPart > 0 ? toWords(decimalPart) : "cero");
            return result;
        }
    }

        public enum InvoiceStatus
    {
        Pending,
        Partial,
        Paid,
        Cancelled,
        Anulada,
        Pagada,
        Parcial,
        Pendiente,
        Overdue,
        NoStatus
    }
}
