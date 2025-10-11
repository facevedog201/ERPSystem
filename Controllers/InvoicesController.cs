using ERPSystem.Data;
using ERPSystem.Helpers;
using ERPSystem.Models;
using ERPSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Reporting.NETCore;
using ERPSystem.ViewModels;
using System.Data;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


namespace ERPSystem.Controllers
{
    [Authorize]
    public class InvoicesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;

        public InvoicesController(AppDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // LISTAR FACTURAS
        public IActionResult Index(string search, string estado)
        {
            if (User.IsInRole("Recepcion") || User.IsInRole("Vendedor") || User.IsInRole("Asistente"))
                return RedirectToAction("Create");

            ViewBag.Search = search;
            ViewBag.SelectedEstado = estado ?? "All";

            var invoices = _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.InvoiceDetails).ThenInclude(d => d.Service)
                .Include(i => i.Payments)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                invoices = invoices.Where(i =>
                    i.InvoiceId.ToString().Contains(search) ||
                    i.Client.Name.Contains(search));
            }

            if (!string.IsNullOrEmpty(estado) && estado != "All")
            {
                if (Enum.TryParse<InvoiceStatus>(estado, out var estadoEnum))
                    invoices = invoices.Where(i => i.Status == (int)estadoEnum);
            }

            return View(invoices.ToList());
        }
        public IActionResult Details(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.InvoiceDetails).ThenInclude(d => d.Service)
                .Include(i => i.Payments)
                .FirstOrDefault(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound();

            return View(invoice); // ✅ devolvemos un objeto Invoice
        }


        //// DETALLES
        //public async Task<IActionResult> Details(int id)
        //{
        //    var datos = await _context.vw_FacturaCompleta
        //        .Where(v => v.InvoiceId == id)
        //        .ToListAsync();

        //    if (datos == null || !datos.Any())
        //        return NotFound();

        //    return View("Details", datos);
        //}

        public async Task<IActionResult> Print(int id)
        {
            var datos = await _context.vw_FacturaCompleta
                .Where(v => v.InvoiceId == id)
                .ToListAsync();

            if (datos == null || !datos.Any())
                return NotFound();

            // Generar RDLC / PDF a partir del dataset
            return View("FacturaReport", datos);
        }

        // CREAR
        [RoleAuthorize("Admin", "Contador", "Recepcion", "Vendedor", "Asistente")]
        public IActionResult Create()
        {
            ViewBag.Clients = _context.Clients.ToList();
            ViewBag.Services = _context.Services.Where(s => s.IsActive).ToList();
            return View(new Invoice { InvoiceDetails = new List<InvoiceDetail>() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice, List<InvoiceDetail> details)
        {
            if (!ModelState.IsValid)
                return View(invoice);

            // Guardar factura y detalles
            invoice.CreatedAt = DateTime.Now;
            invoice.InvoiceDate = DateTime.Now;

            _context.Add(invoice);
            await _context.SaveChangesAsync();

            foreach (var detail in details)
            {
                detail.InvoiceId = invoice.InvoiceId;
                _context.InvoiceDetails.Add(detail);
            }

            await _context.SaveChangesAsync();

            // Actualizar totales y convertir a letras
            invoice.SubTotal = details.Sum(d => d.Quantity * d.Price);
            invoice.TaxAmount = (invoice.SubTotal * 0.15M); // Ejemplo de IVA
            invoice.Total = invoice.SubTotal + invoice.TaxAmount;
            invoice.AmountInWords = invoice.NumberToWords(invoice.Total ?? 0m); //El operador ?? asegura que no se pase un valor nulo y si es nulo se pasa 0m que significa 0 decimal
            await _context.SaveChangesAsync();

            // Ahora, en lugar de ir a Details → generar reporte o imprimir
            return RedirectToAction("Print", new { id = invoice.InvoiceId });
        }

        // EDITAR
        [RoleAuthorize("Admin", "Contador")]
        public IActionResult Edit(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.InvoiceDetails)
                .FirstOrDefault(i => i.InvoiceId == id);
            if (invoice == null) return NotFound();

            ViewBag.Clients = _context.Clients.ToList();
            ViewBag.Services = _context.Services.ToList();
            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Contador")]
        public IActionResult Edit(int invoiceId, int clientId, int[] serviceIds, int[] quantities)
        {
            var invoice = _context.Invoices
                .Include(i => i.InvoiceDetails)
                .FirstOrDefault(i => i.InvoiceId == invoiceId);
            if (invoice == null) return NotFound();

            invoice.ClientId = clientId;
            invoice.InvoiceDetails.Clear();

            for (int i = 0; i < serviceIds.Length; i++)
            {
                int qty = quantities[i];
                if (qty <= 0) continue;

                var service = _context.Services.Find(serviceIds[i]);
                if (service != null)
                {
                    invoice.InvoiceDetails.Add(new InvoiceDetail
                    {
                        ServiceId = service.ServiceId,
                        Quantity = qty,
                        Price = service.Price
                    });
                }
            }

            invoice.Total = invoice.InvoiceDetails.Sum(d => d.Quantity * d.Price);
            invoice.UpdateStatus();
            _context.SaveChanges();

            _auditService.Log("Edit", "Invoice", invoice.InvoiceId,
                $"Se editó la factura #{invoice.InvoiceId}");

            return RedirectToAction("Index");
        }

        // ANULAR FACTURA
        [RoleAuthorize("Admin", "Contador")]
        public IActionResult Cancel(int id)
        {
            var invoice = _context.Invoices.FirstOrDefault(i => i.InvoiceId == id);
            if (invoice == null) return NotFound();

            invoice.Status = (int?)InvoiceStatus.Cancelled;
            _context.SaveChanges();

            _auditService.Log("Cancel", "Invoice", invoice.InvoiceId,
                $"Se anuló la factura #{invoice.InvoiceId}");

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AddPayment(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.InvoiceDetails).ThenInclude(d => d.Service)
                .Include(i => i.Payments)
                .FirstOrDefault(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound();

            return View(invoice); // 👈 Esto carga tu vista AddPayment.cshtml
        }

        // AGREGAR PAGO
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Contador", "Recepcion", "Vendedor", "Asistente")]
        public IActionResult AddPayment(int invoiceId, decimal amount, string notes)
        {
            var invoice = _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefault(i => i.InvoiceId == invoiceId);

            if (invoice == null) return NotFound();

            invoice.UpdateStatus();

            if ((InvoiceStatus?)invoice.Status == InvoiceStatus.Paid || (InvoiceStatus?)invoice.Status == InvoiceStatus.Cancelled)
            {
                TempData["Error"] = "Factura ya pagada o anulada, no se pueden registrar más pagos.";
                return RedirectToAction("Details", new { id = invoiceId });
            }

            var remaining = invoice.Total - invoice.PaidAmount;
            if (amount <= 0)
            {
                TempData["Error"] = "El monto debe ser mayor a 0.";
                return RedirectToAction("Details", new { id = invoiceId });
            }

            if (amount > remaining)
            {
                TempData["Error"] = $"El monto excede el saldo pendiente ({remaining:C}).";
                return RedirectToAction("Details", new { id = invoiceId });
            }

            var payment = new Payment
            {
                InvoiceId = invoiceId,
                Amount = amount,
                Notes = notes
            };

            _context.Payments.Add(payment);
            invoice.PaidAmount += amount;
            invoice.UpdateStatus();
            _context.SaveChanges();

            TempData["Success"] = $"Pago de {amount:C} registrado correctamente.";
            return RedirectToAction("Details", new { id = invoiceId });
        }
    }
}