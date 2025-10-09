using ERPSystem.Data;
using ERPSystem.Models;
using ERPSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.SqlTypes;


namespace ERPSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ExportService _export;
        private readonly AuditService _auditService;
        private readonly ILogger<ReportsController> _logger; // ✅ Declara el logger

        public ReportsController(AppDbContext context, ExportService export, AuditService auditService, ILogger<ReportsController> logger)
        {
            _context = context;
            _export = export;
            _auditService = auditService;
            _logger = logger;
        }

        // Dashboard general
        public IActionResult Index(DateTime? from, DateTime? to, int? clientId)
        {
            var end = to?.Date.AddDays(1).AddTicks(-1) ?? DateTime.Now;
            var start = from?.Date ?? end.AddMonths(-11).Date; // últimos 12 meses por defecto

            var invoicesQuery = _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.InvoiceDetails)
                .Where(i => i.InvoiceDate >= start && i.InvoiceDate <= end);

            if (clientId.HasValue && clientId.Value > 0)
                invoicesQuery = invoicesQuery.Where(i => i.ClientId == clientId.Value);

            var invoices = invoicesQuery.ToList();

            // KPIs
            var totalFacturado = invoices.Sum(i => i.Total);
            var totalCobrado = invoices.Sum(i => i.PaidAmount);
            var totalPendiente = totalFacturado - totalCobrado;
            var countInvoices = invoices.Count;
            var activeClients = _context.Clients.Count();

            // Sales by month (last 12 months window)
            var salesByMonth = invoices
                .GroupBy(i => new { Year = i.InvoiceDate.Year, Month = i.InvoiceDate.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Total = g.Sum(x => x.Total) })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            // Status summary
            var statusSummary = invoices
                .GroupBy(i => i.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToList();

            // Top clients
            var topClients = invoices
                .GroupBy(i => i.Client.Name)
                .Select(g => new { Client = g.Key, Total = g.Sum(x => x.Total) })
                .OrderByDescending(x => x.Total)
                .Take(10)
                .ToList();

            ViewBag.From = start;
            ViewBag.To = end;
            ViewBag.ClientId = clientId ?? 0;
            ViewBag.TotalFacturado = totalFacturado;
            ViewBag.TotalCobrado = totalCobrado;
            ViewBag.TotalPendiente = totalPendiente;
            ViewBag.CountInvoices = countInvoices;
            ViewBag.ActiveClients = activeClients;
            ViewBag.SalesByMonth = salesByMonth;
            ViewBag.StatusSummary = statusSummary;
            ViewBag.TopClients = topClients;
            ViewBag.Clients = _context.Clients.OrderBy(c => c.Name).ToList();

            return View();
        }

        // Sales detail + export
        public IActionResult Sales(DateTime? from, DateTime? to, int? clientId, string export)
        {
            var end = to?.Date.AddDays(1).AddTicks(-1) ?? DateTime.Now;
            var start = from?.Date ?? end.AddMonths(-11).Date;

            var query = _context.Invoices.Include(i => i.Client)
                        .Where(i => i.InvoiceDate >= start && i.InvoiceDate <= end);

            if (clientId.HasValue && clientId.Value > 0)
                query = query.Where(i => i.ClientId == clientId.Value);

            var list = query.OrderByDescending(i => i.InvoiceDate).ToList();

            if (!string.IsNullOrEmpty(export))
            {
                if (export == "excel")
                {
                    var bytes = _export.ExportSalesToExcel(list, "Ventas");
                    return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ventas_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx");
                }
                else if (export == "pdf")
                {
                    var bytes = _export.ExportInvoicesToPdf(list, $"Ventas {start:yyyy-MM-dd} - {end:yyyy-MM-dd}");
                    return File(bytes, "application/pdf", $"ventas_{start:yyyyMMdd}_{end:yyyyMMdd}.pdf");
                }
            }

            ViewBag.From = start;
            ViewBag.To = end;
            ViewBag.ClientId = clientId ?? 0;
            ViewBag.Clients = _context.Clients.OrderBy(c => c.Name).ToList();
            return View(list);
        }

        // Clients report
        public IActionResult Clients(DateTime? from, DateTime? to, string export)
        {
            var end = to?.Date.AddDays(1).AddTicks(-1) ?? DateTime.Now;
            var start = from?.Date ?? end.AddYears(-1).Date;

            var data = _context.Invoices
                        .Include(i => i.Client)
                        .Where(i => i.InvoiceDate >= start && i.InvoiceDate <= end)
                        .GroupBy(i => i.Client.Name)
                        .Select(g => new
                        {
                            Client = g.Key,
                            Count = g.Count(),
                            Total = g.Sum(i => i.Total)
                        })
                        .OrderByDescending(x => x.Total)
                        .ToList();

            if (!string.IsNullOrEmpty(export) && export == "excel")
            {
                var rows = data.Select(d => (d.Client, d.Count, d.Total));
                var bytes = _export.ExportClientsToExcel(rows, "Clientes");
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"clientes_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx");
            }

            ViewBag.From = start;
            ViewBag.To = end;
            ViewBag.Data = data;
            return View();
        }

        // Services report
        public IActionResult ServicesReport(DateTime? from, DateTime? to, string export)
        {
            var end = to?.Date.AddDays(1).AddTicks(-1) ?? DateTime.Now;
            var start = from?.Date ?? end.AddYears(-1).Date;

            var data = _context.InvoiceDetails
                        .Include(d => d.Service)
                        .Include(d => d.Invoice)
                        .Where(d => d.Invoice.InvoiceDate >= start && d.Invoice.InvoiceDate <= end)
                        .GroupBy(d => d.Service.Name)
                        .Select(g => new
                        {
                            Service = g.Key,
                            Quantity = g.Sum(x => x.Quantity),
                            Total = g.Sum(x => x.Quantity * x.Price)
                        })
                        .OrderByDescending(x => x.Quantity)
                        .ToList();

            if (!string.IsNullOrEmpty(export) && export == "excel")
            {
                var rows = data.Select(d => (d.Service, d.Quantity, d.Total));
                var bytes = _export.ExportServicesToExcel(rows, "Servicios");
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"servicios_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx");
            }

            ViewBag.From = start;
            ViewBag.To = end;
            ViewBag.Data = data;
            return View();
        }

        // Payments report
        public IActionResult Payments(DateTime? from, DateTime? to, string export)
        {
            try
            {
                var data = _context.Payments
         .Include(p => p.Invoice)
             .ThenInclude(i => i.Client)
         .Select(p => new
         {
             p.PaymentId,
             PaymentDate = p.PaymentDate,
             Amount = p.Amount,
             Notes = p.Notes == null ? "" : p.Notes, // Evita null
             InvoiceId = p.InvoiceId,
             ClientName = p.Invoice != null && p.Invoice.Client != null
                 ? (p.Invoice.Client.Name ?? "(Sin cliente)")
                 : "(Sin cliente)",
             InvoiceDate = p.Invoice != null
                 ? (DateTime?)p.Invoice.InvoiceDate
                 : null
         })
         .AsEnumerable() // <- 👈 Esto fuerza que EF lea los datos ANTES de aplicar el Select.
         .Where(p => p.PaymentDate != null) // Opcional
         .ToList();

                return View(data);


                if (export == "excel")
                {
                    using var workbook = new ClosedXML.Excel.XLWorkbook();
                    var ws = workbook.Worksheets.Add("Pagos");

                    ws.Cell(1, 1).Value = "ID Pago";
                    ws.Cell(1, 2).Value = "Fecha";
                    ws.Cell(1, 3).Value = "Cliente";
                    ws.Cell(1, 4).Value = "Factura ID";
                    ws.Cell(1, 5).Value = "Monto";
                    ws.Cell(1, 6).Value = "Notas";

                    int row = 2;
                    foreach (var p in data)
                    {
                        ws.Cell(row, 1).Value = p.PaymentId;
                        ws.Cell(row, 2).Value = p.PaymentDate.ToString("dd/MM/yyyy");
                        ws.Cell(row, 3).Value = p.ClientName;
                        ws.Cell(row, 4).Value = p.InvoiceId;
                        ws.Cell(row, 5).Value = p.Amount;
                        ws.Cell(row, 6).Value = p.Notes;
                        row++;
                    }

                    using var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Pagos_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }

                ViewBag.From = from;
                ViewBag.To = to;
                return View(data);
            }
            catch (SqlNullValueException sqlEx)
            {
                _logger.LogError(sqlEx, "Campo con valor NULL detectado en Payments");
                return View("Error", new ErrorViewModel { Message = "Un campo del reporte contiene valores nulos no manejados." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general en Payments report");
                return View("Error", new ErrorViewModel { Message = "Ocurrió un error al generar el reporte de pagos." });
            }
        }
    }
}
