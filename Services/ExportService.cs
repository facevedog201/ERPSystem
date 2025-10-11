using ClosedXML.Excel;
using ERPSystem.Data;
using ERPSystem.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ERPSystem.Services
{
    public class ExportService
    {
        private readonly AppDbContext _context;

        public ExportService(AppDbContext context)
        {
            _context = context;
        }

        // ----- EXCEL: Ventas (Invoices) -----
        public byte[] ExportSalesToExcel(IEnumerable<Invoice> invoices, string sheetName = "Ventas")
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            // Header
            ws.Cell(1, 1).Value = "Factura";
            ws.Cell(1, 2).Value = "Cliente";
            ws.Cell(1, 3).Value = "Fecha";
            ws.Cell(1, 4).Value = "Total";
            ws.Cell(1, 5).Value = "Pagado";
            ws.Cell(1, 6).Value = "Pendiente";
            ws.Cell(1, 7).Value = "Estado";

            int row = 2;
            foreach (var inv in invoices)
            {
                ws.Cell(row, 1).Value = inv.InvoiceId;
                ws.Cell(row, 2).Value = inv.Client?.Name ?? "";
                ws.Cell(row, 3).Value = inv.InvoiceDate?.ToString("yyyy-MM-dd") ?? "";
                ws.Cell(row, 4).Value = Convert.ToDouble(inv.Total ?? 0);
                ws.Cell(row, 5).Value = Convert.ToDouble(inv.PaidAmount ?? 0);
                ws.Cell(row, 6).Value = Convert.ToDouble((inv.Total ?? 0) - (inv.PaidAmount ?? 0));
                ws.Cell(row, 7).Value = inv.Status?.ToString() ?? "";
                row++;
            }

            ws.RangeUsed().SetAutoFilter();
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // ----- EXCEL: Clients summary -----
        public byte[] ExportClientsToExcel(IEnumerable<(string Client, int Count, decimal Total)> rows, string sheetName = "Clientes")
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            ws.Cell(1, 1).Value = "Cliente";
            ws.Cell(1, 2).Value = "Cantidad Facturas";
            ws.Cell(1, 3).Value = "Total";

            int r = 2;
            foreach (var row in rows)
            {
                ws.Cell(r, 1).Value = row.Client;
                ws.Cell(r, 2).Value = row.Count;
                ws.Cell(r, 3).Value = Convert.ToDouble(row.Total);
                r++;
            }

            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // ----- EXCEL: Services summary -----
        public byte[] ExportServicesToExcel(IEnumerable<(string Service, int Quantity, decimal Total)> rows, string sheetName = "Servicios")
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            ws.Cell(1, 1).Value = "Servicio";
            ws.Cell(1, 2).Value = "Cantidad";
            ws.Cell(1, 3).Value = "Total";

            int r = 2;
            foreach (var row in rows)
            {
                ws.Cell(r, 1).Value = row.Service;
                ws.Cell(r, 2).Value = row.Quantity;
                ws.Cell(r, 3).Value = Convert.ToDouble(row.Total);
                r++;
            }

            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // ----- EXCEL: Payments -----
        public byte[] ExportPaymentsToExcel(IEnumerable<Payment> payments, string sheetName = "Pagos")
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            ws.Cell(1, 1).Value = "Fecha";
            ws.Cell(1, 2).Value = "Factura";
            ws.Cell(1, 3).Value = "Cliente";
            ws.Cell(1, 4).Value = "Monto";
            ws.Cell(1, 5).Value = "Notas";

            int r = 2;
            foreach (var p in payments)
            {
                ws.Cell(r, 1).Value = p.PaymentDate.ToString("yyyy-MM-dd");
                ws.Cell(r, 2).Value = p.InvoiceId;
                ws.Cell(r, 3).Value = p.Invoice?.Client?.Name ?? "";
                ws.Cell(r, 4).Value = Convert.ToDouble(p.Amount);
                ws.Cell(r, 5).Value = p.Notes ?? "";
                r++;
            }

            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // ----- PDF (simple table) - generic for invoices -----
        public byte[] ExportInvoicesToPdf(IEnumerable<Invoice> invoices, string title = "Reporte de Facturas")
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            document.Add(new Paragraph(title).SetFontSize(14));

            var table = new Table(7).UseAllAvailableWidth();
            table.AddHeaderCell("Factura");
            table.AddHeaderCell("Cliente");
            table.AddHeaderCell("Fecha");
            table.AddHeaderCell("Total");
            table.AddHeaderCell("Pagado");
            table.AddHeaderCell("Pendiente");
            table.AddHeaderCell("Estado");

            foreach (var inv in invoices)
            {
                table.AddCell(inv.InvoiceId.ToString());
                table.AddCell(inv.Client?.Name ?? "");
                table.AddCell(inv.InvoiceDate?.ToString("yyyy-MM-dd") ?? "");
                table.AddCell((inv.Total ?? 0).ToString("C", CultureInfo.CurrentCulture));
                table.AddCell((inv.PaidAmount ?? 0).ToString("C", CultureInfo.CurrentCulture));
                table.AddCell(((inv.Total ?? 0) - (inv.PaidAmount ?? 0)).ToString("C", CultureInfo.CurrentCulture));
                table.AddCell(inv.Status?.ToString() ?? "");
            }

            document.Add(table);
            document.Close();

            return ms.ToArray();
        }
    }
}
